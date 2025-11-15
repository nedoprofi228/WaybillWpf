using WaybillWpf.Core.DTO;
using WaybillWpf.Core.Entities;

namespace WaybillWpf.Core.Interfaces;

public interface IAuthService
{
    public Task<User?> LoginAsync(AuthUserData userData);
    public Task<User?> RegisterAsync(AuthUserData userData);
}