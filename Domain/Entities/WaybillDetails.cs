using System.ComponentModel.DataAnnotations.Schema;

namespace WaybillWpf.Domain.Entities;

public class WaybillDetails : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int WaybillId { get; set; }
    public Waybill? Waybill { get; set; }

    // ВРЕМЯ
    public DateTime DepartureDateTime { get; set; }
    public DateTime ArrivalDateTime { get; set; }

    // СПИДОМЕТР (Вручную)
    public float StartMealing { get; set; }
    public float EndMealing { get; set; }

    // ОСТАТКИ В БАКЕ (Вручную)
    public float StartRemeaningFuel { get; set; }
    public float EndRemeaningFuel { get; set; }

    // ФАКТИЧЕСКИЙ РАСХОД (Вручную)
    // Пользователь вводит сам, т.к. могли быть заправки или сливы
    public float FuelConsumed { get; set; }

    // НОРМА (Автоматически)
    // Не храним в БД, считаем на лету
    [NotMapped]
    public double NormalFuelConsumed => 
        Waybill?.Car != null 
            ? InWayTime.TotalHours * Waybill.Car.FuelRate 
            : 0;

    [NotMapped]
    public TimeSpan InWayTime => ArrivalDateTime - DepartureDateTime;

    [NotMapped]
    public float Distance => EndMealing - StartMealing;
}