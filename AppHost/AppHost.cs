var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite(
    "chatbot-db",
    builder.Configuration["Database:path"],
    builder.Configuration["Database:fileName"]);
var model = builder.AddParameter(
    "openai-model", 
    builder.Configuration["Parameters:openai-model"]!);
var apiKey = builder.AddParameter("openai-api-key", secret: true);

var cartMcp = builder.AddProject<Projects.CartMcp>("cart-mcp")
    .WithReference(sqlite);

var chatbot = builder.AddProject<Projects.ChatBot>("chatbot")
    .WithReference(sqlite)
    .WithReference(cartMcp)
    .WithEnvironment("OPENAI_MODEL", model)
    .WithEnvironment("OPENAI_API_KEY", apiKey);

builder.AddNpmApp("chat-ui", "../ChatUI")
    .WithReference(chatbot)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();
    
builder.Build().Run();
