namespace SimpleRequest.Runtime.Exceptions;

public class AccessForbiddenException(string message) : 
    GeneralRequestException(403, message);