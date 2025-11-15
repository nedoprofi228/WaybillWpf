using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using WaybillWpf.Core.Enums;

namespace WaybillWpf.Core.Entities;

public class User: BaseEntity
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Password { get; set; }
    public UserRole Role { get; set; }
    
    public ICollection<Waybill> Waybills { get; set; }
}