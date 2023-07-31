// See https://aka.ms/new-console-template for more information
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServicePiter;

Console.WriteLine("Случайные запросы к серверу");

// подключение
using var channel = GrpcChannel.ForAddress("https://localhost:7254");
var client = new Accounter.AccounterClient(channel);

// Ответ сервера на команду SayHello
using var call = client.SayHelloStream();
var readTask = Task.Run(async () =>
{
    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response.Message);
    }
});

// Случайные запросы
while (true)
{
    Console.WriteLine(">>>>>>>> e - выход / количество команд?");
    var result = Console.ReadLine();
    if (String.IsNullOrWhiteSpace(result) || result == "e")
        break;

    Random rnd = new Random();

    for (var iter=1; iter<=int.Parse(result); iter++)
    {
        int selector = rnd.Next(rnd.Next(0, 6));
        Console.WriteLine("");
        Console.WriteLine($"---< {iter} > CMD = {selector} --------------------------");
        switch (selector)
        {
            case 0:
                await call.RequestStream.WriteAsync(new HelloRequest() { Name = result });
                break;
            case 1:
                Command1();
                break;
            case 2:
                Command2();
                break;
            case 3:
                Command3();
                break;
            case 4:
                Command4();
                break;
            case 5:
                Command5();
                break;
        }
        await Task.Delay(100 * selector);
    }

}

await call.RequestStream.CompleteAsync();
await readTask;


// 1) привет серверу
async void Command1()
{
    var reply = await client.SayHelloAsync(new HelloRequest() { Name = "Client" });
    Console.WriteLine(reply.Message);
}

// 2.1) получение списка объектов
async void Command2()
{
    ListReply workers = await client.ListWorkersAsync(new Google.Protobuf.WellKnownTypes.Empty());
    foreach (var Worker in workers.Workers)
    {
        Console.WriteLine($"{Worker.Id}. {Worker.FirstName} - {Worker.LastName} - {Worker.BirthDay}");
    }
}

// 2.2) получение одного объекта по id = 1
async void Command3()
{
    WorkerReply worker2 = await client.GetWorkerAsync(new GetWorkerRequest { Id = 1 });
    Console.WriteLine($"ID=1 -> {worker2.Id}. {worker2.FirstName} - {worker2.LastName} - {worker2.BirthDay}");
}

// 2.3) добавление одного объекта
async void Command4()
{
    var answer = await client.CreateWorkerAsync(new CreateWorkerRequest
    {
        Worker = new WorkerReply() { FirstName = $"D.A.R.Y.L {DateTime.Now}", BirthDay = DateTime.Now.ToLongDateString() }
    });
    Console.WriteLine($"ADD: ID = {answer.Id} result = {answer.Code}");
}

// 2.4) обновление одного объекта - изменим имя у объекта с id = 1
async void Command5()
{
    var answer2 = await client.UpdateWorkerAsync(new UpdateWorkerRequest
    {
        Worker = new WorkerReply() { Id = 1, FirstName = $"D.A.R.Y.L {DateTime.Now}", BirthDay = DateTime.Now.ToLongDateString() }
    });
    Console.WriteLine($"UPDATE: ID = {answer2.Id} result = {answer2.Code}");
}