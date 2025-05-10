using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Impl;

public class OperationParameters : IOperationParameters {
    private readonly object?[] _values;
    private readonly IReadOnlyList<IOperationParameterInfo> _parameters;

    public OperationParameters(IReadOnlyList<IOperationParameterInfo> parameters) {
        _parameters = parameters;
        _values = new object?[_parameters.Count];
    }

    public bool TryGetParameter(string parameterName, out object? parameterValue) {
        for (var i = 0; i < _parameters.Count; i++) {
            if (_parameters[i].Name == parameterName) {
                parameterValue = _values[i];
                return true;
            }
        }
        parameterValue = null;
        return false;
    }

    public bool TrySetParameter(string parameterName, object parameterValue) {
        for (var i = 0; i < _parameters.Count; i++) {
            if (_parameters[i].Name == parameterName) {
                _values[i] = parameterValue;
                return true;
            }
        }
        return false;
    }

    public IReadOnlyList<IOperationParameterInfo> Parameters => _parameters;

    public int ParameterCount => _parameters.Count;

    public IOperationParameters Clone() {
        return new OperationParameters(_parameters);
    }

    public T? Get<T>(int index, T? defaultValue = default) {
        if (_values.Length <= index) {
            throw new IndexOutOfRangeException();
        }

        if (_values[index] is T tValue) {
            return tValue;
        }
        
        return defaultValue;
    }

    public void Set(object? value, int index) {
        if (_values.Length <= index) {
            throw new IndexOutOfRangeException();
        }
        
        _values[index] = value;
    }
}