using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var localPipeName = "node_named_pipe";
        var pipeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"\\.\pipe\"
            : "/tmp/";
        pipeName += localPipeName;

        if (!File.Exists(pipeName))
        {
            Console.WriteLine($"The named pipe {pipeName} does not exist.");
            return;
        }

        await using var client = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            new NamedPipeClientStream(".", localPipeName, PipeDirection.InOut, PipeOptions.Asynchronous) :
            new NamedPipeClientStream(pipeName);
        try
        {
            //Hangs here
            await client.ConnectAsync();
            Console.WriteLine("Connected to the named pipe.");

            await using var writer = new StreamWriter(client);
            var reader = new StreamReader(client);
            writer.AutoFlush = true;
            string message = "Hello from .NET 8!";
            await writer.WriteLineAsync(message);
            Console.WriteLine($"Sent: {message}");

            string response = await reader.ReadLineAsync();
            Console.WriteLine($"Received: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
