namespace AxpoChallengeZHY.Domain.CustomError;

public class NoTradesException(string errorMessage) : Exception(errorMessage);
