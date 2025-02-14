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
    /// Get value by index
    /// </summary>
    /// <param name="index"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Get<T>(int index);

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="value"></param>
    /// <param name="index"></param>
    void Set(object value, int index);
}