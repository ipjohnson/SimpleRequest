namespace SimpleRequest.Runtime.Host;

public interface IRequestHost {
    void Run();
}

public interface IRequestHost<in TArg> {
    void Run(TArg arg);
}