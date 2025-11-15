namespace WaybillWpf.Core.Exceptions;

public class WaybillValidationException(string message) : WaybillFlowException(message)
{
}