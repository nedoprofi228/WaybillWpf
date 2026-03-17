using ClosedXML.Excel;
using System;
using System.Linq;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services;

public class WaybillExcelExporter : IConvertToExcelService
{
    /// <summary>
    /// Экспорт путевого листа в Excel-шаблон.
    /// </summary>
    /// <param name="waybill">Главная модель путевого листа</param>
    /// <param name="templatePath">Путь до исходного шаблона (.xlsx)</param>
    /// <param name="outputPath">Путь, куда сохранить заполненный файл (.xlsx)</param>
    public void ExportToExcel(Waybill waybill, string templatePath, string outputPath)
    {
        // 1. Проверки на null
        if (waybill == null) throw new ArgumentNullException(nameof(waybill));
        if (string.IsNullOrWhiteSpace(templatePath)) throw new ArgumentException("Укажите путь к шаблону", nameof(templatePath));
        if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException("Укажите путь для сохранения", nameof(outputPath));

        // Открываем шаблон с использованием using для автоматического освобождения ресурсов
        using (var workbook = new XLWorkbook(templatePath))
        {
            // Находим нужный лист (первый)
            var worksheet = workbook.Worksheet(1);

            // ---------------------------------------------------------
            // 2. Заполнение шапки (если применимо, строки 1-4)
            // Эти адреса ячеек (B1, B2, B3) взяты для примера, Вы можете их поменять.
            // ---------------------------------------------------------
            if (waybill.Driver != null)
            {
                worksheet.Cell("B1").Value = $"Водитель: {waybill.Driver.FullName}";
            }

            if (waybill.Car != null)
            {
                worksheet.Cell("B2").Value = $"Автомобиль: {waybill.Car.Model} (гос. номер {waybill.Car.CarNumber})";
            }

            if (waybill.Logist != null)
            {
                worksheet.Cell("B3").Value = $"Логист: {waybill.Logist.FullName}";
            }

            // ---------------------------------------------------------
            // 3. Заполнение табличной части (Цикл)
            // ---------------------------------------------------------
            int currentRowIndex = 5; // Первая пустая строка для вставки по условию
            bool isFirstRow = true;

            // Преобразуем коллекции в списки для удобной итерации
            var tasks = waybill.WaybillTasks.ToList();
            var detailsList = waybill.WaybillDetails.ToList();

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                // Берем соответствующие детали (если они соотносятся 1 к 1)
                var details = i < detailsList.Count ? detailsList[i] : null;

                // КРИТИЧЕСКИ ВАЖНО: Для каждой новой записи (кроме самой первой)
                // вставляем строку со смещением вниз
                if (!isFirstRow)
                {
                    // Вставляем пустую строку под предыдущей и смещаем "Итого" вниз.
                    // Форматирование копируется из строки currentRowIndex - 1.
                    worksheet.Row(currentRowIndex - 1).InsertRowsBelow(1);
                }

                // ---------------------------------------------------------
                // 4. Маппинг колонок в текущей строке
                // ---------------------------------------------------------
                // Колонка 1 (Число): Task.Date.Day
                worksheet.Cell(currentRowIndex, 1).Value = task.Date.Day;

                if (details != null)
                {
                    // Колонка 2 (Время выезда): Details.DepartureDateTime
                    worksheet.Cell(currentRowIndex, 2).Value = details.DepartureDateTime.ToString("HH:mm");

                    // Колонка 3 (Время возвращения): Details.ArrivalDateTime
                    worksheet.Cell(currentRowIndex, 3).Value = details.ArrivalDateTime.ToString("HH:mm");

                    // Колонка 5 (Спидометр при выезде): Details.StartMealing
                    worksheet.Cell(currentRowIndex, 5).Value = details.StartMealing;

                    // Колонка 8 (Остаток при выезде): Details.StartRemeaningFuel
                    worksheet.Cell(currentRowIndex, 8).Value = details.StartRemeaningFuel;

                    // Колонка 10 (Заправлено): Details.RefueledAmount
                    worksheet.Cell(currentRowIndex, 10).Value = details.RefueledAmount;

                    // Колонка 12 (Расход фактически): Details.FuelConsumed
                    worksheet.Cell(currentRowIndex, 12).Value = details.FuelConsumed;
                }

                // Колонка 7 (Пробег): Task.Mileage
                worksheet.Cell(currentRowIndex, 7).Value = task.Mileage;

                // Дополнительно можно заполнить "Откуда/Куда"
                // worksheet.Cell(currentRowIndex, 4).Value = $"{task.DeparturePoint} - {task.ArrivalPoint}";

                currentRowIndex++;
                isFirstRow = false;
            }

            // ---------------------------------------------------------
            // 5. Сохранение результата
            // ---------------------------------------------------------
            workbook.SaveAs(outputPath);
        }
    }
}
