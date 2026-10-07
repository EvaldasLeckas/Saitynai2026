using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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

    private static readonly Expression<Func<User, UserDto>> ToDto =
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

    public async Task<UserDto?> GetByIdAsync(long id)
    {
        return await _context.Users
            .Where(u => u.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    // Returns null if saving failed
    public async Task<UserDto?> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Bio = dto.Bio,
            Email = dto.Email,
            BirthDate = dto.BirthDate
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return await GetByIdAsync(user.Id);
    }

    // Returns false if not found
    public async Task<bool> UpdateAsync(long id, CreateUserDto dto)
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
    public async Task<bool> DeleteAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}