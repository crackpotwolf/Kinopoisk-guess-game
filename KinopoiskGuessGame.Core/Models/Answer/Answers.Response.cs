namespace KinopoiskGuessGame.Core.Models.Answer;

public class AnswersResponse
{
    public bool isCorrect { get; set; }
    public bool isTimedOut { get; set; }
    public string correctAnswer { get; set; }
    public StateData stateData { get; set; }
}