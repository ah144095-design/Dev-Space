using DevSpace.Database;
using DevSpace.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevSpace.Repositories
{
    public class SnippetRepository : IRepository<CodeSnippet>
    {
        public void Add(CodeSnippet item)
        {
            using (var conn = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES (@Title, @Date, @Tags, @Type); SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@Title", item.Title ?? "");
                    cmd.Parameters.AddWithValue("@Date", item.DateCreated.ToString("o"));
                    cmd.Parameters.AddWithValue("@Tags", string.Join(",", item.Tags ?? new List<string>()));
                    cmd.Parameters.AddWithValue("@Type", item.GetItemType());

                    long itemId = (long)cmd.ExecuteScalar();

                    var cmd2 = conn.CreateCommand();
                    cmd2.CommandText = "INSERT INTO CodeSnippets (ItemId, Language, CodeBlock, Description, IsFavorite, UsageCount) VALUES (@ItemId, @Language, @CodeBlock, @Description, @IsFavorite, @UsageCount)";
                    cmd2.Parameters.AddWithValue("@ItemId", itemId);
                    cmd2.Parameters.AddWithValue("@Language", item.Language ?? "");
                    cmd2.Parameters.AddWithValue("@CodeBlock", item.CodeBlock ?? "");
                    cmd2.Parameters.AddWithValue("@Description", item.Description ?? "");
                    cmd2.Parameters.AddWithValue("@IsFavorite", item.IsFavorite ? 1 : 0);
                    cmd2.Parameters.AddWithValue("@UsageCount", item.UsageCount);
                    
                    cmd2.ExecuteNonQuery();
                    trans.Commit();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM WorkspaceItems WHERE Id = @Id AND ItemType = 'Snippet'";
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public List<CodeSnippet> GetAll()
        {
            var list = new List<CodeSnippet>();
            using (var conn = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT w.Id, w.Title, w.DateCreated, w.Tags, 
                           s.Language, s.CodeBlock, s.Description, s.IsFavorite, s.UsageCount
                    FROM WorkspaceItems w
                    INNER JOIN CodeSnippets s ON w.Id = s.ItemId
                    WHERE w.ItemType = 'Snippet'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tagsStr = reader.GetString(3);
                        var tags = string.IsNullOrEmpty(tagsStr) ? new List<string>() : tagsStr.Split(',').ToList();

                        var item = new CodeSnippet
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            DateCreated = DateTime.Parse(reader.GetString(2)),
                            Tags = tags,
                            Language = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            CodeBlock = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Description = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            IsFavorite = reader.GetInt32(7) == 1,
                            UsageCount = reader.GetInt32(8)
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public CodeSnippet GetById(int id)
        {
            return GetAll().FirstOrDefault(s => s.Id == id);
        }

        public void Update(CodeSnippet item)
        {
            using (var conn = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE WorkspaceItems SET Title = @Title, Tags = @Tags WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Title", item.Title ?? "");
                    cmd.Parameters.AddWithValue("@Tags", string.Join(",", item.Tags ?? new List<string>()));
                    cmd.Parameters.AddWithValue("@Id", item.Id);
                    cmd.ExecuteNonQuery();

                    var cmd2 = conn.CreateCommand();
                    cmd2.CommandText = "UPDATE CodeSnippets SET Language = @Language, CodeBlock = @CodeBlock, Description = @Description, IsFavorite = @IsFavorite, UsageCount = @UsageCount WHERE ItemId = @Id";
                    cmd2.Parameters.AddWithValue("@Language", item.Language ?? "");
                    cmd2.Parameters.AddWithValue("@CodeBlock", item.CodeBlock ?? "");
                    cmd2.Parameters.AddWithValue("@Description", item.Description ?? "");
                    cmd2.Parameters.AddWithValue("@IsFavorite", item.IsFavorite ? 1 : 0);
                    cmd2.Parameters.AddWithValue("@UsageCount", item.UsageCount);
                    cmd2.Parameters.AddWithValue("@Id", item.Id);
                    cmd2.ExecuteNonQuery();

                    trans.Commit();
                }
            }
        }
    }
}
