using WaybillWpf.Domain.Entities;

namespace WaybillWpf.Domain.Interfaces;

public interface IUsersRepository : IBaseRepository<User>
{
    public Task<User?> GetUserByNameAndPasswordAsync(string username, string password);
}
