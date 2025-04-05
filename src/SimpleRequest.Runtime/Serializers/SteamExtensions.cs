using System.Text;

namespace SimpleRequest.Runtime.Serializers;

public static class SteamExtensions {
    public static ValueTask WriteStringAsync(
        this Stream stream, string value, CancellationToken cancellationToken = default) {
        return stream.WriteAsync(Encoding.UTF8.GetBytes(value), cancellationToken);
    }
}