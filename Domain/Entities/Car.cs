using System.ComponentModel.DataAnnotations;

namespace WaybillWpf.Domain.Entities;

public class Car : BaseEntity
{
    [Required]
    public string Model { get; set; } = string.Empty;
    public string CarNumber { get; set; } = string.Empty;
    public float FuelRate { get; set; }

    public int? FuelTypeId { get; set; }
    public FuelType? FuelType { get; set; }
}