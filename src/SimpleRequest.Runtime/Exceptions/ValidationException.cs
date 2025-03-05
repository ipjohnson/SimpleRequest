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