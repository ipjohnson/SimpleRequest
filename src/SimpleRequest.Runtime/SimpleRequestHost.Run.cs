using DependencyModules.Runtime.Interfaces;

namespace SimpleRequest.Runtime;

public partial class SimpleRequestHost {
    public static void Run<T>() where T : IDependencyModule, new() {
        
    }

    public static void Run<T, TArg>(TArg arg) where T : IDependencyModule, new() {
        
    }
}