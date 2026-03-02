using ChatBot;
using ChatBot.Traditional;
using ChatBotDb;
using OpenAI.Responses;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddSqliteDbContext<ApplicationDataContext>("chatbot-db");
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddSingleton<DeveloperMessageProvider>();
builder.Services.AddScoped<OpenAIManager>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton(_ => new ResponsesClient(
    builder.Configuration["OPENAI_MODEL"] ?? throw new InvalidOperationException("OPENAI_MODEL not set"),
    new System.ClientModel.ApiKeyCredential(builder.Configuration["OPENAI_API_KEY"]!)));

builder.Services.AddCors();

var app = builder.Build();

await app.Services.ApplyMigrations();

app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapTraditionalConversationsEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
