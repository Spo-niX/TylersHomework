using Microsoft.Data.Sqlite;
using TylersHomework.Core.Database.Models;

namespace TylersHomework.Core.Database.Repositories;

public class UserRepository
{
    private SqliteConnection GetConnection() => DatabaseConnection.GetConnection();
    public async Task SaveAsync(User user)
    {
        using var connection = DatabaseConnection.GetConnection();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO Users 
                (TgId, AgentName, Mmr, TaskCompleted)
            VALUES 
                (@tgId, @name, @mmr, @tasks)
        ";
        
        cmd.Parameters.AddWithValue("@tgId", user.TgId);
        cmd.Parameters.AddWithValue("@name", user.AgentName);
        cmd.Parameters.AddWithValue("@mmr", user.Mmr);
        cmd.Parameters.AddWithValue("@tasks", user.TaskCompleted);
        
        await cmd.ExecuteNonQueryAsync();
    }
    
    public async Task<User> GetByTelegramIdAsync(long telegramId)
    {
        using var connection = DatabaseConnection.GetConnection();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE TgId = @tgId";
        cmd.Parameters.AddWithValue("@tgId", telegramId);
        
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                TgId = reader.GetInt64(1),
                AgentName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Mmr = reader.GetInt32(3),
                TaskCompleted = reader.GetInt32(4)
            };
        }
        
        return null!;
    }
    
    public async Task<bool> ExistsAsync(long telegramId)
    {
        var user = await GetByTelegramIdAsync(telegramId);
        return user != null;
    }
    
    public async Task<int> GetCountAsync()
    {
        using var connection = DatabaseConnection.GetConnection();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Users";
        
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}