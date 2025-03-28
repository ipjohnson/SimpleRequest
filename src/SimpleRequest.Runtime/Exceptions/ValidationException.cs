namespace SimpleRequest.Runtime.Exceptions;

public class ValidationException : GeneralRequestException {
    
    public ValidationException(string message) : base(400, message) {
        FieldName = "";
    }
    
    public ValidationException(string message, string fieldName) : base(400, message) {
        FieldName = fieldName;

    }
    public string FieldName {
        get;
    }
}

public class MissingValueValidationException(string fieldName) :
    ValidationException($"Missing value for field '{fieldName}'", fieldName);
    
public class InvalidValueValidationException(string fieldName) : 
    ValidationException($"{fieldName} is invalid value", fieldName);