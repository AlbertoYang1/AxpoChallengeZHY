namespace AxpoChallengeZHY.Domain.CustomError;

public class NoTradesError : Exception
{
    public string ErrorMessage { get; }

    public NoTradesError(string errorMessage) : base(errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public NoTradesError(string errorMessage, Exception innerException) : base(errorMessage, innerException)
    {
        ErrorMessage = errorMessage;
    }


}
