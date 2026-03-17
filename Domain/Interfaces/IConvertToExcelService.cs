using WaybillWpf.Domain.Entities;

namespace WaybillWpf.Domain.Interfaces;

public interface IConvertToExcelService
{
    public void ExportToExcel(Waybill waybill, string templatePath, string outputPath);
}