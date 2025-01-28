namespace DohClient;

public class Resolver(HttpClient httpClient)
{
    private readonly string _dohEndpoint = Environment.GetEnvironmentVariable("DOH_ENDPOINT") ?? "https://8.8.8.8/dns-query";
    
    public async ValueTask<int> Resolve(
        Memory<byte> request, 
        Memory<byte> response,
        CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post, 
            _dohEndpoint);
        var requestId = GetRequestId(request);
        request.Span[0] = 0;
        request.Span[1] = 0;

        using var payload = new ReadOnlyMemoryContent(request);
        payload.Headers.ContentType = new ("application/dns-message");
        requestMessage.Content = payload;
        
        using var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            return -1;

        await using var responseStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
        var ret = await responseStream.ReadAsync(response, cancellationToken);
        response.Span[0] = requestId.Item1;
        response.Span[1] = requestId.Item2;

        return ret;
    }

    private static (byte, byte) GetRequestId(Memory<byte> request)
    {
        return (request.Span[0], request.Span[1]);
    }
}