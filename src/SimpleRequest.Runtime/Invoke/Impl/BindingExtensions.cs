namespace SimpleRequest.Runtime.Invoke.Impl;

public static class BindingExtensions {
    public static void BindPath(this IRequestContext context, string path, int index) {
        var parameter = context.RequestHandlerInfo?.InvokeInfo.Parameters[index];

        if (parameter != null) {
            var token = context.RequestData.PathTokenCollection.Get(path);

            context.InvokeParameters?.Set(token, index);
        }
    }
}