using System;

namespace Mp3Player.Models;

public class MethodResult
{
    public bool IsSuccess { get; set; }
    public object? Result { get; set; }
    public string? Error { get; set; }
    
    public MethodResult(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public MethodResult(object? resultObject)
    {
        Result = resultObject;
        IsSuccess = true;
    }
    
    public static MethodResult Success()
    {
        return new MethodResult(true);
    }
    
    public static MethodResult Success<T>(T resultObject)
    {
        return new MethodResult(resultObject);
    }
    
    public static MethodResult Failure(string? error)
    {
        return new MethodResult(false, error);
    }
}