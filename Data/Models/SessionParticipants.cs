namespace Saitynai.Models;

public class SessionParticipant
{
    public long Id { get; set; }

    public long SessionId { get; set; }

    public Session Session { get; set; }

    public long UserId { get; set; }

    public User User { get; set; }
}