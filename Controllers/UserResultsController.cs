using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Saitynai.DTO;
using Saitynai.Services;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class UserResultController : ControllerBase
{
    private readonly UserResultService _service;
    private readonly IValidator<CreateUserResultDto> _validator;

    public UserResultController(
        UserResultService service,
        IValidator<CreateUserResultDto> validator)
    {
        _service = service;
        _validator = validator;
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
    public async Task<ActionResult<UserResultDto>> AddUserResult(CreateUserResultDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var (result, error) = await _service.CreateAsync(dto);

        if (error != null)
            return NotFound(new { message = error });

        return CreatedAtAction(nameof(GetUserResult), new { id = result!.Id }, result);
    }

    // PUT: api/UserResult/5
    [HttpPut("{id:long}", Name = "UpdateUserResult")]
    public async Task<IActionResult> UpdateUserResult(long id, CreateUserResultDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var error = await _service.UpdateAsync(id, dto);

        return error == null ? NoContent() : NotFound(new { message = error });
    }

    // DELETE: api/UserResult/5
    [HttpDelete("{id:long}", Name = "RemoveUserResult")]
    public async Task<IActionResult> DeleteUserResult(long id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }

    // GET: api/UserResult/session/1/game/2/results
    [HttpGet("session/{sessionId:long}/game/{gameId:long}/results", Name = "GetGameResultsForSession")]
    public async Task<ActionResult<IEnumerable<UserResultDto>>> GetGameResultsForSession(
        long sessionId, long gameId)
    {
        var results = await _service.GetForGameInSessionAsync(sessionId, gameId);

        if (results == null)
            return NotFound(new { message = "Game was not found in the specified session." });

        return Ok(results);
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