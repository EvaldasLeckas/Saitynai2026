using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saitynai.DTO;
using Saitynai.Models;
using Saitynai.Services;
using System.Text.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Saitynai.Auth;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly SessionService _service;
    private readonly IValidator<CreateSessionDto> _validator;
    private readonly UserResultService _resultsService;
    private readonly UserManager<AppUser> _userManager;



    public SessionController(SessionService service, UserResultService resultsService, IValidator<CreateSessionDto> validator, UserManager<AppUser> userManage)
    {
        _service = service;
        _validator = validator;
        _resultsService = resultsService;
        _userManager = userManage;

    }
    private string? CurrentUserId =>
    User.FindFirstValue(JwtRegisteredClaimNames.Sub)
    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<ActionResult?> CheckOwnershipAsync(long id)
    {
        var userId = CurrentUserId;

        if (userId == null)
            return Unauthorized();

        var ownerId = await _service.GetOwnerIdAsync(id);

        if (ownerId == null)
            return NotFound();

        if (ownerId != userId && !User.IsInRole(AppRoles.Admin))
            return Forbid();

        return null;
    }

    // GET: api/Session
    [HttpGet(Name = "GetSessions")]
    public async Task<ActionResult> GetSessions(
        [FromQuery] PageParameters pageParameters,
        [FromQuery] SessionFilterParameters filter,
        LinkGenerator linkGenerator)
    {
        var pagedSessions = await _service.GetAllAsync(pageParameters, filter);

        var resources = pagedSessions.Select(session =>
            new ResourceDto<SessionDto>(
                session,
                CreateLinksForSingleSession(session.Id, linkGenerator, HttpContext).ToArray()
            )).ToList();

        var links = CreateLinksForSessions(
            linkGenerator,
            HttpContext,
            pagedSessions.GetPreviousPageLink(linkGenerator, HttpContext, "GetSessions"),
            pagedSessions.GetNextPageLink(linkGenerator, HttpContext, "GetSessions")
        ).ToArray();

        var paginationMetadata = pagedSessions.CreatePaginationMetadata(
            linkGenerator, HttpContext, "GetSessions");

        Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(new { resources, links });
    }

    // GET: api/Session/my
    [HttpGet("my", Name = "GetMySessions")]
    public async Task<ActionResult> GetMySessions(
        [FromQuery] PageParameters pageParameters,
        [FromQuery] SessionFilterParameters filter,
        LinkGenerator linkGenerator)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        filter.CreatedByUserId = userId;   // 1. force the filter to the logged-in user

        var pagedSessions = await _service.GetAllAsync(pageParameters, filter);

        var resources = pagedSessions.Select(session =>
            new ResourceDto<SessionDto>(
                session,
                CreateLinksForSingleSession(session.Id, linkGenerator, HttpContext).ToArray()
            )).ToList();

        var links = CreateLinksForSessions(
            linkGenerator,
            HttpContext,
            pagedSessions.GetPreviousPageLink(linkGenerator, HttpContext, "GetMySessions"),  // 2.
            pagedSessions.GetNextPageLink(linkGenerator, HttpContext, "GetMySessions")       // 2.
        ).ToArray();

        var paginationMetadata = pagedSessions.CreatePaginationMetadata(
            linkGenerator, HttpContext, "GetMySessions");                                    // 3.

        Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(new { resources, links });
    }
    

    // GET: api/Session/5
    [HttpGet("{id:long}", Name = "GetSession")]
    public async Task<ActionResult<SessionDto>> GetSession(long id)
    {
        var session = await _service.GetByIdAsync(id);
        return session == null ? NotFound() : Ok(session);
    }

    // POST: api/Session
    [HttpPost(Name = "CreateSession")]
    [Authorize]
    public async Task<ActionResult<SessionDto>> CreateSession(
        CreateSessionDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));

        var userId = CurrentUserId;

        if (userId == null)
            return Unauthorized();

        var created = await _service.CreateAsync(dto, userId);

        if (created == null)
            return BadRequest(new
            {
                message = "Unable to create the session."
            });

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return Unauthorized();

        if (!await _userManager.IsInRoleAsync(user, AppRoles.LobbyOwner))
        {
            var roleResult = await _userManager.AddToRoleAsync(
                user, AppRoles.LobbyOwner);

            if (!roleResult.Succeeded)
            {
                return Problem(
                    "The session was created, but the owner role could not be assigned.");
            }
        }

        return CreatedAtAction(
            nameof(GetSession),
            new { id = created.Id },
            created);
    }

    // PUT: api/Session/5
    [HttpPut("{id:long}", Name = "UpdateSession")]
    [Authorize(Roles = "Admin, LobbyOwner")]


    public async Task<ActionResult<SessionDto>> UpdateSession(
        long id, CreateSessionDto dto)
    {
        var ownershipCheck = await CheckOwnershipAsync(id);
        var validation = await _validator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));

        if (ownershipCheck != null)
            return ownershipCheck;



        var success = await _service.UpdateAsync(id, dto);

        if (success == null)
            return NotFound();

        return Ok(dto);
    }

    [HttpDelete("{id:long}", Name = "RemoveSession")]
    [Authorize(Roles = "Admin, LobbyOwner")]

    public async Task<IActionResult> DeleteSession(long id)
    {
        var ownershipCheck = await CheckOwnershipAsync(id);

        if (ownershipCheck != null)
            return ownershipCheck;

        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:long}/game/{gameId:long}/results", Name = "GetGameResultsForSession")]
    public async Task<ActionResult<IEnumerable<UserResultDto>>> GetGameResultsForSession(long gameId, long id)
    {
        var results = await _resultsService.GetForGameInSessionAsync(id, gameId);

        if (results == null)
            return NotFound(new { message = "Game was not found in the specified session." });

        return Ok(results);
    }

    // HATEOAS links
    static IEnumerable<LinkDto> CreateLinksForSingleSession(
        long sessionId,
        LinkGenerator linkGenerator,
        HttpContext httpContext)
    {
        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "GetSession", new { id = sessionId }),
            "self", "GET");

        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "UpdateSession", new { id = sessionId }),
            "edit", "PUT");

        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "RemoveSession", new { id = sessionId }),
            "remove", "DELETE");
    }

    static IEnumerable<LinkDto> CreateLinksForSessions(
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        string? previousPageLink,
        string? nextPageLink)
    {
        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "GetSessions"),
            "self", "GET");

        if (previousPageLink != null)
            yield return new LinkDto(previousPageLink, "previousPage", "GET");

        if (nextPageLink != null)
            yield return new LinkDto(nextPageLink, "nextPage", "GET");
    }
}