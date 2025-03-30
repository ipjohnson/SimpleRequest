namespace SimpleRequest.Runtime.Invoke;

public interface IInvokeParameters {
    /// <summary>
    /// Try getting parameter by name
    /// </summary>
    /// <param name="parameterName"></param>
    /// <param name="parameterValue"></param>
    /// <returns></returns>
    bool TryGetParameter(string parameterName, out object? parameterValue);

    /// <summary>
    /// Try setting parameter value by name
    /// </summary>
    /// <param name="parameterName"></param>
    /// <param name="parameterValue"></param>
    /// <returns></returns>
    bool TrySetParameter(string parameterName, object parameterValue);

    /// <summary>
    /// List of parameter info object for the call
    /// </summary>
    IReadOnlyList<IInvokeParameterInfo> Parameters { get; }

    /// <summary>
    /// Count of parameters
    /// </summary>
    int ParameterCount { get; }

    /// <summary>
    /// Clone parameters
    /// </summary>
    /// <returns></returns>
    IInvokeParameters Clone();

    /// <summary>
    /// Is the value set for specific index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool HasValue(int index) {
        return Get<object>(index) != null;
    }
    
    /// <summary>
    /// Get value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? Get<T>(int index, T? defaultValue = default);

    /// <summary>
    /// Get a required value from parameters 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    T GetRequired<T>(int index, T? defaultValue = default) {
        var value = Get(index, defaultValue);
        
        return value ?? throw new Exception($"Parameter {index} not set.");
    }

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="value"></param>
    /// <param name="index"></param>
    void Set(object? value, int index);
}

public class EmptyInvokeParameters : IInvokeParameters {
    public static EmptyInvokeParameters Instance { get; } = new EmptyInvokeParameters();
    
    public bool TryGetParameter(string parameterName, out object? parameterValue) {
        parameterValue = null;
        return false;
    }

    public bool TrySetParameter(string parameterName, object parameterValue) {
        return false;
    }

    public IReadOnlyList<IInvokeParameterInfo> Parameters => Array.Empty<IInvokeParameterInfo>();

    public int ParameterCount => 0;

    public IInvokeParameters Clone() => this;

    public T? Get<T>(int index, T? defaultValue = default) {
        return defaultValue;
    }

    public void Set(object? value, int index) {
        throw new IndexOutOfRangeException("This request has no parameters.");
    }
    
    public static ParametersCreationDelegate CreationDelegate { get; } = _ => new EmptyInvokeParameters();
    
    private static readonly ValueTask _completionTask = new ValueTask();
    
    public static BindParametersDelegate BindDelegate { get; } = _ => _completionTask;
}