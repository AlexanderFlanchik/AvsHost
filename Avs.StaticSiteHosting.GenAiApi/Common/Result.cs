namespace Avs.StaticSiteHosting.GenAiApi.Common;

public class Result
{
    protected Result() {}
    public string? Error { get; protected set; }
    public bool IsSuccess => string.IsNullOrEmpty(Error);
    public static Result Success() => new();
    public static Result Fail(string errorMessage) => new() { Error =  errorMessage };
}

public class Result<T> : Result
{
    public T? Data { get; private set; }
    
    private Result() {}
    
    public static Result<T> Success(T data) => new() { Data = data };
    public new static Result<T> Fail(string errorMessage) => new() { Error = errorMessage };
}