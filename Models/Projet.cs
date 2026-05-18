using System.ComponentModel.DataAnnotations;
namespace Projet_api2.Models;
public enum ProjectStatus
{
    Pending = 0,  // projet en attente d'etre validé par l'admin
    Active = 1,   // le projet a été approuvé par l'Admin
    Rejected = 2  // l'Admin a refusé le projet
}
public class Projet
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom du projet est obligatoire.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Le nom doit faire entre 3 et 100 caractères.")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire.")]
    [StringLength(1000, ErrorMessage = "La description est trop longue (max 1000 caractères).")]
    public string Description { get; set; } = string.Empty;

    //par defaut, a la creation d'un projet elle est en attente
    public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

    //l'id de la personne qui fais la demande de projet
    public int CreatedBy { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    //sert a afficher le nom du createur de projet dans le dashboard admin
    public string? NomCreateur { get; set; }
    
    
    
}