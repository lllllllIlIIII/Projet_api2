using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_api2.Models;
using Projet_api2.Services;

namespace Projet_api2.Pages.projet;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ProjetService _projetService;
    private readonly TacheService _tacheService;
    private readonly UserService _userService;
    private readonly PdfExportService _pdfExportService;

    public DetailsModel(
        ProjetService projetService, 
        TacheService tacheService, 
        UserService userService,
        PdfExportService pdfExportService)
    {
        _projetService = projetService;
        _tacheService = tacheService;
        _userService = userService;
        _pdfExportService = pdfExportService;
    }

    public Projet Projet { get; set; } = default!;
    public List<TacheProjet> Taches { get; set; } = new();
    public List<(User Membre, string Role)> Membres { get; set; } = new();
    public List<User> TousLesUsers { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        var projet = _projetService.GetProjetById(id);
        if (projet == null)
        {
            return NotFound();
        }

        Projet = projet;
        Taches = _tacheService.GetTachesByProjectId(id);
        Membres = _projetService.GetMembresByProjectId(id);
        TousLesUsers = _userService.GetAllUsers();

        return Page();
    }

    public IActionResult OnPostAjouterMembre(int id, int? selectedUserId, string? selectedRole)
    {
        if (selectedUserId.HasValue && selectedUserId.Value > 0 && !string.IsNullOrEmpty(selectedRole))
        {
            _projetService.AddMembreToProjet(id, selectedUserId.Value, selectedRole);
        }
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostSupprimerMembre(int id, int? userId)
    {
        if (userId.HasValue)
        {
            _projetService.RemoveMembreFromProjet(id, userId.Value);
        }
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostAjouterTache(int id, int? assignedToUserId, string? titreTache, string? commentaireTache)
    {
        if (assignedToUserId.HasValue && assignedToUserId.Value > 0 && !string.IsNullOrWhiteSpace(titreTache))
        {
            var nouvelleTache = new TacheProjet
            {
                ProjectId = id,
                AssignedTo = assignedToUserId.Value,
                Titre = titreTache,
                Commentaire = commentaireTache,
                IsCompleted = false
            };
            
            _tacheService.AddTache(nouvelleTache);
        }
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostBasculerStatutTache(int id)
    {
        if (int.TryParse(Request.Form["tacheId"], out int tacheId))
        {
            bool actuelStatut = Request.Form["actuelStatut"] == "true";
            _tacheService.UpdateTacheStatut(tacheId, !actuelStatut);
        }
        
        return RedirectToPage(new { id });
    }

    public IActionResult OnGetExportPdf(int id)
    {
        var bytes = _pdfExportService.GenerateProjectReport(id);
        if (bytes.Length == 0) return NotFound();

        return File(bytes, "application/pdf", $"rapport-projet-{id}.pdf");
    }
}