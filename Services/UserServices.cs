using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Saitynai.Auth;
using Saitynai.DTO;
using Saitynai.Models;

namespace Saitynai.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    private static readonly Expression<Func<AppUser, UserDto>> ToDto =
        u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname,
            Bio = u.Bio,
            Email = u.Email,
            BirthDate = u.BirthDate
        };

    public async Task<PagedList<UserDto>> GetAllAsync(PageParameters pageParameters)
    {
        var users = _context.Users
            .Select(ToDto)
            .OrderBy(u => u.Name);

        return await PagedList<UserDto>.CreateAsync(
            users,
            pageParameters.PageNumber!.Value,
            pageParameters.PageSize!.Value);
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        return await _context.Users
            .Where(u => u.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    // Returns null if saving failed
    // public async Task<UserDto?> CreateAsync(CreateUserDto dto)
    // {
    //     var user = new AppUser
    //     {
    //         Name = dto.Name,
    //         Surname = dto.Surname,
    //         Bio = dto.Bio,
    //         Email = dto.Email,
    //         BirthDate = dto.BirthDate
    //     };

    //     try
    //     {
    //         _context.Users.Add(user);
    //         await _context.SaveChangesAsync();
    //     }
    //     catch (DbUpdateException)
    //     {
    //         return null;
    //     }

    //     return await GetByIdAsync(user.Id);
    // }

    // Returns false if not found
    public async Task<bool> UpdateAsync(string id, CreateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.Name = dto.Name;
        user.Surname = dto.Surname;
        user.Bio = dto.Bio;
        user.Email = dto.Email;
        user.BirthDate = dto.BirthDate;

        await _context.SaveChangesAsync();
        return true;
    }

    // Returns false if not found
    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    // Grąžina null, jei naudotojas nerastas
    public async Task<UserProfileDto?> GetProfileAsync(string id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Name, u.Surname, u.Email, u.Bio, u.BirthDate, u.AvatarUrl})
            .FirstOrDefaultAsync();

        if (user == null) return null;

        var results = _context.UserResults.Where(r => r.UserId == id);

        var gamesPlayed = await results.CountAsync();
        var wins = await results.CountAsync(r => r.Placement == 1);
        var avgPlacement = await results.AverageAsync(r => (double?)r.Placement);

        var sessionsJoined = await _context.SessionParticipants
            .CountAsync(sp => sp.UserId == id);

        var recent = await results
            .OrderByDescending(r => r.Id)
            .Take(5)
            .Select(r => new ProfileResultDto
            {
                GameId = r.GameId,
                GameName = r.Game.Name,
                Score = r.Score,
                Placement = r.Placement
            })
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - user.BirthDate.Year;
        if (user.BirthDate > today.AddYears(-age)) age--;

        return new UserProfileDto
        {
            UserId = user.Id,
            FullName = $"{user.Name} {user.Surname}",
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Age = age,
            GamesPlayed = gamesPlayed,
            Wins = wins,
            WinRate = gamesPlayed == 0 ? 0 : Math.Round(wins * 100.0 / gamesPlayed, 1),
            AveragePlacement = avgPlacement == null ? null : Math.Round(avgPlacement.Value, 2),
            //SessionsJoined = sessionsJoined,
            RecentResults = recent
        };
    }
}