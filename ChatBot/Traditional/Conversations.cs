namespace ChatBot.Traditional;

public static class ConversationEndpoints
{
    private const string GetConversationHistoryRouteName = "GetConversationHistory";

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTraditionalConversationsEndpoints()
        {
            var api = app.MapGroup("/conversations");
            api.MapPost("/", () => { });
            api.MapPost("/{conversationId}/chat", () => { });
            api.MapGet("/{conversationId}", () => { }).WithName(GetConversationHistoryRouteName);

            return app;
        }
    }
}