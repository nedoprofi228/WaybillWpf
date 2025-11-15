namespace WaybillWpf.Core.DTO;

public class FuelEfficiencyReportItem
{
    public string CarModel { get; set; }
    public string DriverName { get; set; }
    public float TotalMileage { get; set; }      // Суммарный пробег
    public float FuelRateNorm { get; set; }      // Норматив (из Car.FuelRate)
    public float FuelPlanned { get; set; }       // План (Пробег * Норматив / 100)
    public float FuelActual { get; set; }        // Факт (сумма TotalFuel из Details)
    public float Difference { get; set; }        // Перерасход (+) или Экономия (-)
}