using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Saitynai.DTO;
using Saitynai.Services;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly SessionService _service;
    private readonly IValidator<CreateSessionDto> _validator;


    public SessionController(SessionService service, IValidator<CreateSessionDto> validator)
    {
        _service = service;
        _validator = validator;

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

    // GET: api/Session/5
    [HttpGet("{id:long}", Name = "GetSession")]
    public async Task<ActionResult<SessionDto>> GetSession(long id)
    {
        var session = await _service.GetByIdAsync(id);
        return session == null ? NotFound() : Ok(session);
    }

    // POST: api/Session
    [HttpPost(Name = "CreateSession")]
    public async Task<ActionResult<SessionDto>> CreateSession(CreateSessionDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var created = await _service.CreateAsync(dto);

        if (created == null)
            return BadRequest(new { message = "Unable to create the session." });

        return CreatedAtAction(nameof(GetSession), new { id = created.Id }, created);
    }

    // PUT: api/Session/5
    [HttpPut("{id:long}", Name = "UpdateSession")]
    public async Task<IActionResult> UpdateSession(long id, CreateSessionDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        return await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();
    }

    // DELETE: api/Session/5
    [HttpDelete("{id:long}", Name = "RemoveSession")]
    public async Task<IActionResult> DeleteSession(long id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
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