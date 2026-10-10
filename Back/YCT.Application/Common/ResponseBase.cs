using System.Text.Json.Serialization;

namespace YCT.Application.Common;

public class ResponseBase<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    /// <summary>
    /// El recurso pedido no existe: el controlador responde 404 en vez de 400. No se serializa
    /// cuando es false, así que las respuestas de siempre no cambian.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool NoEncontrado { get; set; }

    public static ResponseBase<T> Ok(T data, string message = "Operación exitosa")
        => new() { Success = true, Message = message, Data = data };

    public static ResponseBase<T> Fail(string message)
        => new() { Success = false, Message = message };

    public static ResponseBase<T> NotFound(string message)
        => new() { Success = false, Message = message, NoEncontrado = true };
}
