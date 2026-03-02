# AI with C# - BASTA Spring 2026 Workshop

## Rebuilding the Solution

Commands to recreate this solution from scratch:

```bash
# Install Aspire workload (if not already installed)
dotnet workload install aspire

# Create solution
dotnet new sln -n DotNetChatbot
dotnet sln migrate
rm DotNetChatbot.sln

# Create projects
dotnet new aspire-apphost -n AppHost -o AppHost
dotnet new aspire-servicedefaults -n ServiceDefaults -o ServiceDefaults
dotnet new webapi -n ChatBot -o ChatBot

# Add projects to solution
dotnet sln add AppHost/AppHost.csproj
dotnet sln add ServiceDefaults/ServiceDefaults.csproj
dotnet sln add ChatBot/ChatBot.csproj

# Add project references
dotnet add AppHost/AppHost.csproj reference ChatBot/ChatBot.csproj
dotnet add ChatBot/ChatBot.csproj reference ServiceDefaults/ServiceDefaults.csproj

# Restore and build
dotnet restore
dotnet build

# Run via AppHost
dotnet run --project AppHost/AppHost.csproj
```
