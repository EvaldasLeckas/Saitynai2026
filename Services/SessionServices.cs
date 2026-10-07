using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;

namespace Saitynai.Services;

public class SessionService
{
    private readonly AppDbContext _context;

    public SessionService(AppDbContext context)
    {
        _context = context;
    }

    // Mapping Session -> SessionDto (written once, reused everywhere)
    private static readonly Expression<Func<Session, SessionDto>> ToDto =
        s => new SessionDto
        {
            Id = s.Id,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            PlayerCount = s.PlayerCount,
            City = s.City,
            Address = s.Address,
            Description = s.Description
        };

    public async Task<PagedList<SessionDto>> GetAllAsync(
        PageParameters pageParameters,
        SessionFilterParameters filter)
    {
        var sessions = _context.Sessions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.City))
            sessions = sessions.Where(s => s.City == filter.City);

        if (filter.MinPlayers.HasValue)
            sessions = sessions.Where(s => s.PlayerCount >= filter.MinPlayers.Value);

        if (filter.MaxPlayers.HasValue)
            sessions = sessions.Where(s => s.PlayerCount <= filter.MaxPlayers.Value);

        if (filter.StartDateFrom.HasValue)
            sessions = sessions.Where(s => s.StartDate >= filter.StartDateFrom.Value);

        if (filter.StartDateTo.HasValue)
            sessions = sessions.Where(s => s.StartDate <= filter.StartDateTo.Value);

        var dtos = sessions.Select(ToDto).OrderBy(s => s.StartDate);

        return await PagedList<SessionDto>.CreateAsync(
            dtos,
            pageParameters.PageNumber!.Value,
            pageParameters.PageSize!.Value);
    }

    public async Task<SessionDto?> GetByIdAsync(long id)
    {
        return await _context.Sessions
            .Where(s => s.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    // Returns null if saving failed
    public async Task<SessionDto?> CreateAsync(CreateSessionDto dto)
    {
        var session = new Session
        {
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description,
            PlayerCount = dto.PlayerCount,
            City = dto.City,
            Address = dto.Address
        };

        try
        {
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return await GetByIdAsync(session.Id);
    }

    // Returns false if not found
    public async Task<bool> UpdateAsync(long id, CreateSessionDto dto)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session == null) return false;

        session.StartDate = dto.StartDate;
        session.EndDate = dto.EndDate;
        session.PlayerCount = dto.PlayerCount;
        session.City = dto.City;
        session.Address = dto.Address;
        session.Description = dto.Description;

        await _context.SaveChangesAsync();
        return true;
    }

    // Returns false if not found
    public async Task<bool> DeleteAsync(long id)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session == null) return false;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }
}