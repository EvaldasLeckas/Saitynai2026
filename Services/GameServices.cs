using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Saitynai.DTO;
using Saitynai.Models;

namespace Saitynai.Services;

public class GameService
{
    private readonly AppDbContext _context;

    public GameService(AppDbContext context)
    {
        _context = context;
    }

    // Mapping Session -> SessionDto (written once, reused everywhere)
    private static readonly Expression<Func<Game, GameDto>> ToDto =
        s => new GameDto
        {
            Id = s.Id,
            Name = s.Name,
            PlayerCount = s.PlayerCount,
            Difficulty = s.Difficulty,
            GameLength = s.GameLength,
            Description = s.Description,
            SessionId = s.SessionId
        };

    public async Task<PagedList<GameDto>> GetAllAsync(
        PageParameters pageParameters,
        GameFilterParameters filter)
    {
        var games = _context.Games.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            games = games.Where(g =>
                g.Name.Contains(filter.Name));
        }

        if (filter.MinPlayers.HasValue)
        {
            games = games.Where(g =>
                g.PlayerCount >= filter.MinPlayers.Value);
        }

        if (filter.MaxPlayers.HasValue)
        {
            games = games.Where(g =>
                g.PlayerCount <= filter.MaxPlayers.Value);
        }

        if (filter.GameLength.HasValue)
        {
            games = games.Where(g =>
                g.GameLength == filter.GameLength.Value);
        }

        if (filter.Difficulty.HasValue)
        {
            games = games.Where(g =>
                g.Difficulty == filter.Difficulty.Value);
        }

        var dtos = games.Select(ToDto).OrderBy(s => s.Name);

        return await PagedList<GameDto>.CreateAsync(
            dtos,
            pageParameters.PageNumber!.Value,
            pageParameters.PageSize!.Value);
    }

    public async Task<GameDto?> GetByIdAsync(long id)
    {
        return await _context.Games
            .Where(s => s.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    // Returns null if saving failed
    public async Task<GameDto?> CreateAsync(CreateGameDto dto)
    {
        var game = new Game
        {
            Name = dto.Name,
            Description = dto.Description,
            PlayerCount = dto.PlayerCount,
            GameLength = dto.GameLength,
            Difficulty = dto.Difficulty,
            SessionId = dto.SessionId
        };

        try
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return await GetByIdAsync(game.Id);
    }

    // Returns false if not found
    public async Task<string?> UpdateAsync(long id, CreateGameDto dto)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null) return "Game not found.";

        if (!await _context.Sessions.AnyAsync(s => s.Id == dto.SessionId))
        return "Session not found.";

            game.Name = dto.Name;
            game.Description = dto.Description;
            game.PlayerCount = dto.PlayerCount;
            game.GameLength = dto.GameLength;
            game.Difficulty = dto.Difficulty;
            game.SessionId = dto.SessionId;

        await _context.SaveChangesAsync();
        return null;
    }

    // Returns false if not found
    public async Task<bool> DeleteAsync(long id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null) return false;

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
        return true;
    }
}