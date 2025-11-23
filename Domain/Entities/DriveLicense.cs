using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace WaybillWpf.Domain.Entities;

public class DriveLicense: BaseEntity
{
    [Required]
    public string LicenseNumber { get; set; }
    
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    
    [NotMapped]
    public Driver? Driver { get; set; }
}