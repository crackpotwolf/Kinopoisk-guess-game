using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using AutoMapper;
using KinopoiskGuessGame.Common.Attributes;
using KinopoiskGuessGame.Common.WebClient;
using KinopoiskGuessGame.Core.Models.Answer;
using KinopoiskGuessGame.Core.Models.InitGame;
using KinopoiskGuessGame.Data.Interfaces.Repositories;
using KinopoiskGuessGame.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Newtonsoft.Json;

namespace KinopoiskGuessGame.Core.Api;

/// <summary>
/// API для игры
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[DisplayName("birthday19")]
[SetRoute("birthday19")]
//[Authorize]
public class PlayController : ControllerBase
{
    private readonly IBaseEntityRepository<Answer> _answerRepository;
    private readonly IBaseEntityRepository<Question> _questionRepository;
    private readonly ILogger<IndexModel> _logger;

    private const string UrlInitGame = "https://kp-guess-game-api.kinopoisk.ru/v1/games";
    private const string UrlAnswers = "https://kp-guess-game-api.kinopoisk.ru/v1/questions/answers";

    private readonly TimeSpan _timeout = TimeSpan.FromHours(5);
    
    /// <inheritdoc />
    public PlayController(IBaseEntityRepository<Answer> answer,
        IBaseEntityRepository<Question> question,
        ILogger<IndexModel> logger)
    {
        _logger = logger;
        _answerRepository = answer;
        _questionRepository = question;
    }
    
    /// <summary>
    /// Играть
    /// </summary>
    /// <param name="gameId">Номер игры</param>
    /// <param name="cookie">cookie</param>
    /// <param name="countRepeat">Число запусков</param>
    /// <param name="maxSeconds">Максимальное количество секунд для симуляции ожидания</param>
    /// <returns></returns>
    [HttpGet("game/{gameId:int}/play")]
    public IActionResult Play(int gameId, string cookie, int countRepeat, int maxSeconds)
    {
        // Начать игру
        var initGame = GetInitGame(gameId, cookie);
        
        // Номер вопроса
        var question = initGame.stateData.question;

        while (countRepeat >= 0)
        {
            // Получить ответ из базы
            var answerDb = _answerRepository
                .GetListQuery()
                .Include(p => p.Question)
                .FirstOrDefault(p => p.Question.GameId == gameId &&
                                     p.Question.Id == question.id &&
                                     p.IsCorrect);
            // Если ответ есть
            if (answerDb != null)
            {
                _logger.LogInformation(message: $"{countRepeat}. Ответ найден: Name - {answerDb.Name}, " +
                                                $"QuestionId - {question.id}");

                // Отправить ответ
                var answer = GetAnswer(gameId, cookie, answerDb.Name);

                // Получить id нового вопроса
                question = answer.stateData.question;
                
                countRepeat--;
            
                // Симуляция
                SleepSimulation(maxSeconds);
            }
            else // Если ответа нет
            {
                _logger.LogInformation(message: $"{countRepeat}. !!! Ответ не найден: " +
                                                $"Id - {question.id}");
            }
        }

        return Ok();
    }
    
    /// <summary>
    /// Наполнить базу
    /// </summary>
    /// <param name="gameId">Номер игры</param>
    /// <param name="cookie">cookie</param>
    /// <param name="countRepeat">Число запусков</param>
    /// <param name="maxSeconds">Максимальное количество секунд для симуляции ожидания</param>
    /// <returns></returns>
    [HttpGet("game/{gameId:int}/fillBase")]
    public IActionResult FillBase(int gameId, string cookie, int countRepeat, int maxSeconds)
    {
        // Начать игру
        var initGame = GetInitGame(gameId, cookie);

        // Номер вопроса
        var question = initGame.stateData.question;
        
        while (countRepeat >= 0)
        {
            AnswersResponse answer;
            
            //_logger.LogInformation(message: $"{countRepeat}. Вопрос получен: Name - {initGame.stateData.question.imageUrl}, " +
                                            //$"Id - {questionId}");
            
            // Получить ответ из базы
            var answerFromDb = _answerRepository
                .GetListQuery()
                .Include(p => p.Question)
                .FirstOrDefault(p => p.Question.GameId == gameId &&
                                     p.Question.Id == question.id &&
                                     p.IsCorrect);

            // Если ответ есть
            if (answerFromDb != null)
            {
                _logger.LogInformation(message: $"{countRepeat}. !!! Ответ найден: {answerFromDb.Name}, " +
                                                $"Id - {question.id}");
                // Отправить ответ
                answer = GetAnswer(gameId, cookie, answerFromDb.Name);

                // Обновить номер вопроса
                question = answer.stateData.question;

                continue;
            }
            
            // Если ответа нет
            _logger.LogInformation(message: $"{countRepeat}. Ответ не найден");
            
            // Взять первое значение 
            var firstAnswer = question.answers.FirstOrDefault();
            
            // Отправить ответ
            answer = GetAnswer(gameId, cookie, firstAnswer);
            
            // Записать вопрос в БД
            var questionDb = new Question()
            {
                Id = question.id,
                Name = question.imageUrl,
                GameId = gameId
            };

            _questionRepository.Add(questionDb);

            // Записать все ответы на вопросы в БД
            foreach (var answerName in question.answers)
            {
                var answerToDb = new Answer()
                {
                    QuestionGuid = questionDb.Guid,
                    Name = answerName
                };
                
                // Если это правильный ответ, пометить его
                if (answerName == answer.correctAnswer)
                {
                    answerToDb.IsCorrect = true;
                    //_logger.LogInformation(message: $"{countRepeat}. Ответ получен: {answerName}");
                }
                else if (answerName == firstAnswer && answer.correctAnswer.Length == 0)
                {
                    answerToDb.IsCorrect = true;
                    //_logger.LogInformation(message: $"{countRepeat}. Ответ получен: {answerName}");
                }
                
                // Записать ответ
                _answerRepository.Add(answerToDb);
            }

            countRepeat--;

            // Завершить игру
            while (answer.stateData.livesLeft > 0)
            {
                // Взять первое значение 
                firstAnswer = answer.stateData.question.answers.FirstOrDefault();
            
                // Отправить ответ
                answer = GetAnswer(gameId, cookie, firstAnswer);
            }
            
            // Начать игру
            initGame = GetInitGame(gameId, cookie);

            // Номер вопроса
            question = initGame.stateData.question;
            
            // Симуляция
            //SleepSimulation(maxSeconds);
        }
        
        return Ok();
    }

