using System.Collections;
using WaybillWpf.Core.Enums;

namespace WaybillWpf.Core.Entities;

public class Waybill: BaseEntity
{
    public int DriverId { get; set; }
    public Driver? Driver { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public int CarId { get; set; }
    public Car? Car { get; set; }

    public WaybillStatus WaybillStatus { get; set; } = WaybillStatus.Draft;
    
    public ICollection<WaybillDetails> WaybillDetails { get; set; } = new List<WaybillDetails>();
}