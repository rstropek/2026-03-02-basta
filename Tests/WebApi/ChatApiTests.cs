using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.WebApi;

public class ChatApiTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task Chat_SimpleMessage_ReturnsSseStream()
    {
        var client = fixture.HttpClient;

        // Create a new conversation
        var createResponse = await client.PostAsync("/conversations", null);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var conversationId = createBody.GetProperty("conversationId").GetInt32();

        // Send a chat message and read the SSE stream
        var request = new HttpRequestMessage(HttpMethod.Post, $"/conversations/{conversationId}/chat")
        {
            Content = JsonContent.Create(new { message = "Hi!" })
        };
        request.Headers.Accept.ParseAdd("text/event-stream");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        // Read the full SSE body and verify structure
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("event: textDelta", body);
        Assert.Contains("data:", body);

        // Verify there is actual text content in at least one data line
        var dataLines = body.Split('\n')
            .Where(l => l.StartsWith("data:"))
            .ToList();
        Assert.NotEmpty(dataLines);
    }
}
