using System.Threading.Tasks;
using WaybillWpf.Domain.DTO;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services;

public class AuthService : IAuthService
{
    private readonly IUsersRepository _usersRepository;

    // Используем DI для получения репозитория
    public AuthService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<User?> LoginAsync(AuthUserData userData)
    {
        var user = await _usersRepository.GetUserByNameAndPasswordAsync(
            userData.Name,
            userData.Password
        );

        if (user == null)
        {
            return null; // Вход не удался
        }

        return user;
    }

    public async Task<User?> RegisterAsync(AuthUserData newUserData)
    {
        if (
            await _usersRepository.GetUserByNameAndPasswordAsync(
                newUserData.Name,
                newUserData.Password
            ) != null
        )
        {
            return null;
        }

        User newUser = new User()
        {
            Name = newUserData.Name,
            Password = newUserData.Password,
            Role = UserRole.Employee,
        };

        await _usersRepository.AddAsync(newUser);

        return newUser;
    }
}
