namespace Saitynai.Models;

public class User
{
    public long Id { get; set; }

    public required string Name { get; set; }

    public required string Surname { get; set; }

    public string? Bio { get; set; }

    public required string Email { get; set; }

    public string? PasswordHash { get; set; }

    public required DateTime BirthDate { get; set; }

    // Sesijos, kuriose naudotojas dalyvauja
    public ICollection<SessionParticipant> SessionParticipants { get; set; }
        = new List<SessionParticipant>();

    // Naudotojo gauti rezultatai
    public ICollection<UserResult> Results { get; set; }
        = new List<UserResult>();
}