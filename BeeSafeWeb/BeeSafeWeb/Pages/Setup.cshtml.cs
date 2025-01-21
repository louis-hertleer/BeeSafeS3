using BeeSafeWeb.Service;
using BeeSafeWeb.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeSafeWeb.Pages;

public class SetupModel : PageModel
{
    private readonly SetupService _setupService;
    private readonly IUserStore<IdentityUser> _userStore;
    private readonly IUserEmailStore<IdentityUser> _userEmailStore;
    private readonly UserManager<IdentityUser> _userManager;
    
    [BindProperty] 
    public RegisterModel.InputModel Input { get; set; }

    public SetupModel(SetupService setupService,
                      IUserStore<IdentityUser> userStore,
                      UserManager<IdentityUser> userManager)
    {
        _setupService = setupService;
        _userStore = userStore;
        _userEmailStore = (IUserEmailStore<IdentityUser>) _userStore;
        _userManager = userManager;
    }

    public IActionResult OnGet()
    {
        if (!_setupService.IsFirstTime)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            goto fail;

        var user = Activator.CreateInstance<IdentityUser>();
        /* The first user is automatically confirmed. */
        user.EmailConfirmed = true;

        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await _userEmailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, Input.Password);

        if(!result.Succeeded)
            goto fail;

        _setupService.IsFirstTime = false;

        return RedirectToPage("/");
    fail:
        return Page();
    }
}