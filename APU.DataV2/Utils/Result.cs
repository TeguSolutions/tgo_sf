using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace APU.DataV2.Utils;

[DataContract]
public class Result
{
    [JsonConstructor]
    public Result(bool success, StatusCode statusCode, string message)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
    }

    [DataMember]
    public bool Success { get; }
    [DataMember]
    public StatusCode StatusCode { get; }
    [DataMember]
    public string Message { get; }


    public static Result Ok()
    {
        return new Result(true, StatusCode.OK, "");
    }
    public static Result OkWithErrors(string message)
    {
        return new Result(true, StatusCode.OK, message);
    }
    public static Result Fail()
    {
        return new Result(false, StatusCode.BadRequest, "");
    }

    public static Result Fail(string message)
    {
        return new Result(false, StatusCode.BadRequest, message);
    }

    public static Result InsufficientPrivilege()
    {
        return new Result(false, StatusCode.Forbidden, "Insufficient Privilege");
    }

    public static Result ServerError()
    {
        return new Result(false, StatusCode.InternalServerError, "Unexpected error happened. Try again or contact with support.");
    }
}

[DataContract]
public class Result<T> : Result
{
    public Result(bool success, StatusCode statusCode, string message) : base(success, statusCode, message)
    {

    }

    [JsonConstructor]
    public Result(bool success, StatusCode statusCode, string message, T data) : base(success, statusCode, message)
    {
        Data = data;
    }

    [DataMember]
    public T Data { get; }

    public new static Result<T> Ok()
    {
        return new Result<T>(true, StatusCode.OK, "", default);
    }
    
    public static Result<T> OkMessage(string message)
    {
        return new Result<T>(true, StatusCode.OK, message, default);
    }

    public static Result<T> OkData(T data)
    {
        return new Result<T>(true, StatusCode.OK, "", data);
    }

    public static Result<T> OkErrors(string message)
    {
        return new Result<T>(true, StatusCode.OK, message);
    }

    public new static Result<T> Fail()
    {
        return new Result<T>(false, StatusCode.BadRequest, "");
    }
    public static Result<T> FailMessage(string message)
    {
        return new Result<T>(false, StatusCode.BadRequest, message);
    }
    public static Result<T> FailData(T data)
    {
        return new Result<T>(false, StatusCode.BadRequest, "", data);
    }

    public new static Result<T> InsufficientPrivilege()
    {
        return new Result<T>(false, StatusCode.Forbidden, "Insufficient Privilege");
    }

    public new static Result<T> ServerError()
    {
        return new Result<T>(false, StatusCode.InternalServerError, "Unexpected error happened.\r\n Try again or contact with support.");
    }
}

public static class ResultExtensions
{
    public static bool IsSuccess(this Result response)
    {
        return response is not null && response.Success && response.StatusCode == StatusCode.OK;
    }

    public static bool IsSuccess<T>(this Result<T> response)
    {
        return response is not null && response.Success && response.StatusCode == StatusCode.OK;
    }
}
