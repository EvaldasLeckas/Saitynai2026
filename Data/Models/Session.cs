using System.ComponentModel.DataAnnotations;
using Saitynai.Auth;

namespace Saitynai.Models;

public class Session
{
    public long Id { get; set; }

    public required DateTime StartDate { get; set; }

    public required DateTime EndDate { get; set; }

    public required int PlayerCount { get; set; }

    public required string City { get; set; }

    public required string Address { get; set; }

    public string? Description { get; set; }

    // Sesijos dalyviai
    public ICollection<SessionParticipant> Participants { get; set; }
        = new List<SessionParticipant>();

    // Sesijoje vykstantys žaidimai
    public ICollection<Game> Games { get; set; }
        = new List<Game>();

    [Required]
    public required string CreatedByUserId { get; set; }
    public AppUser CreatedByUser { get; set; } = null!;
}