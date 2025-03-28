namespace SimpleRequest.Runtime.Exceptions;

public class UnauthorizedAccessException(string message) : 
    GeneralRequestException(401, message);