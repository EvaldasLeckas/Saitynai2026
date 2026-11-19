using Microsoft.AspNetCore.Identity;
using Saitynai.Auth;

public class AuthSeeder
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthSeeder(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task SeedAsync()
    {
        await AddDefaultRolesAsync();
        await AddAdminUserAsync();
    }

    private async Task AddAdminUserAsync()
    {
        var user = new AppUser
        {
            UserName = "admin@admin.com",
            Email = "admin@admin.com",
            Name = "Admin",
            Surname = "Test",
            BirthDate = new DateOnly(1990, 1, 1),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, "Admin123!");

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, AppRoles.Admin);
        }
    }
    

    private async Task AddDefaultRolesAsync()
    {
        foreach (var role in AppRoles.All)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}