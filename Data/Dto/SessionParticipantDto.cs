namespace Saitynai.DTO;

public class SessionParticipantDto
{
    public required long SessionId { get; set; }

    public required long UserId { get; set; }
}

public class CreateSessionParticipantDto
{
    public required long UserId { get; set; }
}