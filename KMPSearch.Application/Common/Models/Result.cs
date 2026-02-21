namespace KMPSearch.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public static Result Success() => new() { IsSuccess = true, StatusCode = 200 };

    public static Result Failure(string error, int statusCode = 400) =>
        new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };
}

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }

    public static Result<T> Success(T data, int statusCode = 200) =>
        new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static Result<T> Failure(string error, int statusCode = 400) =>
        new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };
}
