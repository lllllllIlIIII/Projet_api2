using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_api2.Models;
using Projet_api2.Services;

namespace Projet_api2.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ProjetService _projetService;

    public IndexModel(ProjetService projetService)
    {
        _projetService = projetService;
    }

    public List<Projet> Projets { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 5;

    public void OnGet()
    {
        int currentUserId = 0;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int parsedId))
        {
            currentUserId = parsedId;
        }

        // 🛠️ On vérifie si l'utilisateur est Admin
        bool isAdmin = User.IsInRole("Admin");

        // 🛠️ On passe l'information au service
        var result = _projetService.GetAllProjets(SearchTerm, CurrentPage, PageSize, currentUserId, isAdmin);
        
        Projets = result.Projets;
        TotalPages = (int)Math.Ceiling(result.TotalCount / (double)PageSize);
    }
}