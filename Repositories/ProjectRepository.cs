using DevSpace.Database;
using DevSpace.Helpers;
using DevSpace.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevSpace.Repositories
{
    public class ProjectRepository : IRepository<ProjectFolder>
    {
        private const string ProjectSelect = @"
            SELECT w.Id, w.Title, w.DateCreated, w.Tags,
                   p.LocalPath, p.IDEType, p.GitHubRepoUrl, p.TechStack, p.Status, p.LastModified,
                   p.AccentColor, p.CustomLaunchCommand
            FROM WorkspaceItems w
            INNER JOIN ProjectFolders p ON w.Id = p.ItemId
            WHERE w.ItemType = 'Project'";

        public void Add(ProjectFolder item)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO WorkspaceItems (Title, DateCreated, Tags, ItemType) VALUES (@Title, @Date, @Tags, @Type); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@Title", item.Title ?? "");
            cmd.Parameters.AddWithValue("@Date", item.DateCreated.ToString("o"));
            cmd.Parameters.AddWithValue("@Tags", string.Join(",", item.Tags ?? new List<string>()));
            cmd.Parameters.AddWithValue("@Type", item.GetItemType());

            long itemId = (long)cmd.ExecuteScalar();

            var cmd2 = conn.CreateCommand();
            cmd2.CommandText = @"INSERT INTO ProjectFolders
                (ItemId, LocalPath, IDEType, GitHubRepoUrl, TechStack, Status, LastModified, AccentColor, CustomLaunchCommand)
                VALUES (@ItemId, @LocalPath, @IDEType, @GitHubRepoUrl, @TechStack, @Status, @LastModified, @AccentColor, @CustomLaunch)";
            cmd2.Parameters.AddWithValue("@ItemId", itemId);
            cmd2.Parameters.AddWithValue("@LocalPath", item.LocalPath ?? "");
            cmd2.Parameters.AddWithValue("@IDEType", item.IDEType.ToString());
            cmd2.Parameters.AddWithValue("@GitHubRepoUrl", item.GitHubRepoUrl ?? "");
            cmd2.Parameters.AddWithValue("@TechStack", item.TechStack ?? "");
            cmd2.Parameters.AddWithValue("@Status", item.Status.ToString());
            cmd2.Parameters.AddWithValue("@LastModified", item.LastModified.ToString("o"));
            cmd2.Parameters.AddWithValue("@AccentColor", ProjectColorHelper.Normalize(item.AccentColor));
            cmd2.Parameters.AddWithValue("@CustomLaunch", item.CustomLaunchCommand ?? "");
            cmd2.ExecuteNonQuery();
            trans.Commit();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM WorkspaceItems WHERE Id = @Id AND ItemType = 'Project'";
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public List<ProjectFolder> GetAll()
        {
            var list = new List<ProjectFolder>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = ProjectSelect;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapProject(reader));

            return list;
        }

        public ProjectFolder GetById(int id) => GetAll().FirstOrDefault(p => p.Id == id);

        public void Update(ProjectFolder item)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE WorkspaceItems SET Title = @Title, Tags = @Tags WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Title", item.Title ?? "");
            cmd.Parameters.AddWithValue("@Tags", string.Join(",", item.Tags ?? new List<string>()));
            cmd.Parameters.AddWithValue("@Id", item.Id);
            cmd.ExecuteNonQuery();

            var cmd2 = conn.CreateCommand();
            cmd2.CommandText = @"UPDATE ProjectFolders SET
                LocalPath = @LocalPath, IDEType = @IDEType, GitHubRepoUrl = @GitHubRepoUrl,
                TechStack = @TechStack, Status = @Status, LastModified = @LastModified,
                AccentColor = @AccentColor, CustomLaunchCommand = @CustomLaunch
                WHERE ItemId = @Id";
            cmd2.Parameters.AddWithValue("@LocalPath", item.LocalPath ?? "");
            cmd2.Parameters.AddWithValue("@IDEType", item.IDEType.ToString());
            cmd2.Parameters.AddWithValue("@GitHubRepoUrl", item.GitHubRepoUrl ?? "");
            cmd2.Parameters.AddWithValue("@TechStack", item.TechStack ?? "");
            cmd2.Parameters.AddWithValue("@Status", item.Status.ToString());
            cmd2.Parameters.AddWithValue("@LastModified", item.LastModified.ToString("o"));
            cmd2.Parameters.AddWithValue("@AccentColor", ProjectColorHelper.Normalize(item.AccentColor));
            cmd2.Parameters.AddWithValue("@CustomLaunch", item.CustomLaunchCommand ?? "");
            cmd2.Parameters.AddWithValue("@Id", item.Id);
            cmd2.ExecuteNonQuery();

            trans.Commit();
        }

        private static ProjectFolder MapProject(SqliteDataReader reader)
        {
            var tagsStr = reader.GetString(3);
            var tags = string.IsNullOrEmpty(tagsStr) ? new List<string>() : tagsStr.Split(',').ToList();

            return new ProjectFolder
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                DateCreated = DateTime.Parse(reader.GetString(2)),
                Tags = tags,
                LocalPath = reader.IsDBNull(4) ? "" : reader.GetString(4),
                IDEType = Enum.TryParse<IDEType>(reader.GetString(5), out var ide) ? ide : IDEType.Other,
                GitHubRepoUrl = reader.IsDBNull(6) ? "" : reader.GetString(6),
                TechStack = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Status = Enum.TryParse<ProjectStatus>(reader.GetString(8), out var stat) ? stat : ProjectStatus.Active,
                LastModified = reader.IsDBNull(9) ? DateTime.MinValue : DateTime.Parse(reader.GetString(9)),
                AccentColor = reader.FieldCount > 10 && !reader.IsDBNull(10)
                    ? ProjectColorHelper.Normalize(reader.GetString(10))
                    : ProjectColorHelper.DefaultColor,
                CustomLaunchCommand = reader.FieldCount > 11 && !reader.IsDBNull(11)
                    ? reader.GetString(11)
                    : ""
            };
        }
    }
}
