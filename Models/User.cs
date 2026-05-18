namespace Projet_api2.Models;
public enum GlobalRole 
{ 
    User = 0, 
    Admin = 1 
}
public class User
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public GlobalRole GlobalRole { get; set; } = GlobalRole.User;
}