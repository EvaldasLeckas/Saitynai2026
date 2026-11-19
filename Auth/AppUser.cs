using Microsoft.AspNetCore.Identity;
using Saitynai.Models;


namespace Saitynai.Auth;
public class AppUser : IdentityUser
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? Bio { get; set; }
    public required DateOnly BirthDate { get; set; }
    public string? AvatarUrl { get; set; }

    // Sesijos, kuriose naudotojas dalyvauja
    public ICollection<SessionParticipant> SessionParticipants { get; set; }
        = new List<SessionParticipant>();

    // Naudotojo gauti rezultatai
    public ICollection<UserResult> Results { get; set; }
        = new List<UserResult>();
}