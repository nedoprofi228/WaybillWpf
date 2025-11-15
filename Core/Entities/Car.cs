using System.ComponentModel.DataAnnotations;

namespace WaybillWpf.Core.Entities;

public class Car: BaseEntity
{
    [Required]
    public string Model { get; set; }
    public float FuelRate { get; set; }
}