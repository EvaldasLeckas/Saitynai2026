using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saitynai.DTO;
using Saitynai.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class UserResultController : ControllerBase
{
    private readonly UserResultService _service;
    private readonly IValidator<CreateUserResultDto> _validator;
    private readonly GameService _gameService;

    public UserResultController(
        UserResultService service,
        IValidator<CreateUserResultDto> validator,
        GameService gameService)
    {
        _service = service;
        _validator = validator;
        _gameService = gameService;
    }

        private string? CurrentUserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<IActionResult?> CheckOwnershipAsync(long gameId)
    {
        var userId = CurrentUserId;

        if (userId == null)
            return Unauthorized();

        var ownerId = await _gameService.GetOwnerIdAsync(gameId);

        if (ownerId == null)
            return NotFound();

        if (ownerId != userId && !User.IsInRole(AppRoles.Admin))
            return Forbid();

        return null;
    }

    // GET: api/UserResult
    [HttpGet(Name = "GetUserResults")]
    public async Task<ActionResult> GetUserResults(
        [FromQuery] PageParameters pageParameters,
        [FromQuery] UserResultFilterParameters filter,
        LinkGenerator linkGenerator)
    {
        var pagedResults = await _service.GetAllAsync(pageParameters, filter);

        var resources = pagedResults.Select(userResult =>
            new ResourceDto<UserResultDto>(
                userResult,
                CreateLinksForSingleUserResult(userResult.Id, linkGenerator, HttpContext).ToArray()
            )).ToList();

        var links = CreateLinksForUserResults(
            linkGenerator,
            HttpContext,
            pagedResults.GetPreviousPageLink(linkGenerator, HttpContext, "GetUserResults"),
            pagedResults.GetNextPageLink(linkGenerator, HttpContext, "GetUserResults")
        ).ToArray();

        var paginationMetadata = pagedResults.CreatePaginationMetadata(
            linkGenerator, HttpContext, "GetUserResults");

        Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(new { resources, links });
    }

    // GET: api/UserResult/5
    [HttpGet("{id:long}", Name = "GetUserResult")]
    public async Task<ActionResult<UserResultDto>> GetUserResult(long id)
    {
        var userResult = await _service.GetByIdAsync(id);
        return userResult == null ? NotFound() : Ok(userResult);
    }

    // POST: api/UserResult
    [HttpPost(Name = "CreateUserResult")]
    [Authorize(Roles = "Admin, LobbyOwner")]

    public async Task<ActionResult<UserResultDto>> AddUserResult(CreateUserResultDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        
        var denied = await CheckOwnershipAsync(dto.GameId);
        if (denied != null)
            return denied is UnauthorizedResult
                ? Unauthorized()
                : denied is NotFoundResult
                    ? NotFound()
                    : Forbid();
        var (result, error) = await _service.CreateAsync(dto);

        if (error != null)
            return NotFound(new { message = error });

        return CreatedAtAction(nameof(GetUserResult), new { id = result!.Id }, result);
    }

    // PUT: api/UserResult/5
    [HttpPut("{id:long}", Name = "UpdateUserResult")]
    [Authorize(Roles = "Admin, LobbyOwner")]


    public async Task<IActionResult> UpdateUserResult(long id, CreateUserResultDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();

        var denied = await CheckOwnershipAsync(existing.GameId);
        if (denied != null) return denied;

        var targetDenied = await CheckOwnershipAsync(dto.GameId); 
        if (targetDenied != null) return targetDenied;
        var error = await _service.UpdateAsync(id, dto);

        return error == null ? Ok(dto) : NotFound(new { message = error });
    }

    // DELETE: api/UserResult/5
    [HttpDelete("{id:long}", Name = "RemoveUserResult")]
    [Authorize(Roles = "Admin, LobbyOwner")]


    public async Task<IActionResult> DeleteUserResult(long id)
    {

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();

        var denied = await CheckOwnershipAsync(existing.GameId);
        if (denied != null) return denied;
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }


    // HATEOAS links
    static IEnumerable<LinkDto> CreateLinksForSingleUserResult(
        long userResultId,
        LinkGenerator linkGenerator,
        HttpContext httpContext)
    {
        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "GetUserResult", new { id = userResultId }),
            "self", "GET");

        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "UpdateUserResult", new { id = userResultId }),
            "edit", "PUT");

        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "RemoveUserResult", new { id = userResultId }),
            "remove", "DELETE");
    }

    static IEnumerable<LinkDto> CreateLinksForUserResults(
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        string? previousPageLink,
        string? nextPageLink)
    {
        yield return new LinkDto(
            linkGenerator.GetUriByName(httpContext, "GetUserResults"),
            "self", "GET");

        if (previousPageLink != null)
            yield return new LinkDto(previousPageLink, "previousPage", "GET");

        if (nextPageLink != null)
            yield return new LinkDto(nextPageLink, "nextPage", "GET");
    }
}