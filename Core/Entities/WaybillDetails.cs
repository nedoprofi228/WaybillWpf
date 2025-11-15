using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaybillWpf.Core.Entities;

public class WaybillDetails : BaseEntity
{
    public DateTime CreatedAt { get; set; }

    public int WaybillId { get; set; }
    public Waybill Waybill { get; set; } = null!;

    public DateTime DepartureDateTime { get; set; }
    public DateTime ArrivalDateTime { get; set; }

    public float StartMealing { get; set; }
    public float EndMealing { get; set; }

    public float StartRemeaningFuel { get; set; }
    public float EndRemeaningFuel { get; set; }


    [NotMapped] public TimeSpan InWayTime => ArrivalDateTime - DepartureDateTime;

    [NotMapped] public float TotalMealing => EndRemeaningFuel - StartMealing;

    [NotMapped] public float TotalFuel => EndRemeaningFuel - StartRemeaningFuel;
}
