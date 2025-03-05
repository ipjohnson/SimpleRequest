using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Attributes;

/// <summary>
/// Classes that implement IRequestFilterProvider can be attributed with this attribute.
/// A filter attribute will be generated that can be used to attribute request methods
/// </summary>
public class RequestFilterAttribute : Attribute {
    /// <summary>
    /// By default a new instance of the filter will be created for each request
    /// set to true to reuse the filter between calls.
    /// </summary>
    public bool Reuse { get; set; }

    /// <summary>
    /// Filters are assigned an order of 100 by default. This is after parameter binding (-1)
    /// and before the invoke filter (int.max) 
    /// </summary>
    public int Order { get; set; } = (int)RequestFilterOrder.Normal;
}