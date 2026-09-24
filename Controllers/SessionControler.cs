using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Saitynai.DTO;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly AppDbContext _context;

    public SessionController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/sessions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Session>>> Getsessions()
    {
        var sessions = await _context.Sessions.ToListAsync();

        return Ok(sessions
);
    }

    // GET: api/sessions/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Session>> Getsession(long id)
    {
        var session = await _context.Sessions.FindAsync(id);

        if (session == null)
        {
            return NotFound();
        }

        return Ok(session);
    }

    // POST: api/session
    [HttpPost]
    public async Task<ActionResult<Session>> AddSession(CreateSessionDto dto)
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
            PlayerCount = dto.PlayerCount,
            City = dto.City,
            Adress = dto.Adress,
            Description = dto.Description,
            UserId = dto.UserId,
            User = user
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Getsession),
            new { id = session.Id },
            session
        );
    }

    // PUT: api/sessions/5
    [HttpPut("{id:long}")]
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
    [HttpDelete("{id:long}")]
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
}