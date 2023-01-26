using System;
namespace ApiGenerator.Exceptions;

public class ApiException : Exception
{
    public ApiException(
      string message,
      int statusCode = 400,
      int? errorCode = null,
      Exception innerException = null)
      : base(message, innerException)
    {
        this.StatusCode = statusCode;
        this.ErrorCode = errorCode;
    }

    public int? ErrorCode { get; private set; }

    public int StatusCode { get; private set; }
}

public class ApiException<TResult> : ApiException
{
    public ApiException(
      string message,
      int statusCode,
      TResult result,
      int? errorCode = null,
      Exception innerException = null)
      : base(message, statusCode, errorCode, innerException)
    {
        Result = result;
    }

    public TResult Result { get; private set; }
}