    /// <summary>
    /// Наполнить базу Fast
    /// </summary>
    /// <param name="gameId">Номер игры</param>
    /// <param name="cookie">cookie</param>
    /// <param name="countRepeat">Число запусков</param>
    /// <param name="maxSeconds">Максимальное количество секунд для симуляции ожидания</param>
    /// <returns></returns>
    [HttpGet("game/{gameId:int}/fillBaseFast")]
    public IActionResult FillBaseFast(int gameId, string cookie, int countRepeat, int maxSeconds)
    {
        while (countRepeat >= 0)
        {
            // Начать игру
            var initGame = GetInitGame(gameId, cookie);

            // Номер вопроса
            var question = initGame.stateData.question;

            //_logger.LogInformation(message: $"{countRepeat}. Вопрос получен: Name - {initGame.stateData.question.imageUrl}, " +
                                            //$"Id - {questionId}");
            
            // Получить ответ из базы
            var answerFromDb = _answerRepository
                .GetListQuery()
                .Include(p => p.Question)
                .FirstOrDefault(p => p.Question.GameId == gameId &&
                                     p.Question.Id == question.id &&
                                     p.IsCorrect);

            // Если ответ есть
            if (answerFromDb != null)
            {
                _logger.LogInformation(message: $"{countRepeat}. !!! Ответ найден: {answerFromDb.Name}, " +
                                                $"Id - {question.id}");
                continue;
            }
            
            // Если ответа нет
            _logger.LogInformation(message: $"{countRepeat}. Ответ не найден");
            
            // Взять первое значение 
            var firstAnswer = question.answers.FirstOrDefault();
            
            // Отправить ответ
            var answer = GetAnswer(gameId, cookie, firstAnswer);
            
            // Записать вопрос в БД
            var questionDb = new Question()
            {
                Id = question.id,
                Name = question.imageUrl,
                GameId = gameId
            };

            _questionRepository.Add(questionDb);

            // Записать все ответы на вопросы в БД
            foreach (var answerName in question.answers)
            {
                var answerToDb = new Answer()
                {
                    QuestionGuid = questionDb.Guid,
                    Name = answerName
                };
                
                // Если это правильный ответ, пометить его
                if (answerName == answer.correctAnswer)
                {
                    answerToDb.IsCorrect = true;
                    //_logger.LogInformation(message: $"{countRepeat}. Ответ получен: {answerName}");
                }
                else if (answerName == firstAnswer && answer.correctAnswer.Length == 0)
                {
                    answerToDb.IsCorrect = true;
                    //_logger.LogInformation(message: $"{countRepeat}. Ответ получен: {answerName}");
                }
                
                // Записать ответ
                _answerRepository.Add(answerToDb);
            }

            countRepeat--;
            
            // Симуляция
            //SleepSimulation(maxSeconds);
        }
        
        return Ok();
    }
    
    /// <summary>
    /// Симуляция
    /// </summary>
    /// <param name="maxSeconds">Максимальное число секунд</param>
    private static void SleepSimulation(int maxSeconds)
    {
        var random = new Random();
        var mseconds = random.Next(100, maxSeconds * 1000);
            
        Thread.Sleep(mseconds);
    }

    /// <summary>
    /// Запустить игру
    /// </summary>
    /// <param name="gameId">id игры</param>
    /// <param name="cookie">cookie</param>
    /// <returns></returns>
    private InitGameResponse GetInitGame(int gameId, string cookie)
    {
        try
        {
            using var webClient = new TimeoutWebClient(_timeout);
            webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            webClient.Headers[HttpRequestHeader.Cookie] = cookie;
        
            var initGameData = JsonConvert.SerializeObject(new InitGameRequest
            {
                gameId = gameId
            });

            // Выполняем запрос по адресу и получаем ответ в виде строки
            var initGameResponse = webClient.UploadString(UrlInitGame, initGameData);
        
            var result = JsonConvert.DeserializeObject<InitGameResponse>(initGameResponse);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogInformation(message: $"Ошибка: {e.Message}");
            return GetInitGame(gameId, cookie);
        }
    }
    
    /// <summary>
    /// Получить ответ
    /// </summary>
    /// <param name="gameId">id игры</param>
    /// <param name="cookie">cookie</param>
    /// <param name="answer">ответ</param>
    /// <returns></returns>
    private AnswersResponse GetAnswer(int gameId, string cookie, string answer)
    {
        try
        {
            using var webClient = new TimeoutWebClient(_timeout);
            webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            webClient.Headers[HttpRequestHeader.Cookie] = cookie;

            var answerData = JsonConvert.SerializeObject(new AnswerRequest
            {
                answer = answer
            });

            // Выполняем запрос по адресу и получаем ответ в виде строки
            var answerResponse = webClient.UploadString(UrlAnswers, answerData);
        
            var result = JsonConvert.DeserializeObject<AnswersResponse>(answerResponse);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogInformation(message: $"Ошибка: {e.Message}");
            return GetAnswer(gameId, cookie, answer);
        }
    }
}