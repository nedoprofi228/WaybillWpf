using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Entities;

public class User: BaseEntity
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    [Required]
    public UserRole Role { get; set; }

    public ICollection<Waybill> Waybills { get; set; } = [];
}