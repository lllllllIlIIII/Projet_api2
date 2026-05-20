using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Npgsql;
using Projet_api2.Models;

namespace Projet_api2.Services;

public class UserService
{
    private readonly string _connectionString;

    public UserService(string connectionString) => _connectionString = connectionString;

    private (string Hash, string Salt) HashPassword(string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
        string salt = Convert.ToBase64String(saltBytes);

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 32));

        return (hashed, salt);
    }

    private bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        byte[] saltBytes = Convert.FromBase64String(storedSalt);
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 32));

        return hashed == storedHash;
    }

    public User? Authenticate(string email, string password)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand("SELECT Id, Nom, Prenom, Email, PasswordHash, Salt, GlobalRole FROM users WHERE Email = @Email", connection);
        command.Parameters.AddWithValue("Email", email);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
            var storedSalt = reader.GetString(reader.GetOrdinal("Salt"));

            if (VerifyPassword(password, storedHash, storedSalt))
            {
                return new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Nom = reader.GetString(reader.GetOrdinal("Nom")),
                    Prenom = reader.GetString(reader.GetOrdinal("Prenom")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    GlobalRole = (GlobalRole)reader.GetInt32(reader.GetOrdinal("GlobalRole"))
                };
            }
        }
        return null; 
    }

    public void RegisterUser(User user, string password)
    {
        var (hash, salt) = HashPassword(password);

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = "INSERT INTO users (Nom, Prenom, Email, PasswordHash, Salt, GlobalRole) VALUES (@Nom, @Prenom, @Email, @Hash, @Salt, @Role)";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Nom", user.Nom);
        command.Parameters.AddWithValue("Prenom", user.Prenom);
        command.Parameters.AddWithValue("Email", user.Email);
        command.Parameters.AddWithValue("Hash", hash);
        command.Parameters.AddWithValue("Salt", salt);
        command.Parameters.AddWithValue("Role", (int)user.GlobalRole);
        command.ExecuteNonQuery();
    }

    public List<User> GetAllUsers()
    {
        List<User> users = new();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand("SELECT id, nom, prenom, email, globalrole FROM users ORDER BY nom ASC", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nom = reader.GetString(reader.GetOrdinal("nom")),
                Prenom = reader.GetString(reader.GetOrdinal("prenom")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                GlobalRole = (GlobalRole)reader.GetInt32(reader.GetOrdinal("globalrole"))
            });
        }
        return users;
    }
}