using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_api2.Models;
using Projet_api2.Services;
using System.ComponentModel.DataAnnotations;

namespace Projet_api2.Pages.admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel 
{
    private readonly UserService _userService;
    private readonly ProjetService _projetService;

    public DashboardModel(UserService userService, ProjetService projetService)
    {
        _userService = userService;
        _projetService = projetService;
    }

    [BindProperty]
    public NewUserViewModel NouvelUtilisateur { get; set; } = new();

    public List<Projet> ProjetsEnAttente { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        ProjetsEnAttente = _projetService.GetPendingProjets();
    }

    public IActionResult OnPost()
    {
        ProjetsEnAttente = _projetService.GetPendingProjets();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new User
        {
            Nom = NouvelUtilisateur.Nom,
            Prenom = NouvelUtilisateur.Prenom,
            Email = NouvelUtilisateur.Email,
            GlobalRole = GlobalRole.User
        };

        try
        {
            _userService.RegisterUser(user, NouvelUtilisateur.Password);
            TempData["SuccessMessage"] = $"Le compte de {user.Prenom} {user.Nom} a été créé avec succès !";
            ModelState.Clear();
            NouvelUtilisateur = new();
            return RedirectToPage();
        }
        catch
        {
            ErrorMessage = "Erreur lors de la création. Cet email est peut-être déjà utilisé.";
        }

        return Page();
    }
    
    public IActionResult OnPostApprouver(int id)
    {
        _projetService.ChangerStatutProjet(id, ProjectStatus.Active);
        return RedirectToPage();
    }

    public IActionResult OnPostRefuser(int id)
    {
        _projetService.DeleteProjet(id);
        return RedirectToPage();
    }
}

public class NewUserViewModel
{
    [Required(ErrorMessage = "Le nom est obligatoire")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "Format invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire")]
    [MinLength(6, ErrorMessage = "6 caractères minimum")]
    public string Password { get; set; } = string.Empty;
}