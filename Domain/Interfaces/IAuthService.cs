using WaybillWpf.Domain.DTO;
using WaybillWpf.Domain.Entities;

namespace WaybillWpf.Domain.Interfaces;

public interface IAuthService
{
    public Task<User?> LoginAsync(AuthUserData userData);
    public Task<User?> RegisterAsync(AuthUserData userData);
}