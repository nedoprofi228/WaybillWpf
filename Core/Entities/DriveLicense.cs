using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace WaybillWpf.Core.Entities;

public class DriveLicense: BaseEntity
{
    [Required]
    public string LicenseNumber { get; set; }
    
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    
    public int DriverId { get; set; }
    public Driver? Driver { get; set; }
}