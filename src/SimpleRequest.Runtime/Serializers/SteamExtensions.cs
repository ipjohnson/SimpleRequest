using System.Text;

namespace SimpleRequest.Runtime.Serializers;

public static class SteamExtensions {
    public static void WriteString(
        this Stream stream, string value) {
        stream.Write(Encoding.UTF8.GetBytes(value));
    }
}