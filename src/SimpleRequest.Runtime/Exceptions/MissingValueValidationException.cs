namespace SimpleRequest.Runtime.Exceptions;

public class MissingValueValidationException(string fieldName) :
    ValidationException($"Missing value for field '{fieldName}'", fieldName) {
    
}