namespace SimpleRequest.Runtime.Invoke;

public static class RequestFilterOrder {
    public const int Init = -10000;

    public const int FullRequestMetrics = -7000;

    public const int RetryFilter = -5000;

    public const int BeforeSerialize = -2;

    public const int BindParameters = -1;

    public const int First = 1;

    public const int Second = 2;

    public const int Third = 3;

    public const int Normal = 100;

    public const int Last = int.MaxValue;
}

public interface IRequestFilter {
    Task Invoke(IRequestChain requestChain);
}


public delegate IRequestFilter FilterProvider(IRequestContext context);

public record RequestFilterInfo(
    FilterProvider FilterProvider,
    int Order =  RequestFilterOrder.Normal);