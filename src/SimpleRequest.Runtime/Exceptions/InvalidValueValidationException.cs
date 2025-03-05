namespace SimpleRequest.Runtime.Exceptions;

public class InvalidValueValidationException(string fieldName) : 
    ValidationException($"{fieldName} is invalid value", fieldName) {
}