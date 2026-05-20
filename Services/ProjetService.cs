using Npgsql;
using Projet_api2.Models;

namespace Projet_api2.Services;

public class ProjetService
{
    private readonly string _connectionString;

    public ProjetService(string connectionString) => _connectionString = connectionString;

    public void CreateProjet(Projet projet)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = @"INSERT INTO projets (nom, description, status, createdby, datecreation) 
                      VALUES (@Nom, @Description, @Status, @CreatedBy, @DateCreation)";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Nom", projet.Nom);
        command.Parameters.AddWithValue("Description", projet.Description);
        command.Parameters.AddWithValue("Status", (int)projet.Status);
        command.Parameters.AddWithValue("CreatedBy", projet.CreatedBy);
        command.Parameters.AddWithValue("DateCreation", projet.DateCreation);
        command.ExecuteNonQuery();
    }

    public (List<Projet> Projets, int TotalCount) GetAllProjets(string? search, int page = 1, int pageSize = 5, int currentUserId = 0, bool isAdmin = false)
    {
        List<Projet> projets = new();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        object searchParam = string.IsNullOrWhiteSpace(search) ? DBNull.Value : $"%{search}%";

        var countQuery = @"SELECT COUNT(DISTINCT p.id) FROM projets p 
                           LEFT JOIN projetmembres pm ON p.id = pm.projectid
                           WHERE (@Search::text IS NULL OR p.nom ILIKE @Search::text)
                           AND (@IsAdmin = true OR p.createdby = @CurrentUserId OR pm.userid = @CurrentUserId)";
        using var countCommand = new NpgsqlCommand(countQuery, connection);
        countCommand.Parameters.AddWithValue("Search", searchParam);
        countCommand.Parameters.AddWithValue("IsAdmin", isAdmin);
        countCommand.Parameters.AddWithValue("CurrentUserId", currentUserId);
        int totalCount = Convert.ToInt32(countCommand.ExecuteScalar());
        
        var query = @"SELECT DISTINCT p.id, p.nom, p.description, p.status, p.createdby, p.datecreation, 
                             u.nom AS ""UserNom"", u.prenom AS ""UserPrenom""
                      FROM projets p
                      JOIN users u ON p.createdby = u.id
                      LEFT JOIN projetmembres pm ON p.id = pm.projectid
                      WHERE (@Search::text IS NULL OR p.nom ILIKE @Search::text)
                      AND (@IsAdmin = true OR p.createdby = @CurrentUserId OR pm.userid = @CurrentUserId)
                      ORDER BY p.datecreation DESC LIMIT @Limit OFFSET @Offset";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Search", searchParam);
        command.Parameters.AddWithValue("IsAdmin", isAdmin);
        command.Parameters.AddWithValue("CurrentUserId", currentUserId);
        command.Parameters.AddWithValue("Limit", pageSize);
        command.Parameters.AddWithValue("Offset", (page - 1) * pageSize);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            projets.Add(new Projet
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nom = reader.GetString(reader.GetOrdinal("nom")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Status = (ProjectStatus)reader.GetInt32(reader.GetOrdinal("status")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("createdby")),
                DateCreation = reader.GetDateTime(reader.GetOrdinal("datecreation")),
                NomCreateur = $"{reader.GetString(reader.GetOrdinal("UserPrenom"))} {reader.GetString(reader.GetOrdinal("UserNom"))}"
            });
        }
        return (projets, totalCount);
    }

    public void ChangerStatutProjet(int id, ProjectStatus nouveauStatut)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var command = new NpgsqlCommand("UPDATE projets SET status = @Status WHERE id = @Id", connection);
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("Status", (int)nouveauStatut);
        command.ExecuteNonQuery();
    }

    public void DeleteProjet(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        
        using var cmdMembers = new NpgsqlCommand("DELETE FROM projetmembres WHERE projectid = @Id", connection);
        cmdMembers.Parameters.AddWithValue("Id", id);
        cmdMembers.ExecuteNonQuery();

        using var command = new NpgsqlCommand("DELETE FROM projets WHERE id = @Id", connection);
        command.Parameters.AddWithValue("Id", id);
        command.ExecuteNonQuery();
    }

    public Projet? GetProjetById(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = @"SELECT p.id, p.nom, p.description, p.status, p.createdby, p.datecreation, 
                             u.nom AS ""UserNom"", u.prenom AS ""UserPrenom""
                      FROM projets p JOIN users u ON p.createdby = u.id WHERE p.id = @Id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Projet
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nom = reader.GetString(reader.GetOrdinal("nom")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Status = (ProjectStatus)reader.GetInt32(reader.GetOrdinal("status")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("createdby")),
                DateCreation = reader.GetDateTime(reader.GetOrdinal("datecreation")),
                NomCreateur = $"{reader.GetString(reader.GetOrdinal("UserPrenom"))} {reader.GetString(reader.GetOrdinal("UserNom"))}"
            };
        }
        return null;
    }

    public void UpdateProjet(Projet projet)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var query = "UPDATE projets SET nom = @Nom, description = @Description, status = @Status WHERE id = @Id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Id", projet.Id);
        command.Parameters.AddWithValue("Nom", projet.Nom);
        command.Parameters.AddWithValue("Description", projet.Description);
        command.Parameters.AddWithValue("Status", (int)projet.Status);
        command.ExecuteNonQuery();
    }

    public void AddMembreToProjet(int projectId, int userId, string role)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var query = @"INSERT INTO projetmembres (userid, projectid, roledansprojet) VALUES (@UserId, @ProjectId, @Role)
                      ON CONFLICT (userid, projectid) DO UPDATE SET roledansprojet = @Role";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("UserId", userId);
        command.Parameters.AddWithValue("ProjectId", projectId);
        command.Parameters.AddWithValue("Role", role);
        command.ExecuteNonQuery();
    }

    public List<(User Membre, string Role)> GetMembresByProjectId(int projectId)
    {
        List<(User Membre, string Role)> membres = new();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = @"SELECT u.id, u.nom, u.prenom, u.email, pm.roledansprojet FROM projetmembres pm
                      JOIN users u ON pm.userid = u.id WHERE pm.projectid = @ProjectId ORDER BY u.nom ASC";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("ProjectId", projectId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var user = new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nom = reader.GetString(reader.GetOrdinal("nom")),
                Prenom = reader.GetString(reader.GetOrdinal("prenom")),
                Email = reader.GetString(reader.GetOrdinal("email"))
            };
            membres.Add((user, reader.GetString(reader.GetOrdinal("roledansprojet"))));
        }
        return membres;
    }
    
    public void RemoveMembreFromProjet(int projectId, int userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        
        using var cmdTasks = new NpgsqlCommand("DELETE FROM tacheprojets WHERE projectid = @ProjectId AND assignedto = @UserId", connection);
        cmdTasks.Parameters.AddWithValue("ProjectId", projectId);
        cmdTasks.Parameters.AddWithValue("UserId", userId);
        cmdTasks.ExecuteNonQuery();

        using var command = new NpgsqlCommand("DELETE FROM projetmembres WHERE projectid = @ProjectId AND userid = @UserId", connection);
        command.Parameters.AddWithValue("ProjectId", projectId);
        command.Parameters.AddWithValue("UserId", userId);
        command.ExecuteNonQuery();
    }
    public List<Projet> GetPendingProjets()
    {
        List<Projet> projets = new();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = @"SELECT p.id, p.nom, p.description, p.status, p.createdby, p.datecreation, 
                         u.nom AS ""UserNom"", u.prenom AS ""UserPrenom""
                  FROM projets p JOIN users u ON p.createdby = u.id 
                  WHERE p.status = 0 ORDER BY p.datecreation ASC";

        using var command = new NpgsqlCommand(query, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            projets.Add(new Projet
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nom = reader.GetString(reader.GetOrdinal("nom")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Status = (ProjectStatus)reader.GetInt32(reader.GetOrdinal("status")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("createdby")),
                DateCreation = reader.GetDateTime(reader.GetOrdinal("datecreation")),
                NomCreateur = $"{reader.GetString(reader.GetOrdinal("UserPrenom"))} {reader.GetString(reader.GetOrdinal("UserNom"))}"
            });
        }
        return projets;
    }
}