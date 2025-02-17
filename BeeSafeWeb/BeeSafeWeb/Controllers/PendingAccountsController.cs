using BeeSafeWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

public class PendingAccountsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public PendingAccountsController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var users = _userManager.Users
                                               .Where(u => !u.EmailConfirmed)
                                               .ToList();

        return View(users);
    }

    public async Task<IActionResult> Approve(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Reject(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.EmailConfirmed = true;
        await _userManager.SetLockoutEnabledAsync(user, true);
        /* HACK: there's no "locked out" attribute in AspNetUsers, so we'll
         * just lock the user out for a really long time
         */
        await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.UtcNow.AddYears(100)));
        await _userManager.UpdateAsync(user);

        return RedirectToAction("Index");
    }
}