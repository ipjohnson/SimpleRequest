namespace SimpleRequest.Client.Exceptions;

public class MissingChannelException(string channelName) : 
    Exception($"Could not find channel '{channelName}'");