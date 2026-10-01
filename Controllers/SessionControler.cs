using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Saitynai.DTO;
using System.Text.Json;


[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly AppDbContext _context;

    public SessionController(AppDbContext context)
    {
        _context = context;
    }


 // GET: api/Session
[HttpGet(Name = "GetSessions")]
public async Task<ActionResult> GetSessions(
    [FromQuery] PageParameters pageParameters,
    [FromQuery] SessionFilterParameters filter,
    LinkGenerator linkGenerator)
{
    var sessions = _context.Sessions.AsQueryable();

    // Filtering
    if (!string.IsNullOrWhiteSpace(filter.City))
    {
        sessions = sessions.Where(s =>
            s.City == filter.City);
    }

    if (filter.MinPlayers.HasValue)
    {
        sessions = sessions.Where(s =>
            s.PlayerCount >= filter.MinPlayers.Value);
    }

    if (filter.MaxPlayers.HasValue)
    {
        sessions = sessions.Where(s =>
            s.PlayerCount <= filter.MaxPlayers.Value);
    }

    if (filter.StartDateFrom.HasValue)
    {
        sessions = sessions.Where(s =>
            s.StartDate >= filter.StartDateFrom.Value);
    }

    if (filter.StartDateTo.HasValue)
    {
        sessions = sessions.Where(s =>
            s.StartDate <= filter.StartDateTo.Value);
    }

    // Map to DTO
    var sessionDtos = sessions
        .Select(s => new SessionDto
        {
            Id = s.Id,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            PlayerCount = s.PlayerCount,
            City = s.City,
            Adress = s.Adress,
            Description = s.Description,
            UserId = s.UserId
        })
        .OrderBy(s => s.StartDate);

    // Pagination
    var pagedSessions = await PagedList<SessionDto>.CreateAsync(
        sessionDtos,
        pageParameters.PageNumber!.Value,
        pageParameters.PageSize!.Value);

    // HATEOAS links for each session
    var resources = pagedSessions.Select(session =>
    {
        var links = CreateLinksForSingleSession(
            session.Id,
            linkGenerator,
            HttpContext
        ).ToArray();

        return new ResourceDto<SessionDto>(
            session,
            links);
    }).ToList();

    // Pagination links
    var links = CreateLinksForSessions(
        linkGenerator,
        HttpContext,
        pagedSessions.GetPreviousPageLink(
            linkGenerator,
            HttpContext,
            "GetSessions"),
        pagedSessions.GetNextPageLink(
            linkGenerator,
            HttpContext,
            "GetSessions")
    ).ToArray();

    // Pagination metadata
    var paginationMetadata =
        pagedSessions.CreatePaginationMetadata(
            linkGenerator,
            HttpContext,
            "GetSessions");

    HttpContext.Response.Headers.Append(
        "Pagination",
        JsonSerializer.Serialize(paginationMetadata));

    return Ok(new
    {
        resources,
        links
    });
}

    // GET: api/sessions/5
    [HttpGet("{id:long}", Name = "GetSession")]
    public async Task<ActionResult<SessionDto>> GetSession(long id)
    {
        var session = await _context.Sessions.FindAsync(id);

        if (session == null)
        {
            return NotFound();
        }

        return Ok(session);
    }

    // POST: api/session
[HttpPost(Name = "CreateSession")]
public async Task<ActionResult> CreateSession(CreateSessionDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        var session = new Session
        {
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description,
            PlayerCount = dto.PlayerCount,
            City = dto.City,
            Adress = dto.Adress,
            UserId = dto.UserId
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSession),
            new { id = session.Id },
            session
        );
    }

    // PUT: api/sessions/5
    [HttpPut("{id:long}",  Name = "UpdateSession")]
    public async Task<IActionResult> UpdateSession(long id, CreateSessionDto dto)
    {
        var existingsession = await _context.Sessions.FindAsync(id);

        if (existingsession == null)
        {
            return NotFound();
        }
        var user = await _context.Users.FindAsync(dto.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        existingsession.StartDate = dto.StartDate;
        existingsession.EndDate = dto.EndDate;
        existingsession.PlayerCount = dto.PlayerCount;
        existingsession.City = dto.City;
        existingsession.Adress = dto.Adress;
        existingsession.Description = dto.Description;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/sessions/5
    [HttpDelete("{id:long}",  Name = "RemoveSession")]
    public async Task<IActionResult> DeleteSession(long id)
    {
        var session = await _context.Sessions.FindAsync(id);

        if (session == null)
        {
            return NotFound();
        }

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    
static IEnumerable<LinkDto> CreateLinksForSingleSession(long sessionId, LinkGenerator linkGenerator, HttpContext httpContext)
{
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetSession", new {id = sessionId}), "self", "GET");
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "UpdateSession", new {id = sessionId}), "edit", "PUT");
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "RemoveSession", new {id = sessionId}), "remove", "DELETE");

}
static IEnumerable<LinkDto> CreateLinksForSessions(LinkGenerator linkGenerator, HttpContext httpContext, string? previousPageLink, string? nextPageLink)
{
    yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetSessions"), "self", "GET");
    
    if (previousPageLink != null)
        {
            yield return new LinkDto(previousPageLink, "previousPage", "GET");
        }
    if (nextPageLink != null)
        {
            yield return new LinkDto(nextPageLink, "nextPage", "GET");

        }

}


}