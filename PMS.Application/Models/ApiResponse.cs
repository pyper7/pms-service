namespace PMS.Application.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, string? errorCode = null, object? errors = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode,
        Errors = errors
    };
}

public static class ApiResponse
{
    public static ApiResponse<object> Ok(string message = "Success") => new()
    {
        Success = true,
        Message = message,
        Data = null
    };

    public static ApiResponse<object> Fail(string message, string? errorCode = null, object? errors = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode,
        Errors = errors
    };
}


