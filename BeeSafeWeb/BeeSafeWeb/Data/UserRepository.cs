using BeeSafeWeb.Models;

namespace BeeSafeWeb.Data;

public interface IUserRepository
{
    User GetUserByEmail(string email);
}

public class UserRepository : IUserRepository
{
    private readonly BeeSafeContext _context;

    public UserRepository(BeeSafeContext context)
    {
        _context = context;
    }

    public User GetUserByEmail(string email)
    {
        return _context.Users.SingleOrDefault(u => u.Email == email);
    }
}