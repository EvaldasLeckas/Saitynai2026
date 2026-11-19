using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Saitynai.Auth;
using Saitynai.DTO;
using Saitynai.Services;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    private readonly UserService _service;
    private readonly IValidator<CreateUserDto> _validator;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public UsersController(
        UserService service,
        IValidator<CreateUserDto> validator,
        UserManager<AppUser> userManager,
        IWebHostEnvironment env)
    {
        _service = service;
        _validator = validator;
        _userManager = userManager;
        _env = env;
    }

    private ActionResult? CheckOwnership(string? ownerId)
    {
        if (User.GetUserId() == null) return Unauthorized();
        if (ownerId == null) return NotFound();
        return User.IsOwnerOrAdmin(ownerId) ? null : Forbid();
    }

    // ---------- avatar helpers ----------

    // decides the type from the file's real bytes, not from the client's content-type or filename
    private static string? DetectImageExtension(byte[] h, int len)
    {
        if (len >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ".jpg";
        if (len >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return ".png";
        if (len >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                      && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P') return ".webp";
        return null;
    }

    private string AvatarFolder =>
        Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "avatars");

    private void DeleteAvatarFile(string? avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl)) return;
        var path = Path.Combine(AvatarFolder, Path.GetFileName(avatarUrl)); // GetFileName blocks path tricks
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    // ---------- users ----------

    [HttpGet(Name = "GetUsers")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] PageParameters pageParameters,
        LinkGenerator linkGenerator)
    {
        var pagedUsers = await _service.GetAllAsync(pageParameters);

        var paginationMetadata = pagedUsers.CreatePaginationMetadata(
            linkGenerator, HttpContext, "GetUsers");

        Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(pagedUsers);
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var check = CheckOwnership(id);
        if (check != null) return check;

        var user = await _service.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string id, CreateUserDto dto)
    {
        var check = CheckOwnership(id);
        if (check != null) return check;

        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var check = CheckOwnership(id);
        if (check != null) return check;

        var avatarUrl = (await _userManager.FindByIdAsync(id))?.AvatarUrl; // remember it before the user is gone

        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();

        DeleteAvatarFile(avatarUrl);
        return NoContent();
    }

    // GET: api/users/5/profile
    [HttpGet("{id}/profile", Name = "GetUserProfile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(string id)
    {
        var check = CheckOwnership(id);
        if (check != null) return check;

        var profile = await _service.GetProfileAsync(id);
        return profile == null ? NotFound() : Ok(profile);
    }

    // GET: api/users/me/profile
    [HttpGet("me/profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var profile = await _service.GetProfileAsync(userId);
        return profile == null ? NotFound() : Ok(profile);
    }


    [HttpPost("{id}/avatar")]
    [Authorize]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(string id, [FromForm] IFormFile file)
    {
        var check = CheckOwnership(id);
        if (check != null) return check;

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });
        if (file.Length > MaxAvatarBytes)
            return BadRequest(new { message = "Image must be 2 MB or smaller." });

        // read only the first bytes to identify the real file type
        var header = new byte[12];
        int read;
        await using (var stream = file.OpenReadStream())
            read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);

        var ext = DetectImageExtension(header, read);
        if (ext == null)
            return BadRequest(new { message = "Only JPEG, PNG or WebP images are allowed." });

        Directory.CreateDirectory(AvatarFolder);
        var fileName = $"{Guid.NewGuid():N}{ext}"; // never use the client's filename
        var fullPath = Path.Combine(AvatarFolder, fileName);
        await using (var target = System.IO.File.Create(fullPath))
            await file.CopyToAsync(target);

        var oldUrl = user.AvatarUrl;
        user.AvatarUrl = $"/avatars/{fileName}";
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            System.IO.File.Delete(fullPath);
            return Problem("Could not save the avatar.");
        }

        DeleteAvatarFile(oldUrl); // remove the previous image
        return Ok(new { avatarUrl = user.AvatarUrl });
    }

    // DELETE: api/users/5/avatar
    [HttpDelete("{id}/avatar")]
    [Authorize]
    public async Task<IActionResult> DeleteAvatar(string id)
    {
        var check = CheckOwnership(id);
        if (check != null) return check;

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var oldUrl = user.AvatarUrl;
        user.AvatarUrl = null;
        await _userManager.UpdateAsync(user);
        DeleteAvatarFile(oldUrl);

        return NoContent();
    }
}