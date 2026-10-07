using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;

namespace Saitynai.Services;

public class UserResultService
{
    private readonly AppDbContext _context;

    public UserResultService(AppDbContext context)
    {
        _context = context;
    }

    private static readonly Expression<Func<UserResult, UserResultDto>> ToDto =
        u => new UserResultDto
        {
            Id = u.Id,
            Score = u.Score,
            Placement = u.Placement,
            GameId = u.GameId,
            UserId = u.UserId
        };

    public async Task<PagedList<UserResultDto>> GetAllAsync(
        PageParameters pageParameters,
        UserResultFilterParameters filter)
    {
        var results = _context.UserResults.AsQueryable();

        if (filter.MinScore.HasValue)
            results = results.Where(r => r.Score >= filter.MinScore.Value);

        if (filter.MaxScore.HasValue)
            results = results.Where(r => r.Score <= filter.MaxScore.Value);

        if (filter.Placement.HasValue)
            results = results.Where(r => r.Placement == filter.Placement.Value);

        var dtos = results.Select(ToDto).OrderBy(u => u.Score);

        return await PagedList<UserResultDto>.CreateAsync(
            dtos,
            pageParameters.PageNumber!.Value,
            pageParameters.PageSize!.Value);
    }

    public async Task<UserResultDto?> GetByIdAsync(long id)
    {
        return await _context.UserResults
            .Where(u => u.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    // Returns null if the game was not found in that session
    public async Task<List<UserResultDto>?> GetForGameInSessionAsync(long sessionId, long gameId)
    {
        var gameExists = await _context.Games
            .AnyAsync(g => g.Id == gameId && g.SessionId == sessionId);

        if (!gameExists) return null;

        return await _context.UserResults
            .Where(r => r.GameId == gameId)
            .Select(ToDto)
            .OrderBy(r => r.Placement)
            .ToListAsync();
    }

    // Returns (result, null) on success, or (null, errorMessage) on failure
    public async Task<(UserResultDto? Result, string? Error)> CreateAsync(CreateUserResultDto dto)
    {
        var error = await CheckReferencesAsync(dto);
        if (error != null) return (null, error);

        var userResult = new UserResult
        {
            Score = dto.Score,
            Placement = dto.Placement,
            GameId = dto.GameId,
            UserId = dto.UserId
        };

        _context.UserResults.Add(userResult);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(userResult.Id), null);
    }

    // Returns null on success, or an error message
    public async Task<string?> UpdateAsync(long id, CreateUserResultDto dto)
    {
        var userResult = await _context.UserResults.FindAsync(id);
        if (userResult == null) return "User result not found.";

        var error = await CheckReferencesAsync(dto);
        if (error != null) return error;

        userResult.Score = dto.Score;
        userResult.Placement = dto.Placement;
        userResult.GameId = dto.GameId;
        userResult.UserId = dto.UserId;

        await _context.SaveChangesAsync();
        return null;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var userResult = await _context.UserResults.FindAsync(id);
        if (userResult == null) return false;

        _context.UserResults.Remove(userResult);
        await _context.SaveChangesAsync();
        return true;
    }

    // Shared by create and update
    private async Task<string?> CheckReferencesAsync(CreateUserResultDto dto)
    {
        if (!await _context.Games.AnyAsync(g => g.Id == dto.GameId))
            return "Game not found.";

        if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            return "User not found.";

        return null;
    }
}