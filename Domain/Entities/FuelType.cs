using System.ComponentModel.DataAnnotations;

namespace WaybillWpf.Domain.Entities;

public class FuelType : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
