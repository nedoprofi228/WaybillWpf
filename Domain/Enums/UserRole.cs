
using System.ComponentModel;

namespace WaybillWpf.Domain.Enums;

public enum UserRole
{
    [Description("Логист")]
    Manager,
    Admin,
    
    [Description("Водитель")]
    Driver
}