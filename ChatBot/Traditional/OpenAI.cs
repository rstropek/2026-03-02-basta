using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenAI.Responses;

namespace ChatBot.Traditional;

public class OpenAIManager(ResponsesClient client,
    IConfiguration config, DeveloperMessageProvider developerMessageProvider)
{
    private string Model => config["OPENAI_MODEL"] ?? throw new InvalidOperationException("OPENAI_MODEL not set");

    public async IAsyncEnumerable<AssistantResponseMessage> GetAssistantStreaming(
        IList<ResponseItem> conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // We loop until no more function calls are required
        bool requiresAction;
        do
        {
            requiresAction = false;

            var options = await GetResponseCreationOptions(conversation);

            var response = client.CreateResponseStreamingAsync(options, cancellationToken);
            await foreach (var chunk in response)
            {
                if (cancellationToken.IsCancellationRequested) { yield break; }

                if (chunk is StreamingResponseOutputTextDeltaUpdate textDelta)
                {
                    // We got a chunk of text from the LLM, let's send it to the client
                    yield return new AssistantResponseMessage(textDelta.Delta);
                }

                if (chunk is StreamingResponseOutputItemDoneUpdate doneUpdate
                    && doneUpdate.Item is not ReasoningResponseItem)
                {
                    // Accumulate the item in memory
                    conversation.Add(doneUpdate.Item);

                    // The response might be a function call, in which case we need to execute it
                    if (doneUpdate.Item is FunctionCallResponseItem functionCall)
                    {
                        requiresAction = true;

                        // For demo purposes, we notify the client of the function call
                        yield return new AssistantResponseMessage($"""

                            ```txt
                            {functionCall.FunctionName}({functionCall.FunctionArguments})
                            ```

                            """);
                    }
                }
            }
        }
        while (requiresAction);
    }

    private async Task<CreateResponseOptions> GetResponseCreationOptions(IList<ResponseItem> conversation)
    {
        var options = new CreateResponseOptions(conversation, Model)
        {
            Instructions = await developerMessageProvider.GetAsync(),
            ReasoningOptions = new()
            {
                ReasoningEffortLevel = ResponseReasoningEffortLevel.Low
            },
            MaxOutputTokenCount = 2500,
            StoredOutputEnabled = false,
            StreamingEnabled = true,
        };

        return options;
    }

    public record AssistantResponseMessage(string DeltaText);
}
