using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace DevSpace.Database
{
    public static class DatabaseManager
    {
        public static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devspace.db");
        public static string ConnectionString => $"Data Source={DbPath}";

        public static void InitializeDatabase()
        {
            bool dbExists = File.Exists(DbPath);

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS WorkspaceItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        DateCreated TEXT NOT NULL,
                        Tags TEXT,
                        ItemType TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS ProjectFolders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemId INTEGER REFERENCES WorkspaceItems(Id) ON DELETE CASCADE,
                        LocalPath TEXT,
                        IDEType TEXT,
                        GitHubRepoUrl TEXT,
                        TechStack TEXT,
                        Status TEXT,
                        LastModified TEXT
                    );

                    CREATE TABLE IF NOT EXISTS CodeSnippets (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemId INTEGER REFERENCES WorkspaceItems(Id) ON DELETE CASCADE,
                        Language TEXT,
                        CodeBlock TEXT,
                        Description TEXT,
                        IsFavorite INTEGER DEFAULT 0,
                        UsageCount INTEGER DEFAULT 0
                    );

                    CREATE TABLE IF NOT EXISTS DevNotes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemId INTEGER REFERENCES WorkspaceItems(Id) ON DELETE CASCADE,
                        Content TEXT,
                        LinkedProjectId INTEGER,
                        LastEdited TEXT
                    );

                    CREATE TABLE IF NOT EXISTS DeveloperProfile (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT,
                        Theme TEXT DEFAULT 'Dark',
                        TotalLaunches INTEGER DEFAULT 0
                    );
                ";
                command.ExecuteNonQuery();
                MigrateSchema(connection);

                if (!dbExists || IsDatabaseEmpty(connection))
                {
                    SeedData(connection);
                }
            }
        }

        private static void MigrateSchema(SqliteConnection connection)
        {
            EnsureColumn(connection, "ProjectFolders", "AccentColor", "TEXT DEFAULT '#D97706'");
            EnsureColumn(connection, "ProjectFolders", "CustomLaunchCommand", "TEXT");
        }

        private static void EnsureColumn(SqliteConnection connection, string table, string column, string definition)
        {
            var check = connection.CreateCommand();
            check.CommandText = $"PRAGMA table_info({table})";
            using var reader = check.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(column, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
            alter.ExecuteNonQuery();
        }

        private static bool IsDatabaseEmpty(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM WorkspaceItems";
            long count = (long)command.ExecuteScalar();
            return count == 0;
        }

        private static void SeedData(SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Profile
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "INSERT INTO DeveloperProfile (Name, Theme, TotalLaunches) VALUES ('Alex', 'Dark', 0)";
                    cmd.ExecuteNonQuery();

                    // Project 1
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('E-Commerce API', $date, 'C#,WebAPI', 'Project'); SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long p1Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO ProjectFolders (ItemId, LocalPath, IDEType, GitHubRepoUrl, TechStack, Status, LastModified) VALUES ($itemId, 'C:\\Dev\\ECommerceApi', 'VisualStudio', 'https://github.com/alex/ecommerce', 'C#, .NET 8, EF Core', 'Active', $modDate)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", p1Id);
                    cmd.Parameters.AddWithValue("$modDate", DateTime.Now.ToString("o"));
                    cmd.ExecuteNonQuery();

                    // Project 2
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('React Dashboard', $date, 'React,Frontend', 'Project'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long p2Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO ProjectFolders (ItemId, LocalPath, IDEType, GitHubRepoUrl, TechStack, Status, LastModified) VALUES ($itemId, 'C:\\Dev\\ReactDashboard', 'VSCode', 'https://github.com/alex/react-dashboard', 'React, TypeScript, Tailwind', 'Paused', $modDate)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", p2Id);
                    cmd.Parameters.AddWithValue("$modDate", DateTime.Now.AddDays(-2).ToString("o"));
                    cmd.ExecuteNonQuery();

                    // Project 3
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('Python Scraper', $date, 'Python,Tools', 'Project'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.AddDays(-10).ToString("o"));
                    long p3Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO ProjectFolders (ItemId, LocalPath, IDEType, GitHubRepoUrl, TechStack, Status, LastModified) VALUES ($itemId, 'C:\\Dev\\PyScraper', 'VSCode', '', 'Python, BeautifulSoup', 'Completed', $modDate)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", p3Id);
                    cmd.Parameters.AddWithValue("$modDate", DateTime.Now.AddDays(-5).ToString("o"));
                    cmd.ExecuteNonQuery();

                    // Snippet 1
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('EF Core Context Setup', $date, 'EFCore,Setup', 'Snippet'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long s1Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO CodeSnippets (ItemId, Language, CodeBlock, Description, IsFavorite, UsageCount) VALUES ($itemId, 'C#', 'public class AppDbContext : DbContext\n{\n    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }\n}', 'Basic DbContext setup for EF Core', 1, 5)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", s1Id);
                    cmd.ExecuteNonQuery();

                    // Snippet 2
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('Fetch Data React', $date, 'React,Hooks', 'Snippet'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long s2Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO CodeSnippets (ItemId, Language, CodeBlock, Description, IsFavorite, UsageCount) VALUES ($itemId, 'JavaScript', 'useEffect(() => {\n  const fetchData = async () => {\n    const res = await fetch(\"/api/data\");\n    const data = await res.json();\n    setData(data);\n  };\n  fetchData();\n}, []);', 'UseEffect hook for fetching data', 0, 12)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", s2Id);
                    cmd.ExecuteNonQuery();

                    // Snippet 3
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('SQL User Login', $date, 'SQL,Auth', 'Snippet'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long s3Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO CodeSnippets (ItemId, Language, CodeBlock, Description, IsFavorite, UsageCount) VALUES ($itemId, 'SQL', 'SELECT Id, Username FROM Users WHERE Username = @User AND PasswordHash = @Hash;', 'Verify user credentials', 0, 3)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", s3Id);
                    cmd.ExecuteNonQuery();

                    // Snippet 4
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('Centering in CSS', $date, 'CSS,Layout', 'Snippet'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long s4Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO CodeSnippets (ItemId, Language, CodeBlock, Description, IsFavorite, UsageCount) VALUES ($itemId, 'CSS', '.centered {\n  display: flex;\n  justify-content: center;\n  align-items: center;\n}', 'Perfect centering with Flexbox', 1, 20)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", s4Id);
                    cmd.ExecuteNonQuery();

                    // Snippet 5
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('Python Read File', $date, 'Python,IO', 'Snippet'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long s5Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO CodeSnippets (ItemId, Language, CodeBlock, Description, IsFavorite, UsageCount) VALUES ($itemId, 'Python', 'with open(\"file.txt\", \"r\") as f:\n    content = f.read()', 'Read entire file content in Python', 0, 2)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", s5Id);
                    cmd.ExecuteNonQuery();

                    // Note 1
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('API Architecture Ideas', $date, 'Architecture', 'Note'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long n1Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO DevNotes (ItemId, Content, LinkedProjectId, LastEdited) VALUES ($itemId, 'Considering using CQRS for the new API. Need to evaluate MediatR vs custom implementation.', $projId, $editDate)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", n1Id);
                    cmd.Parameters.AddWithValue("$projId", p1Id);
                    cmd.Parameters.AddWithValue("$editDate", DateTime.Now.ToString("o"));
                    cmd.ExecuteNonQuery();

                    // Note 2
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES ('UI State Management', $date, 'State', 'Note'); SELECT last_insert_rowid();";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
                    long n2Id = (long)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO DevNotes (ItemId, Content, LinkedProjectId, LastEdited) VALUES ($itemId, 'Should we use Redux or Context API? Given the size of the app, Context API might be enough.', $projId, $editDate)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$itemId", n2Id);
                    cmd.Parameters.AddWithValue("$projId", p2Id);
                    cmd.Parameters.AddWithValue("$editDate", DateTime.Now.ToString("o"));
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
