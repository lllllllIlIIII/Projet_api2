using System.ComponentModel.DataAnnotations;
namespace Projet_api2.Models;

public class TacheProjet
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    //a qui est assigné la tache dans le projet
    public int AssignedTo { get; set; }

    [Required(ErrorMessage = "Le titre de la tâche est obligatoire.")]
    [StringLength(200, MinimumLength = 3)]
    public string Titre { get; set; } = string.Empty;

    // statut de la tache, complete ou non
    public bool IsCompleted { get; set; } = false;
    [StringLength(1000, ErrorMessage = "Le commentaire est trop long.")]
    public string? Commentaire { get; set; }
    //afficher le nom
    public string? NomAssigne { get; set; }
}