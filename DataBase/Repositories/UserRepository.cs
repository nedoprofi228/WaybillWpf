using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class UserRepository(ApplicationContext context) : BaseRepository<User>(context), IUsersRepository
{
    public async Task<User?> GetUserByNameAndPasswordAsync(string username, string password)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Name == username && u.Password == password);
    }
}