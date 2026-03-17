namespace WaybillWpf.Domain.DTO;

public class DashboardKpi
{
    /// <summary>
    /// Общее количество путевых листов за период
    /// </summary>
    public int TotalWaybills { get; set; }

    /// <summary>
    /// Суммарный пробег (км)
    /// </summary>
    public float TotalMileage { get; set; }

    /// <summary>
    /// Суммарный расход топлива (ФАКТ, введенный вручную)
    /// </summary>
    public float TotalFuelConsumed { get; set; }

    /// <summary>
    /// Суммарные затраты на топливо (руб)
    /// </summary>
    public decimal TotalFinancialCost { get; set; }
}