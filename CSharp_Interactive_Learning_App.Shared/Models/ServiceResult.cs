using System.Net;

namespace CSharp_Interactive_Learning_App.Shared.Models;

public class ServiceResult
{
    public bool IsSuccess { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public HttpStatusCode StatusCode { get; protected set; }

    public ServiceResult(bool isSuccess, string? errorMessage, HttpStatusCode statusCode)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static ServiceResult Success() => new(true, null, HttpStatusCode.OK);
    public static ServiceResult Failure(string errorMsg = null!, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new(false, errorMsg, statusCode);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    private ServiceResult(bool isSuccess, T? data, string? errorMessage, HttpStatusCode statusCode) : base(isSuccess, errorMessage, statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static ServiceResult<T> Success(T data) => new(true, data, null, HttpStatusCode.OK);
    public new static ServiceResult<T> Failure(string errorMessage = null, HttpStatusCode statusCode = HttpStatusCode.Unauthorized) => new(false, default, errorMessage, statusCode);
}
