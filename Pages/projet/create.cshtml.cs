using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_api2.Models;
using Projet_api2.Services;

namespace Projet_api2.Pages.projet;

[Authorize] 
public class CreateModel : PageModel
{
    private readonly ProjetService _projetService;

    public CreateModel(ProjetService projetService)
    {
        _projetService = projetService;
    }

    [BindProperty]
    public Projet NouveauProjet { get; set; } = new();
    

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            NouveauProjet.CreatedBy = userId;
        }
        NouveauProjet.DateCreation = DateTime.UtcNow;
        
        _projetService.CreateProjet(NouveauProjet);
        
        return RedirectToPage("/Index");
    }
}