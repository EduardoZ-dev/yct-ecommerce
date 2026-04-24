namespace YCT.Application.Common;

public class ResponseBase<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ResponseBase<T> Ok(T data, string message = "Operación exitosa")
        => new() { Success = true, Message = message, Data = data };

    public static ResponseBase<T> Fail(string message)
        => new() { Success = false, Message = message };
}
