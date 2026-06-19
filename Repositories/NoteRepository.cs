using DevSpace.Database;
using DevSpace.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevSpace.Repositories
{
    public class NoteRepository : IRepository<DevNote>
    {
        public void Add(DevNote item)
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
                    cmd2.CommandText = "INSERT INTO DevNotes (ItemId, Content, LinkedProjectId, LastEdited) VALUES (@ItemId, @Content, @LinkedProjectId, @LastEdited)";
                    cmd2.Parameters.AddWithValue("@ItemId", itemId);
                    cmd2.Parameters.AddWithValue("@Content", item.Content ?? "");
                    cmd2.Parameters.AddWithValue("@LinkedProjectId", item.LinkedProjectId);
                    cmd2.Parameters.AddWithValue("@LastEdited", item.LastEdited.ToString("o"));
                    
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
                cmd.CommandText = "DELETE FROM WorkspaceItems WHERE Id = @Id AND ItemType = 'Note'";
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public List<DevNote> GetAll()
        {
            var list = new List<DevNote>();
            using (var conn = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT w.Id, w.Title, w.DateCreated, w.Tags, 
                           n.Content, n.LinkedProjectId, n.LastEdited
                    FROM WorkspaceItems w
                    INNER JOIN DevNotes n ON w.Id = n.ItemId
                    WHERE w.ItemType = 'Note'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tagsStr = reader.GetString(3);
                        var tags = string.IsNullOrEmpty(tagsStr) ? new List<string>() : tagsStr.Split(',').ToList();

                        var item = new DevNote
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            DateCreated = DateTime.Parse(reader.GetString(2)),
                            Tags = tags,
                            Content = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            LinkedProjectId = reader.GetInt32(5),
                            LastEdited = reader.IsDBNull(6) ? DateTime.MinValue : DateTime.Parse(reader.GetString(6))
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public DevNote GetById(int id)
        {
            return GetAll().FirstOrDefault(n => n.Id == id);
        }

        public void Update(DevNote item)
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
                    cmd2.CommandText = "UPDATE DevNotes SET Content = @Content, LinkedProjectId = @LinkedProjectId, LastEdited = @LastEdited WHERE ItemId = @Id";
                    cmd2.Parameters.AddWithValue("@Content", item.Content ?? "");
                    cmd2.Parameters.AddWithValue("@LinkedProjectId", item.LinkedProjectId);
                    cmd2.Parameters.AddWithValue("@LastEdited", item.LastEdited.ToString("o"));
                    cmd2.Parameters.AddWithValue("@Id", item.Id);
                    cmd2.ExecuteNonQuery();

                    trans.Commit();
                }
            }
        }
    }
}
