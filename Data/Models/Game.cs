namespace Saitynai.Models;

    public enum GameLenght
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
    public required string Name {get; set; }
    public required string Description {get; set; }
    public required int PlayerCount {get; set; }
    public required GameLenght GameLenght { get; set; }
    public required Difficulty Difficulty {get; set; }
    public long SessionId { get; set; }

    public Session Session {get; set; }



    
}