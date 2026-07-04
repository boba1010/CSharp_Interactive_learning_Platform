using System.Net;

namespace CSharp_Interactive_Learning_App.Shared.Models
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public string? ErrorMessage { get; }
        public HttpStatusCode StatusCode { get; }

        private ServiceResult(bool isSuccess, T? data, string? errorMessage, HttpStatusCode statusCode)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        public static ServiceResult<T> Success(T data) => new(true, data, null, HttpStatusCode.OK);
        public static ServiceResult<T> Failure(string errorMessage = null, HttpStatusCode statusCode = HttpStatusCode.Unauthorized) => new(false, default, errorMessage, statusCode);
    }
}
