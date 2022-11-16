namespace KinopoiskGuessGame.Core.Models;

public class Question
{
    public int id { get; set; }
    public string imageUrl { get; set; }
    public List<string> answers { get; set; }
}