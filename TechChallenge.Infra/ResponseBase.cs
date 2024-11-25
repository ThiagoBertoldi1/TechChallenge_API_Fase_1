using System.Net;

namespace TechChallenge.Infrastructure;
public class ResponseBase<T>
{
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; }

    private ResponseBase(T? data)
    {
        Data = data;
        StatusCode = HttpStatusCode.OK;
    }

    private ResponseBase(HttpStatusCode statusCode, string message)
    {
        Data = default;
        StatusCode = statusCode;
        Message = message;
    }

    public static ResponseBase<T> Create(T? data) => new(data);
    public static ResponseBase<T> Fault(HttpStatusCode statusCode, string message) => new(statusCode, message);
}
