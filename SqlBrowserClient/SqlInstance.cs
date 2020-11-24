namespace SqlBrowserClient
{
    public record SqlInstance (string ServerName, string InstanceName, bool IsClustered, string Version, int TcpPort, string NamedPipe);
}
