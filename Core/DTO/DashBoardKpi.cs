namespace WaybillWpf.Core.DTO;

public class DashboardKpi
{
    public int TotalWaybillsCompleted { get; set; } // Всего "документов"
    public int TotalTripsCompleted { get; set; }    // Всего "отрезков пути"
    public float TotalMileage { get; set; }
    public float TotalFuelSpent { get; set; }
    public float TotalFuelDifference { get; set; } // План/Факт ( < 0 экономия)
}