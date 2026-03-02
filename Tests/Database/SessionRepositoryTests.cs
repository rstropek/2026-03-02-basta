using ChatBotDb;

namespace Tests.Database;

public class SessionRepositoryTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task GetSession_NonExistentConversation_ThrowsConversationNotFoundException()
    {
        using var context = new ApplicationDataContext(fixture.Options);
        var repo = new SessionRepository(context);

        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => repo.GetSession(999));
    }

    [Fact]
    public async Task GetSession_ExistingConversation_ReturnsNull_WhenNoSessionData()
    {
        using var context = new ApplicationDataContext(fixture.Options);
        var conversation = new Conversation();
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var repo = new SessionRepository(context);
        var result = await repo.GetSession(conversation.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveSession_ExistingConversation_PersistsSessionData()
    {
        using var context = new ApplicationDataContext(fixture.Options);
        var conversation = new Conversation();
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var repo = new SessionRepository(context);
        await repo.SaveSession(conversation.Id, "[{\"type\":\"message\"}]");

        var result = await repo.GetSession(conversation.Id);
        Assert.Equal("[{\"type\":\"message\"}]", result);
    }

    [Fact]
    public async Task SaveSession_NonExistentConversation_ThrowsConversationNotFoundException()
    {
        using var context = new ApplicationDataContext(fixture.Options);
        var repo = new SessionRepository(context);

        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => repo.SaveSession(999, "data"));
    }

    [Fact]
    public async Task SaveSession_OverwritesPreviousSessionData()
    {
        using var context = new ApplicationDataContext(fixture.Options);
        var conversation = new Conversation();
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var repo = new SessionRepository(context);
        await repo.SaveSession(conversation.Id, "first");
        await repo.SaveSession(conversation.Id, "second");

        var result = await repo.GetSession(conversation.Id);
        Assert.Equal("second", result);
    }
}
