namespace WaybillWpf.Core.DTO;

public class DriverDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime LicenseExpiration { get; set; }
}