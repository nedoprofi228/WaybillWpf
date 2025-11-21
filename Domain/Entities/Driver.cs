using System.ComponentModel.DataAnnotations;

namespace WaybillWpf.Domain.Entities;

public class Driver: BaseEntity
{
    [Required]
    public string DriverName { get; set; }
    
    public int DriverLicenseId { get; set; }
    public DriveLicense? DriveLicense { get; set; }
}