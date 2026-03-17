using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Entities;

public class Logist : User
{

    public ICollection<Waybill> Waybills { get; set; } = [];

    public Logist(string fullName, string Login, string password) :
        base(fullName, Login, password, UserRole.Manager)
    {

    }

    public Logist() { }
}