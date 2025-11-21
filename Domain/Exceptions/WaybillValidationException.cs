namespace WaybillWpf.Domain.Exceptions;

public class WaybillValidationException(string message) : WaybillFlowException(message)
{
}