using System.Runtime.CompilerServices;
using System.Text.Json;
using ChatBotDb;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.AI;

namespace ChatBot.AgentFramework;

public static class ConversationEndpoints
{
    private const string GetConversationHistoryRouteName = "AF_GetConversationHistory";

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapAgentFrameworkConversationsEndpoints()
        {
            var api = app.MapGroup("/af/conversations");
            api.MapPost("/", AddConversation);
            api.MapPost("/{conversationId}/chat", Chat);
            api.MapGet("/{conversationId}", GetHistory).WithName(GetConversationHistoryRouteName);

            return app;
        }
    }

    public async static Task<Created<NewConversationResponse>> AddConversation(
        ApplicationDataContext context,
        LinkGenerator linkGenerator)
    {
        var conversation = new Conversation();
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();
        var url = linkGenerator.GetPathByName(GetConversationHistoryRouteName, new { conversationId = conversation.Id });
        return TypedResults.Created(url, new NewConversationResponse(conversation.Id));
    }

    public async static Task<IResult> Chat(
        ISessionRepository sessionRepository,
        AgentFrameworkManager agentManager,
        int conversationId,
        NewMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest("Message must not be empty.");
        }

        string? serializedSession;
        try
        {
            serializedSession = await sessionRepository.GetSession(conversationId);
        }
        catch (ConversationNotFoundException)
        {
            return Results.NotFound();
        }

        var messages = DeserializeSession(serializedSession);
        messages.Add(new ChatMessage(ChatRole.User, request.Message));

        return TypedResults.ServerSentEvents(
            StreamAndPersist(agentManager, sessionRepository, conversationId, messages, cancellationToken),
            eventType: "textDelta");
    }

    private static async IAsyncEnumerable<AgentFrameworkManager.AssistantResponseMessage> StreamAndPersist(
        AgentFrameworkManager agentManager,
        ISessionRepository sessionRepository,
        int conversationId,
        List<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var fullResponse = new System.Text.StringBuilder();
        await foreach (var update in agentManager.GetAssistantStreaming(messages, cancellationToken))
        {
            fullResponse.Append(update.DeltaText);
            yield return update;
        }

        // Add assistant response to messages for persistence
        messages.Add(new ChatMessage(ChatRole.Assistant, fullResponse.ToString()));

        var serialized = SerializeSession(messages);
        await sessionRepository.SaveSession(conversationId, serialized);
    }

    public static async Task<IResult> GetHistory(
        ISessionRepository sessionRepository,
        int conversationId)
    {
        string? serializedSession;
        try
        {
            serializedSession = await sessionRepository.GetSession(conversationId);
        }
        catch (ConversationNotFoundException)
        {
            return Results.NotFound();
        }

        if (serializedSession is null)
        {
            return Results.Ok(Array.Empty<object>());
        }

        var messages = DeserializeSession(serializedSession);

        var history = messages
            .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
            .Select(m => new
            {
                role = m.Role == ChatRole.User ? "user" : "assistant",
                content = m.Text
            })
            .Where(m => m.content is not null)
            .ToList();

        return Results.Ok(history);
    }

    private static List<ChatMessage> DeserializeSession(string? serializedSession)
    {
        if (serializedSession is null) { return []; }

        var entries = JsonSerializer.Deserialize<List<ChatMessageEntry>>(serializedSession) ?? [];
        return entries.Select(e => new ChatMessage(
            e.Role == "user" ? ChatRole.User : ChatRole.Assistant,
            e.Content)).ToList();
    }

    private static string SerializeSession(List<ChatMessage> messages)
    {
        var entries = messages
            .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
            .Where(m => m.Text is not null)
            .Select(m => new ChatMessageEntry(
                m.Role == ChatRole.User ? "user" : "assistant",
                m.Text!))
            .ToList();
        return JsonSerializer.Serialize(entries);
    }

    private record ChatMessageEntry(string Role, string Content);
    public record NewConversationResponse(int ConversationId);
    public record NewMessageRequest(string Message);
}
