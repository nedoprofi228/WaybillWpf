
using System.Windows.Documents;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;
using Xceed.Words.NET;
using Table = Xceed.Document.NET.Table;
using Row = Xceed.Document.NET.Row;
using System.Linq;
using Xceed.Document.NET;

namespace WaybillWpf.Services;

public class ConvertToWordService : IConvertToWordService
{
    // Вспомогательный метод для обновления текста в ячейке
    private void FillCell(Cell cell, string? text)
    {
        if (cell == null || text == null) return;
        
        var paragraph = cell.Paragraphs.FirstOrDefault();
        if (paragraph == null)
        {
            paragraph = cell.InsertParagraph();
        }

        if (paragraph.Text.Length > 0)
        {
            paragraph.ReplaceText(paragraph.Text, string.Empty);
        }

        Formatting formatting = new Formatting();
        formatting.Size = 9;
        formatting.Spacing = 0;

        paragraph.Append(text, formatting);
    }

    public void ExportToWord(Waybill waybill, string templatePath, string outputPath)
    {
        using (DocX document = DocX.Load(templatePath))
        {
            // Предполагаем, что таблица с путевым листом - это ПЕРВАЯ таблица в документе (индекс 0)
            if (document.Tables.Count == 0)
            {
                throw new Exception("В шаблоне не найдена таблица!");
            }

            Table table = document.Tables[0];

            int insertIndex = table.RowCount - 1;
            
            // Берем строку-шаблон, из которой скопируем стили (шрифты, границы, выравнивание)
            Row patternRow = table.Rows[table.RowCount - 2];

            var details = waybill.WaybillDetails.FirstOrDefault();

            foreach (var detail in waybill.WaybillDetails)
            {
                // Вставляем копию строки с сохранением форматирования (передаем true)
                Row newRow = table.InsertRow(patternRow, insertIndex, true);
                
                // Заполняем новую строку (текст заменяется, стили от patternRow остаются)
                FillCell(newRow.Cells[0], detail.DepartureDateTime.ToString("dd.MM"));
                FillCell(newRow.Cells[1], detail.DepartureDateTime.ToString("HH:mm"));
                FillCell(newRow.Cells[2], detail.ArrivalDateTime.ToString("HH:mm"));
                FillCell(newRow.Cells[3], (detail.ArrivalDateTime - detail.DepartureDateTime).ToString(@"hh\:mm"));
                FillCell(newRow.Cells[4], detail.StartMealing.ToString());
                FillCell(newRow.Cells[5], detail.EndMealing.ToString());
                FillCell(newRow.Cells[6], (detail.EndMealing - detail.StartMealing).ToString());
                FillCell(newRow.Cells[7], detail.StartRemeaningFuel.ToString());
                FillCell(newRow.Cells[8], detail.EndRemeaningFuel.ToString());
                FillCell(newRow.Cells[9], detail.RefueledAmount.ToString());
                FillCell(newRow.Cells[10], detail.NormalFuelConsumed.ToString("F1"));
                FillCell(newRow.Cells[11], detail.FuelConsumed.ToString());

                // Если есть связь с WaybillDetails для конкретной задачи, заполняем данные о топливе/времени
                if (details != null)
                {
                    // Время выезда (Колонка 2) -> индекс 4 для примера
                    FillCell(newRow.Cells[4], details.DepartureDateTime.ToString("HH:mm"));

                    // Остаток при выезде (Колонка 8) -> индекс 5
                    FillCell(newRow.Cells[5], details.StartRemeaningFuel.ToString("F1"));
                }

                // После вставки строки сдвигаем индекс вниз, чтобы следующая строка вставилась под текущей
                insertIndex++;
            }

            Table taskTable = document.Tables[1];

            int taskInsertIndex = taskTable.RowCount - 1;
            
            // Берем последнюю строку второй таблицы в качестве шаблона
            Row taskPatternRow = taskTable.Rows[taskTable.RowCount - 1];

            foreach (var task in waybill.WaybillTasks)
            {
                // Вставляем копию строки с сохранением форматирования
                Row newRow = taskTable.InsertRow(taskPatternRow, taskInsertIndex, true);
                
                // Заполняем ячейки второй таблицы аналогично
                FillCell(newRow.Cells[0], task.Date.ToString("dd.MM"));
                FillCell(newRow.Cells[1], task.DeparturePoint);
                FillCell(newRow.Cells[2], task.ArrivalPoint);
                FillCell(newRow.Cells[3], task.Mileage.ToString());
                FillCell(newRow.Cells[4], task.CustomerName?.ToString());
                FillCell(newRow.Cells[4], task.OtherInfo?.ToString());
                
                // Исправлена опечатка: нужно увеличивать taskInsertIndex, а не insertIndex
                taskInsertIndex++;
            }

            document.ReplaceText("{waybillNum}", waybill.Id.ToString());
            document.ReplaceText("{startDate}", waybill.WaybillDetails?.FirstOrDefault()?.DepartureDateTime.ToString("dd.MM.yyyy"));
            document.ReplaceText("{endDate}", waybill.WaybillDetails?.LastOrDefault()?.ArrivalDateTime.ToString("dd.MM.yyyy"));
            document.ReplaceText("{currentYear}", DateTime.Now.Year.ToString());
            
            // --- ЗАПОЛНЕНИЕ ТЕКСТА ВНЕ ТАБЛИЦЫ (Опционально) ---
            // DocX позволяет легко менять текст по всему документу с помощью ReplaceText
            if (waybill.Driver != null)
            {
                // Например, если в шаблоне вы напишете текст "{DriverName}"
                document.ReplaceText("{driverName}", waybill.Driver.FullName);
                document.ReplaceText("{licenceNumber}", waybill.Driver.DriveLicense.LicenseNumber.ToString());
                
            }

            if (waybill.Car != null)
            {
                document.ReplaceText("{CarModel}", waybill.Car.Model);
                document.ReplaceText("{carNumber}", waybill.Car.CarNumber);
            }

            // Сохраняем заполненный документ в новый файл
            document.SaveAs(outputPath);
        }
    }
}
