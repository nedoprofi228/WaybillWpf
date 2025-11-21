namespace WaybillWpf.Domain.DTO;

public class MileageReportItem
{
    public string EntityName { get; set; } // Имя машины или водителя
    public float TotalMileage { get; set; }
    public int TotalTrips { get; set; } // Кол-во "плеч" (WaybillDetails)
}