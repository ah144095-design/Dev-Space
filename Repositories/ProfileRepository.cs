using DevSpace.Database;
using DevSpace.Models;
using Microsoft.Data.Sqlite;

namespace DevSpace.Repositories
{
    public class ProfileRepository
    {
        public DeveloperProfile GetProfile()
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Theme, TotalLaunches FROM DeveloperProfile LIMIT 1";

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return new DeveloperProfile { Name = "Developer", Theme = "Light" };

            return new DeveloperProfile
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "Developer" : reader.GetString(1),
                Theme = reader.IsDBNull(2) ? "Light" : reader.GetString(2),
                TotalLaunches = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
            };
        }

        public void UpdateTheme(string theme)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE DeveloperProfile SET Theme = @Theme WHERE Id = (SELECT Id FROM DeveloperProfile LIMIT 1);
                INSERT INTO DeveloperProfile (Name, Theme, TotalLaunches)
                SELECT 'Developer', @Theme, 0
                WHERE NOT EXISTS (SELECT 1 FROM DeveloperProfile);";
            cmd.Parameters.AddWithValue("@Theme", theme);
            cmd.ExecuteNonQuery();
        }
    }
}
