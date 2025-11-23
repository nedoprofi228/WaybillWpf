using System;
using System.ComponentModel.DataAnnotations;

namespace WaybillWpf.Domain.Entities;

public class WaybillTask : BaseEntity
{
    // Связь с путевым листом
    public int WaybillId { get; set; }
    public Waybill? Waybill { get; set; }

    // Гр. 21 - Дата (число, месяц)
    public DateTime Date { get; set; } = DateTime.Now;

    // Гр. 22 - Откуда
    [Required] public string DeparturePoint { get; set; } = string.Empty;

    // Гр. 23 - Куда
    [Required] public string ArrivalPoint { get; set; } = string.Empty;

    // Гр. 24 - Пробег, км
    public float Mileage { get; set; }

    // Гр. 25 - Наименование и подпись (штамп) заказчика
    // В программе будем хранить просто как текстовое поле (название фирмы/ФИО)
    public string CustomerName { get; set; } = string.Empty;
    
    public string? OtherInfo { get; set; }
}