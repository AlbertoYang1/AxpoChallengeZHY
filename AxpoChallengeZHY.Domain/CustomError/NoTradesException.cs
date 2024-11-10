namespace AxpoChallengeZHY.Domain.CustomError;

public class NoTradesException : Exception
{
    public string ErrorMessage { get; }

    public NoTradesException(string errorMessage) : base(errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public NoTradesException(string errorMessage, Exception innerException) : base(errorMessage, innerException)
    {
        ErrorMessage = errorMessage;
    }


}
