namespace Saitynai.Models;

public class UserResult
{
    public long Id { get; set; }

    public required float Score { get; set; }

    public required int Placement { get; set; }

    // Žaidimas
    public long GameId { get; set; }

    public Game Game { get; set; }

    // Naudotojas, kuriam priklauso rezultatas
    public long UserId { get; set; }

    public User User { get; set; }
}