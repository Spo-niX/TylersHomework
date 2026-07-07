using Microsoft.Data.Sqlite;

namespace TylersHomework.Core.Database;

public static class DatabaseConnection
{
    private static string _connectionString;
    
    public static void Initialize(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
        CreateTables();
    }
    
    public static SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
    
    private static void CreateTables()
    {
        using var connection = GetConnection();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TgId INTEGER UNIQUE NOT NULL,
                AgentName TEXT,
                Mmr INTEGER,
                TaskCompleted INTEGER
            )
        ";
        cmd.ExecuteNonQuery();
    }
}