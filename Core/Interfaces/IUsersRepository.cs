using WaybillWpf.Core.Entities;

namespace WaybillWpf.Core.Interfaces;

public interface IUsersRepository: IBaseRepository<User>
{
    public Task<User?> GetUserByNameAndPasswordAsync(string username, string password);
}