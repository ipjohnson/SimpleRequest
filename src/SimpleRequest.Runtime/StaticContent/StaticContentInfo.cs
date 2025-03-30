namespace SimpleRequest.Runtime.StaticContent;

public record StaticContentInfo(
    string Path,
    string ETag,
    int CompressedSize,
    string ContentEncoding
    );