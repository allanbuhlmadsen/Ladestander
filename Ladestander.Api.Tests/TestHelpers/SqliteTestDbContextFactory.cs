using Ladestander.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Tests.TestHelpers;

public static class SqliteTestDbContextFactory
{
    public static AppDbContext CreateContext()
    {
        // SQLite in-memory is used instead of EF Core InMemory
        // to test relational behavior such as foreign key constraints.
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}