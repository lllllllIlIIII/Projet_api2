namespace Projet_api2.Models;
using System.ComponentModel.DataAnnotations;

public class ProjetMembre
{
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    [Required(ErrorMessage = "Le role dans le projet est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le role ne doit pas dépasser 100 caractères.")]
    public string RoleDansProjet { get; set; } = string.Empty;
    public string? NomComplet { get; set; }
    public string? Email { get; set; }
}