using ComplexTestingApi.CSharp;
using FluentAssertions;
using NUnit.Framework;

namespace TestingClient.ComplexApi.ACGT;

[TestFixture]
internal class MinimalApiTestingHandler
{
    private MinimalApiClient client;
    private Todo todo = new Todo()
    {
        Id = 1,
        Name = "todo",
        IsComplete = true
    };

    [SetUp]
    public void Setup()
    {
        var httpClient = HttpHelper.CreateHttpClient();
        var client = new MinimalApiClient(httpClient);
        client.JsonSerializerOptions.WithAllPossiblyNecessarySettings();
        this.client = client;

    }

    [Test]
    public async Task Test_Get()
    {
        var result = await this.client.GET_todoitemsAsync<Todo>(CancellationToken.None);
        result.SuccessResponse.Should().BeEquivalentTo(this.todo);
    }

    [Test]
    public async Task Test_Post()
    {
        var result = await this.client.POST_todoitemsAsync<Todo>(this.todo, CancellationToken.None);
        result.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task Test_Put()
    {
        string id = "2";
        var result = await this.client.PUT_todoitemsAsync<Todo, Todo>(id, this.todo, CancellationToken.None);
        result.SuccessResponse.Id.Should().Be(2);
        result.SuccessResponse.Name.Should().Be(todo.Name);
        result.SuccessResponse.IsComplete.Should().Be(todo.IsComplete);
    }

    [Test]
    public async Task Test_Delete()
    {
        string id = "2";
        var result = await this.client.PUT_todoitemsAsync<Todo, Todo>(id, this.todo, CancellationToken.None);
        result.SuccessResponse.Id.Should().Be(2);
        result.SuccessResponse.Name.Should().Be(todo.Name);
        result.SuccessResponse.IsComplete.Should().Be(todo.IsComplete);
    }

    public class Todo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
