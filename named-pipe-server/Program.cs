using MessagePack;
using System.IO.Pipes;
using System.Runtime.InteropServices;

[Union(0, typeof(Message1))]
[Union(1, typeof(Message2))]
[MessagePackObject(keyAsPropertyName: true)]
public abstract class Message
{
    private const string CurrentVersion = "2.0";

    public string Version { get; set; } = CurrentVersion;

    public Guid Id { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class Message1 : Message
{
    public string MetaData1 { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class Message2 : Message
{
    public string MetaData2 { get; set; }
}


class Program
{
    private static int ReadInt32(Stream stream)
    {
        byte[] bytes = new byte[4];
        _ = stream.Read(bytes, 0, 4);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt32(bytes, 0);
    }

    private static Message ReadMessage(Stream stream)
    {
        int len = ReadInt32(stream);
        Console.WriteLine("Length - " + len);
        byte[] inBuffer = new byte[len];
        _ = stream.Read(inBuffer, 0, len);

        return MessagePack.MessagePackSerializer.Deserialize<Message>(inBuffer);
    }

    static void Main(string[] args)
    {
        var localPipeName = "node_named_pipe";
        var pipeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"\\.\pipe\"
            : "/tmp/";
        pipeName += localPipeName;

        /*
        if (!File.Exists(pipeName))
        {
            Console.WriteLine($"The named pipe {pipeName} does not exist.");
            return;
        }*/

        var client = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            new NamedPipeClientStream(".", localPipeName, PipeDirection.In) :
            new NamedPipeClientStream(".", pipeName, PipeDirection.In);
        try
        {
            client.Connect();
            Console.WriteLine("Connected to the named pipe.");

            var deserializedMessage = ReadMessage(client);
            Console.WriteLine($"Received: {deserializedMessage.Version} {deserializedMessage.Id}");
            if (deserializedMessage is Message1 msg1)
            {
                Console.WriteLine($"MetaData1: {msg1.MetaData1}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
