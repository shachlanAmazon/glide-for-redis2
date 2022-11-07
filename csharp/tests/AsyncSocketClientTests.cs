namespace tests;

using babushka;

// TODO - need to start a new redis server for each test?
public class AsyncSocketClientTests
{
    static Random randomizer = new();

    private async Task GetAndSetRandomValues(AsyncSocketClient client)
    {
        var key = (randomizer.Next(3750000) + 1).ToString();
        var value = new string('0', 4500);
        await client.SetAsync(key, value);
        var result = await client.GetAsync(key);
        Assert.That(result, Is.EqualTo(value));
    }

    [Test, Timeout(200)]
    public async Task GetReturnsLastSet()
    {
        var client = await AsyncSocketClient.CreateSocketClient("redis://localhost:6379");
        Console.WriteLine("got client");
        await GetAndSetRandomValues(client);
    }

    [Test, Timeout(200)]
    public async Task GetAndSetCanHandleNonASCIIUnicode()
    {
        var client = await AsyncSocketClient.CreateSocketClient("redis://localhost:6379");
        var key = Guid.NewGuid().ToString();
        var value = "שלום hello 汉字";
        await client.SetAsync(key, value);
        var result = await client.GetAsync(key);
        Assert.That(result, Is.EqualTo(value));
    }

    [Test, Timeout(200)]
    public async Task GetReturnsNull()
    {
        var client = await AsyncSocketClient.CreateSocketClient("redis://localhost:6379");
        var result = await client.GetAsync(Guid.NewGuid().ToString());
        Assert.That(result, Is.EqualTo(null));
    }

    [Test, Timeout(200)]
    public async Task GetReturnsEmptyString()
    {
        var client = await AsyncSocketClient.CreateSocketClient("redis://localhost:6379");
        var key = Guid.NewGuid().ToString();
        var value = "";
        await client.SetAsync(key, value);
        var result = await client.GetAsync(key);
        Assert.That(result, Is.EqualTo(value));
    }

    [Test, Timeout(2000)]
    public async Task HandleVeryLargeInput()
    {
        var client = await AsyncSocketClient.CreateSocketClient("redis://localhost:6379");
        Console.WriteLine("done creating client");
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();
        while (value.Length < Math.Pow(2, 23))
        {
            value += value;
        }
        Console.WriteLine("start set with length " + value.Length);
        await client.SetAsync(key, value);
        Console.WriteLine("start get");
        var result = await client.GetAsync(key);
        Console.WriteLine("done get");
        Assert.That(result, Is.EqualTo(value));
    }

    // This test is slow and hardly a unit test, but it caught timing and releasing issues in the past,
    // so it's being kept.
    // [Test, Timeout(10000)]
    // public async Task ConcurrentOperationsWork()
    // {
    //     var client = await AsyncSocketClient.CreateSocketClient("redis://localhost:6379");
    //     var operations = new List<Task>();

    //     for (int i = 0; i < 2; ++i)
    //     {
    //         var index = i;
    //         operations.Add(Task.Run(async () =>
    //         {
    //             for (int i = 0; i < 10; ++i)
    //             {
    //                 Console.WriteLine($"Test send for {i} {index}");
    //                 if ((i + index) % 5 == 0)
    //                 {
    //                     var result = await client.GetAsync(Guid.NewGuid().ToString());
    //                     Assert.That(result, Is.EqualTo(null));
    //                 }
    //                 else
    //                 {
    //                     await GetAndSetRandomValues(client);
    //                 }
    //                 Console.WriteLine($"Test received for {i} {index}");
    //             }
    //         }));
    //     }

    //     Task.WaitAll(operations.ToArray());
    // }
}
