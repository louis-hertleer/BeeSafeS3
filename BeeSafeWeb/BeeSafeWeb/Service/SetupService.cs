using BeeSafeWeb.Data;

namespace BeeSafeWeb.Service;

/* This class wraps the functionality of checking if there are no users in the
 * database. The reason for this is that it needs to make a query to check if
 * there are users in the database, but it would be silly to check whether there
 * are users every single request, hence this class.
 */
public class SetupService(IServiceProvider serviceProvider)
{
    private bool _hasBeenRun = false;
    private bool _isFirstTime = true;

    public bool IsFirstTime
    {
        get
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (!_hasBeenRun)
                {
                    _isFirstTime = !context.Users.Any();
                    _hasBeenRun = true;
                }
                return _isFirstTime;
            }
        }
        set => _isFirstTime = value;
    }
}