using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace Intellegens.Commons.Search.Tests.SearchTests.Setup
{
    public abstract class SearchDbContext : DbContext
    {
        public abstract string ConnectionString { get; }

        public DbSet<SearchTestEntity> SearchTestEntities { get; set; }
        public DbSet<SearchTestChildEntity> SearchTestChildEntities { get; set; }
    }

    public class SearchDbContextPostgres : SearchDbContext
    {
        public override string ConnectionString 
            => Environment.GetEnvironmentVariable("INT_TESTS_DB_POSTGRES_CONNECTION_STRING", EnvironmentVariableTarget.User);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString, x => x.MigrationsHistoryTable("__EFSearchDbContextMigrationsHistory"));

            base.OnConfiguring(optionsBuilder);
        }
    }

    public class SearchDbContextSqlite : SearchDbContext
    {
        public override string ConnectionString 
            => Environment.GetEnvironmentVariable("INT_TESTS_DB_SQLITE_CONNECTION_STRING", EnvironmentVariableTarget.User);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));

            optionsBuilder.UseSqlite($@"Data Source={ConnectionString};", x => x.MigrationsHistoryTable("__EFSearchDbContextMigrationsHistory"));
        }
    }
}