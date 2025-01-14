using BeeSafeWeb.Data;
using BeeSafeWeb.Models;

namespace BeeSafeWeb.Authentication;

public interface IAuthService
{
    bool ValidateUser(string email, string password, out User user);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public bool ValidateUser(string email, string password, out User user)
    {
        user = _userRepository.GetUserByEmail(email);
        if (user == null) return false;

        // Replace with a proper hashing mechanism
        return user.Password == password;
    }
}