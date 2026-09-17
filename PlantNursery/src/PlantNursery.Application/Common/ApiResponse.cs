namespace PlantNursery.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(
        bool success,
        string message,
        T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public static ApiResponse<T> Ok(
        T data,
        string message = "Request completed successfully.")
    {
        return new ApiResponse<T>(
            true,
            message,
            data);
    }

    public static ApiResponse<T> Fail(
        string message)
    {
        return new ApiResponse<T>(
            false,
            message,
            default);
    }
}
public class ApiResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public object? Data { get; set; }

    public static ApiResponse Ok(
        string message = "Request completed successfully.",
        object? data = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse Fail(
        string message)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Data = null
        };
    }
}