using System.Collections;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Entities;

public class Waybill: BaseEntity
{
    public DateTime AtCreated { get; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    public int DriverId { get; set; }
    public Driver? Driver { get; set; }
    
    public int LogistId { get; set; }
    public Logist? Logist { get; set; }
    
    public int CarId { get; set; }
    public Car? Car { get; set; }

    public WaybillStatus WaybillStatus { get; set; } = WaybillStatus.Draft;
    public string? ReasonOfDecline { get; set; } = null;
    
    public ICollection<WaybillDetails> WaybillDetails { get; set; } = new List<WaybillDetails>();
    public ICollection<WaybillTask> WaybillTasks { get; set; } = new List<WaybillTask>();
}