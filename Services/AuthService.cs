using WaybillWpf.Core.DTO;
using WaybillWpf.Core.Entities;
using WaybillWpf.Core.Interfaces;
using System.Threading.Tasks;
using WaybillWpf.Core.Enums;

namespace WaybillWpf.Services;

public class AuthService : IAuthService
{
    private readonly IUsersRepository _usersRepository;

    // Используем ваш ServicesProvider для получения репозитория
    public AuthService()
    {
        _usersRepository = ServicesProvider.GetService<IUsersRepository>()
                           ?? throw new InvalidOperationException("IUsersRepository is not registered.");
    }

    public async Task<User?> LoginAsync(AuthUserData userData)
    {
        // ⚠️ ВАЖНО: См. замечание о безопасности ниже!
        var user = await _usersRepository.GetUserByNameAndPasswordAsync(userData.Name, userData.Password);

        if (user == null)
        {
            return null; // Вход не удался
        }

        return user;
    }

    public async Task<User?> RegisterAsync(AuthUserData newUserData)
    {
        if(await _usersRepository.GetUserByNameAndPasswordAsync(newUserData.Name, newUserData.Password) != null)
            return null;
        
        User newUser = new User()
        {
            Name = newUserData.Name,
            Password = newUserData.Password,
            Role = UserRole.Employee
        };
        
        await _usersRepository.AddAsync(newUser);

        return newUser;
    }
}