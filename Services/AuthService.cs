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

    public async Task<User?> LoginAsync(User newUser)
    {
        var user = await _usersRepository.GetUserByNameAndPasswordAsync(
            newUser.Login,
            newUser.Password
        );

        if (user == null)
        {
            return null; // Вход не удался
        }

        return user;
    }

    public async Task<User?> RegisterAsync(User newUser)
    {
        if (
            await _usersRepository.GetUserByNameAndPasswordAsync(
                newUser.Login,
                newUser.Password
            ) != null
        )
        {
            return null;
        }

        await _usersRepository.AddAsync(newUser);

        return newUser;
    }
}
