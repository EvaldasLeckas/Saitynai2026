using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;

[ApiController]
[Route("api/session-participants")]
public class SessionParticipantController : ControllerBase
{
    private readonly AppDbContext _context;

    public SessionParticipantController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/session-participants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionParticipantDto>>>
    GetParticipants()
    {
        var participants = await _context.SessionParticipants
            .Select(sp => new SessionParticipantDto
            {
                SessionId = sp.SessionId,
                UserId = sp.UserId
            })
            .ToListAsync();

        return Ok(participants);
    }

    // GET:
    // api/session-participants/session/5
    [HttpGet("session/{sessionId:long}")]
    public async Task<ActionResult<IEnumerable<SessionParticipantDto>>>
        GetSessionParticipants(long sessionId)
    {
        var sessionExists = await _context.Sessions
            .AnyAsync(s => s.Id == sessionId);

        if (!sessionExists)
        {
            return NotFound(new
            {
                message = "Session not found."
            });
        }

        var participants = await _context.SessionParticipants
            .Where(sp => sp.SessionId == sessionId)
            .Select(sp => new SessionParticipantDto
            {
                SessionId = sp.SessionId,
                UserId = sp.UserId
            })
            .ToListAsync();

        return Ok(participants);
    }

    // POST:
    // api/session-participants/session/5
    [HttpPost("session/{sessionId:long}")]
    public async Task<ActionResult<SessionParticipantDto>>
        AddParticipant(
            long sessionId,
            CreateSessionParticipantDto dto)
    {
        var session = await _context.Sessions
            .FindAsync(sessionId);

        if (session == null)
        {
            return NotFound(new
            {
                message = "Session not found."
            });
        }

        var user = await _context.Users
            .FindAsync(dto.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        var alreadyParticipant =
            await _context.SessionParticipants.AnyAsync(
                sp => sp.SessionId == sessionId &&
                      sp.UserId == dto.UserId);

        if (alreadyParticipant)
        {
            return Conflict(new
            {
                message =
                    "User is already a participant of this session."
            });
        }

        var participant = new SessionParticipant
        {
            SessionId = sessionId,
            UserId = dto.UserId
        };

        _context.SessionParticipants.Add(participant);
        await _context.SaveChangesAsync();

        var participantDto = new SessionParticipantDto
        {
            SessionId = participant.SessionId,
            UserId = participant.UserId
        };

        return Ok(participantDto);
    }

    // DELETE:
    // api/session-participants/session/5/user/3
    [HttpDelete("session/{sessionId:long}/user/{userId:long}")]
    public async Task<IActionResult> RemoveParticipant(
        long sessionId,
        long userId)
    {
        var participant = await _context.SessionParticipants
            .FirstOrDefaultAsync(
                sp => sp.SessionId == sessionId &&
                      sp.UserId == userId);

        if (participant == null)
        {
            return NotFound();
        }

        _context.SessionParticipants.Remove(participant);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}