using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Auth;
using Saitynai.DTO;
using Saitynai.Models;
using Saitynai.Services;

[ApiController]
[Route("api/session-participants")]
public class SessionParticipantController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SessionService _sessionService;

    public SessionParticipantController(AppDbContext context, SessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Returns null if the current user owns the session (or is admin)
    private async Task<ActionResult?> CheckOwnerAsync(long sessionId)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        var ownerId = await _sessionService.GetOwnerIdAsync(sessionId);
        if (ownerId == null)
            return NotFound(new { message = "Session not found." });

        if (ownerId != userId && !User.IsInRole(AppRoles.Admin))
            return Forbid();

        return null;
    }

    // GET: api/session-participants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionParticipantDto>>> GetParticipants()
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

    // GET: api/session-participants/session/5
    [HttpGet("session/{sessionId:long}")]
    public async Task<ActionResult<IEnumerable<SessionParticipantDto>>> GetSessionParticipants(long sessionId)
    {
        var sessionExists = await _context.Sessions.AnyAsync(s => s.Id == sessionId);
        if (!sessionExists)
            return NotFound(new { message = "Session not found." });

        var participants = await (
    from sp in _context.SessionParticipants
    where sp.SessionId == sessionId
    join u in _context.Users on sp.UserId equals u.Id
    select new SessionParticipantDto
    {
        SessionId = sp.SessionId,
        UserId = sp.UserId,
        Name = u.Name,
        LastName = u.Surname
    }).ToListAsync();

        return Ok(participants);
    }

    // POST: api/session-participants/session/5/join   (the logged-in user joins)
    [HttpPost("session/{sessionId:long}/join")]
    [Authorize]
    public async Task<IActionResult> Join(long sessionId)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        var already = await _context.SessionParticipants
            .AnyAsync(sp => sp.SessionId == sessionId && sp.UserId == userId);
        if (already)
            return Conflict(new { message = "You already joined this session." });

        var count = await _context.SessionParticipants
            .CountAsync(sp => sp.SessionId == sessionId);
        if (count >= session.PlayerCount)
            return Conflict(new { message = "This session is full." });

        _context.SessionParticipants.Add(
            new SessionParticipant { SessionId = sessionId, UserId = userId });
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/session-participants/session/5/leave   (the logged-in user leaves)
    [HttpDelete("session/{sessionId:long}/leave")]
    [Authorize]
    public async Task<IActionResult> Leave(long sessionId)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        var ownerId = await _sessionService.GetOwnerIdAsync(sessionId);
        if (ownerId == null)
            return NotFound(new { message = "Session not found." });

        if (ownerId == userId)
            return Conflict(new { message = "The owner can't leave. Delete the session instead." });

        var participant = await _context.SessionParticipants
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.UserId == userId);
        if (participant == null)
            return NotFound(new { message = "You are not in this session." });

        _context.SessionParticipants.Remove(participant);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/session-participants/session/5   (owner or admin adds a user)
    [HttpPost("session/{sessionId:long}")]
    [Authorize]
    public async Task<ActionResult<SessionParticipantDto>> AddParticipant(
        long sessionId, CreateSessionParticipantDto dto)
    {
        var denied = await CheckOwnerAsync(sessionId);
        if (denied != null) return denied;

        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        var already = await _context.SessionParticipants
            .AnyAsync(sp => sp.SessionId == sessionId && sp.UserId == dto.UserId);
        if (already)
            return Conflict(new { message = "User is already a participant of this session." });

        var count = await _context.SessionParticipants
            .CountAsync(sp => sp.SessionId == sessionId);
        if (count >= session.PlayerCount)
            return Conflict(new { message = "This session is full." });

        var participant = new SessionParticipant
        {
            SessionId = sessionId,
            UserId = dto.UserId
        };
        _context.SessionParticipants.Add(participant);
        await _context.SaveChangesAsync();

        return Ok(new SessionParticipantDto
        {
            SessionId = participant.SessionId,
            UserId = participant.UserId
        });
    }

    // DELETE: api/session-participants/session/5/user/abc   (owner or admin removes a user)
    [HttpDelete("session/{sessionId:long}/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> RemoveParticipant(long sessionId, string userId)
    {
        var denied = await CheckOwnerAsync(sessionId);
        if (denied != null) return denied;

        var participant = await _context.SessionParticipants
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.UserId == userId);
        if (participant == null)
            return NotFound();

        _context.SessionParticipants.Remove(participant);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}