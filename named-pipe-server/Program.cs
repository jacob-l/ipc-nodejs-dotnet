using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string pipeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"\\.\pipe\node_named_pipe"
            : "/tmp/node_named_pipe";

        if (!File.Exists(pipeName))
        {
            Console.WriteLine($"The named pipe {pipeName} does not exist.");
            return;
        }

        using (var client = new NamedPipeClientStream(pipeName))
        {
            try
            {
                await client.ConnectAsync();
                Console.WriteLine("Connected to the named pipe.");

                using var writer = new StreamWriter(client);
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
}
