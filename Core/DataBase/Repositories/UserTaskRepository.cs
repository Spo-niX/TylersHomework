using System.Text.Json;
using Microsoft.Data.Sqlite;
using TylersHomework.Core.Database.Models;

namespace TylersHomework.Core.Database.Repositories;

public class UserTaskRepository
{
    private SqliteConnection GetConnection() => DatabaseConnection.GetConnection();
    public async Task SaveAsync(UserTask task)
    {
        using var connection = DatabaseConnection.GetConnection();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO UserTasks 
                (OwnerId, Mode, Hero, Slots, IsActive)
            VALUES 
                (@ownerId, @mode, @hero, @slots, @isactive)
        ";
        
        cmd.Parameters.AddWithValue("@ownerId", task.OwnerId);
        cmd.Parameters.AddWithValue("@mode", task.Mode);
        cmd.Parameters.AddWithValue("@hero", task.Hero);
        var json = JsonSerializer.Serialize(task.Slots);
        cmd.Parameters.AddWithValue("@slots", json);
        cmd.Parameters.AddWithValue("@isactive", task.IsActive);
        
        await cmd.ExecuteNonQueryAsync();
    }
    
    public async Task<UserTask> GetByIdAsync(long ownerId)
    {
        using var connection = DatabaseConnection.GetConnection();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM UserTasks WHERE OwnerId = @ownerId AND IsActive = 1";
        cmd.Parameters.AddWithValue("@ownerId", ownerId);
        
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new UserTask
            {
                Id = reader.GetInt32(0),
                OwnerId = reader.GetInt64(1),
                Mode = reader.GetInt32(2),
                Hero = reader.GetInt32(3),
                Slots = JsonSerializer.Deserialize<List<string>>(reader.GetString(4)),
                IsActive = reader.GetInt64(5) == 1
            };
        }
        
        return null!;
    }
    
    public async Task<bool> ExistsAsync(int ownerId)
    {
        var userTask = await GetByIdAsync(ownerId);
        return userTask != null;
    }
    
    public async Task<int> GetCountAsync()
    {
        using var connection = DatabaseConnection.GetConnection();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM UserTasks";  
        
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}