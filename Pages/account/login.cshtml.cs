using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_api2.Services;
using System.ComponentModel.DataAnnotations;

namespace Projet_api2.Pages.account;

public class LoginModel : PageModel
{
    private readonly UserService _userService;

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public LoginModel(UserService userService)
    {
        _userService = userService;
    }
    

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var user = _userService.Authenticate(Input.Email, Input.Password);

        if (user == null)
        {
            ErrorMessage = "Adresse email ou mot de passe incorrect.";
            return Page();
        }
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{user.Prenom} {user.Nom}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.GlobalRole.ToString()) 
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        };
        
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
        return RedirectToPage("/Index");
    }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}