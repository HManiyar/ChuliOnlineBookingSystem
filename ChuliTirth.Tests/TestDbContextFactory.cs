using ChuliTirth.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Tests;

// Sqlite in-memory keeps real relational/transaction semantics (unlike the EF InMemory provider),
// which matters here because BookingService opens a real DB transaction.
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
