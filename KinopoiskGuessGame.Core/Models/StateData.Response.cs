namespace KinopoiskGuessGame.Core.Models;

public class StateData
{
    public int gameId { get; set; }
    public int livesCount { get; set; }
    public Question question { get; set; }
    public int questionTime { get; set; }
    public bool accumulateTime { get; set; }
    public int livesLeft { get; set; }
    public int points { get; set; }
    public string state { get; set; }
}