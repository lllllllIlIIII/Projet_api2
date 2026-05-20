using Npgsql;
using Projet_api2.Models;

namespace Projet_api2.Services;

public class TacheService
{
    private readonly string _connectionString;

    public TacheService(string connectionString) => _connectionString = connectionString;

    public List<TacheProjet> GetTachesByProjectId(int projectId)
    {
        List<TacheProjet> taches = new();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = @"SELECT t.id, t.projectid, t.assignedto, t.titre, t.iscompleted, t.commentaire, 
                             u.nom AS ""UserNom"", u.prenom AS ""UserPrenom""
                      FROM tacheprojets t JOIN users u ON t.assignedto = u.id
                      WHERE t.projectid = @ProjectId ORDER BY t.id ASC";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("ProjectId", projectId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            taches.Add(new TacheProjet
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                ProjectId = reader.GetInt32(reader.GetOrdinal("projectid")),
                AssignedTo = reader.GetInt32(reader.GetOrdinal("assignedto")),
                Titre = reader.GetString(reader.GetOrdinal("titre")),
                IsCompleted = reader.GetBoolean(reader.GetOrdinal("iscompleted")),
                Commentaire = reader.IsDBNull(reader.GetOrdinal("commentaire")) ? null : reader.GetString(reader.GetOrdinal("commentaire")),
                NomAssigne = $"{reader.GetString(reader.GetOrdinal("UserPrenom"))} {reader.GetString(reader.GetOrdinal("UserNom"))}"
            });
        }
        return taches;
    }

    public void AddTache(TacheProjet tache)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = "INSERT INTO tacheprojets (projectid, assignedto, titre, iscompleted, commentaire) VALUES (@ProjectId, @AssignedTo, @Titre, false, @Commentaire)";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("ProjectId", tache.ProjectId);
        command.Parameters.AddWithValue("AssignedTo", tache.AssignedTo);
        command.Parameters.AddWithValue("Titre", tache.Titre);
        command.Parameters.AddWithValue("Commentaire", tache.Commentaire ?? (object)DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void UpdateTacheStatut(int tacheId, bool isCompleted)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand("UPDATE tacheprojets SET iscompleted = @IsCompleted WHERE id = @Id", connection);
        command.Parameters.AddWithValue("Id", tacheId);
        command.Parameters.AddWithValue("IsCompleted", isCompleted);
        command.ExecuteNonQuery();
    }
}