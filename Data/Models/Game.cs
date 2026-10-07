namespace Saitynai.Models;

public enum GameLength
{
    Short,
    Normal,
    Long
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class Game
{
    public long Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required int PlayerCount { get; set; }

    public required GameLength GameLength { get; set; }

    public required Difficulty Difficulty { get; set; }

    // Ryšys su Session
    public long SessionId { get; set; }

    public Session Session { get; set; }

    // Žaidimo rezultatai
    public ICollection<UserResult> UserResults { get; set; }
        = new List<UserResult>();
}