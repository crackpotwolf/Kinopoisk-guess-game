using Microsoft.EntityFrameworkCore;
using KinopoiskGuessGame.Data.Models;

namespace KinopoiskGuessGame.Data;

/// <summary>
/// Context Db
/// </summary>
public class KinopoiskContext : DbContext
{
    /// <inheritdoc />
    public KinopoiskContext(DbContextOptions options) : base(options)
    {
    }

    /// <inheritdoc />
    public KinopoiskContext()
    {
    }
    
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    
    /// <summary>
    /// Связи
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}