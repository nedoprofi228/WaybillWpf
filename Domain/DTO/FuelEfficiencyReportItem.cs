namespace WaybillWpf.Domain.DTO;

public class FuelEfficiencyReportItem
{
    /// <summary>
    /// Модель автомобиля
    /// </summary>
    public string CarModel { get; set; } = string.Empty;

    /// <summary>
    /// Общий пробег за период (справочно)
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// Фактический расход (сумма введенных значений)
    /// </summary>
    public float FactFuel { get; set; }

    /// <summary>
    /// Нормативный расход (расчитан как: Время в пути * Норма л/ч)
    /// </summary>
    public float NormFuel { get; set; }

    /// <summary>
    /// Разница: Факт - Норма.
    /// Положительное значение = Перерасход.
    /// Отрицательное значение = Экономия.
    /// </summary>
    public float Difference { get; set; }
}