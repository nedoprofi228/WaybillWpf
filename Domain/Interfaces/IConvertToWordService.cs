using WaybillWpf.Domain.Entities;

namespace WaybillWpf.Domain.Interfaces;

public interface IConvertToWordService
{
    void ExportToWord(Waybill waybill, string templatePath, string outputPath);
}