using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Saitynai.Auth;

public static class AuthEndpoints
{
    public static void AddAuthApi(this WebApplication app)
    {
        //register
        app.MapPost("api/register", async (UserManager<AppUser> UserManager, RegisterUserDto dto) =>
        {
            var user = await UserManager.FindByEmailAsync(dto.Email);
            if(user != null)
                return Results.UnprocessableEntity("User is already registered by that email");

            var newUser = new AppUser()
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.LastName,
                BirthDate = dto.BirthDate,
            };


            // wrap in trasaction
            // create and async is not in the same transaction, if creates goes by, but async breaks, user will be created without role (FIX)
            var createUserResult = await UserManager.CreateAsync(newUser, dto.Password);
            if (!createUserResult.Succeeded)
            {
                return Results.UnprocessableEntity();
            }

            await UserManager.AddToRoleAsync(newUser, AppRoles.AppUser);
            return Results.Created();
        });

        //login
        app.MapPost("api/login", async (UserManager<AppUser> UserManager, JwtTokenService jwtTokenService, SessionLoginService sessionService, HttpContext httpContext, LoginUserDto dto) =>
        {
            var user = await UserManager.FindByEmailAsync(dto.Email);
            if(user == null)
                return Results.UnprocessableEntity("User does not exist");
            var isPasswordValid = await UserManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                return Results.UnprocessableEntity("Username or password was incorrect");
            }

            var roles = await UserManager.GetRolesAsync(user);

            var sessionId = Guid.NewGuid();
            var expiresAt = DateTime.UtcNow.AddDays(1);
            var accessToken = jwtTokenService.CreateAccessToken(user.Email, user.Id, roles);
            var refreshToken = jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

            // Kadangi tokenas turi ilga expiracija, jį gėriau yra dėti į HttpCookies, kurio browseris negali pasiekti ir sumažinama rizika
            // Matysim tą refresh tokena su cookie

           await sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, expiresAt);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Expires = expiresAt,
                Secure = true
            };
            httpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);


            return Results.Ok(new SuccessfullLoginDto(accessToken));
        });

        app.MapPost("api/accessToken", async (UserManager<AppUser> userManager, JwtTokenService jwtTokenService, SessionLoginService sessionService, HttpContext httpContext) =>
        {
            if(!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return Results.UnprocessableEntity();
            }
            if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            {
                return Results.UnprocessableEntity();
            }

            var sessionId = claims.FindFirstValue("SessionId");
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Results.UnprocessableEntity();
            }

            var sessionIdAsGuid = Guid.Parse(sessionId);
            if (!await sessionService.IsSessionValidAsync(sessionIdAsGuid, refreshToken))
            {
                return Results.UnprocessableEntity();
            }



            var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.UnprocessableEntity();
            }

            var roles = await userManager.GetRolesAsync(user);
            var expiresAt = DateTime.UtcNow.AddDays(1);
            var accessToken = jwtTokenService.CreateAccessToken(user.Email, user.Id, roles);
            var newRefreshToken = jwtTokenService.CreateRefreshToken(sessionIdAsGuid, user.Id, expiresAt);


            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Expires = expiresAt,
                Secure = true
            };
            httpContext.Response.Cookies.Append("RefreshToken", newRefreshToken, cookieOptions);

            await sessionService.ExtendSessionAsync(sessionIdAsGuid, newRefreshToken, expiresAt);

            return Results.Ok(new SuccessfullLoginDto(accessToken));


        });


        app.MapPost("api/logout", async (UserManager<AppUser> userManager, JwtTokenService jwtTokenService, SessionLoginService sessionService, HttpContext httpContext) =>
        {
            if(!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return Results.UnprocessableEntity();
            }
            if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            {
                return Results.UnprocessableEntity();
            }

            var sessionId = claims.FindFirstValue("SessionId");
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Results.UnprocessableEntity();
            }

            await sessionService.InvalidateSessionAsync(Guid.Parse(sessionId));
            httpContext.Response.Cookies.Delete("RefreshToken");


            return Results.Ok();


        });
        

    }

    public record RegisterUserDto(string Name, string LastName, string Email, string Password, DateOnly BirthDate);
    public record LoginUserDto(string Email, string Password);
    public record SuccessfullLoginDto(string AccessToken);

}