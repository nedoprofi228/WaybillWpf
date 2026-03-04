using System.ComponentModel.DataAnnotations;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Entities;

public class Driver: User
{
    
    public int DriverLicenseId { get; set; }
    public DriveLicense? DriveLicense { get; set; }
    
    public ICollection<Waybill> Waybills { get; set; } = [];
    
    public Driver(string fullName, string Login, string password) :
        base(fullName, Login, password, UserRole.Driver) { }
    
    public Driver() {}
}