using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Entities;

public class Admin: User
{
    public Admin(string fullName, string Login, string password) :
        base(fullName, Login, password, UserRole.Admin) {}
}