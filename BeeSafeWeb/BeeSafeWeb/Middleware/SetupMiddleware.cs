using BeeSafeWeb.Data;
using BeeSafeWeb.Services;

/// <summary>
/// This middleware, as its name suggests, runs on every request, but
/// when there are no users, it redirects the user to the setup page.
/// </summary>
public class SetupMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly SetupService _setupService;

    public SetupMiddleware(RequestDelegate next,
                           SetupService setupService,
                           IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _setupService = setupService;
    }

    public async Task Invoke(HttpContext context)
    {
        if (_setupService.IsFirstTime)
        {
            if (!context.Request.Path.StartsWithSegments("/Setup"))
            {
                context.Response.Redirect("/Setup");
                return;
            }
        }
        await _next(context);
    }
}