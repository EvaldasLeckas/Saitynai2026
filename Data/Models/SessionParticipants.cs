using System.ComponentModel.DataAnnotations;
using Saitynai.Auth;

namespace Saitynai.Models;

public class SessionParticipant
{
    public long Id { get; set; }

    public long SessionId { get; set; }

    public Session Session { get; set; }

    [Required]
    public required string UserId { get; set; }

    public AppUser User { get; set; }
}