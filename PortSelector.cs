namespace DohProxy;

public static class PortSelector
{
    static PortSelector()
    {
        if (Environment.GetEnvironmentVariable("PORT") is {} portStr
            && int.TryParse(portStr, out var port))
            Port = port;
        else
            Port = 53;
    }
    
    public static int Port { get; }
}