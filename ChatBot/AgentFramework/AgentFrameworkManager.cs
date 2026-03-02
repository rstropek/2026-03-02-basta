using System.ComponentModel;
using System.Runtime.CompilerServices;
using ChatBot.Traditional;
using ChatBotDb;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace ChatBot.AgentFramework;

public class AgentFrameworkManager(
    ResponsesClient responsesClient,
    McpToolsProvider mcpToolsProvider,
    DeveloperMessageProvider developerMessageProvider,
    ApplicationDataContext db,
    ILoggerFactory loggerFactory)
{

    public async IAsyncEnumerable<AssistantResponseMessage> GetAssistantStreaming(
        List<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var instructions = await developerMessageProvider.GetAsync();

        // Build function tools wrapping the existing ProductsTools methods
        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(
                (string flowerName) => ProductsTools.GetAvailableColorsForFlower(new(flowerName)),
                nameof(ProductsTools.GetAvailableColorsForFlower),
                "Gets a list of available colors for a specific flower"),
            AIFunctionFactory.Create(
                async () => await ProductsTools.GetBouquetSizes(db),
                nameof(ProductsTools.GetBouquetSizes),
                "Gets the list of available bouquet sizes (e.g. Small, Medium, Large)"),
            AIFunctionFactory.Create(
                async ([Description("The bouquet size (e.g. Small, Medium, Large)")] string size) =>
                    await ProductsTools.GetBouquetPrice(db, new(size)),
                nameof(ProductsTools.GetBouquetPrice),
                "Gets the price, number of flowers, and description for a specific bouquet size"),
        };

        // Add MCP tools (McpClientTool implements AITool from M.E.AI)
        var mcpClient = await mcpToolsProvider.GetClientAsync();
        var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
        tools.AddRange(mcpTools);

        var agent = responsesClient.AsAIAgent(
            instructions: instructions,
            tools: [.. tools],
            loggerFactory: loggerFactory,
            clientFactory: c => c.AsBuilder().UseOpenTelemetry(loggerFactory: loggerFactory).Build());

        await foreach (var update in agent.RunStreamingAsync(messages, cancellationToken: cancellationToken))
        {
            foreach (var content in update.Contents)
            {
                if (content is TextContent textContent && !string.IsNullOrEmpty(textContent.Text))
                {
                    yield return new AssistantResponseMessage(textContent.Text);
                }
            }
        }
    }

    public record AssistantResponseMessage(string DeltaText);
}
