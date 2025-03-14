namespace SimpleRequest.RazorBlade.Impl;

public interface IRazorBladeTemplate {
    Task RenderAsync(TextWriter writer, CancellationToken cancellationToken);
}