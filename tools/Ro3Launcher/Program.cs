using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace Ro3Launcher;

internal static class Program
{
    private static int Main(string[] args)
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
        string exe = Path.Combine(dir, "ro3.exe");
        if (!File.Exists(exe))
        {
            Console.WriteLine("ro3.exe not found in " + dir);
            return 1;
        }

        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        var start = new ProcessStartInfo(exe)
        {
            UseShellExecute = false,
            WorkingDirectory = dir,
        };
        foreach (var a in args) start.ArgumentList.Add(a);
        start.Environment["RO3_LAUNCH_PIPE_HANDLE"] = pipe.GetClientHandleAsString();

        var game = Process.Start(start);
        if (game == null)
        {
            Console.WriteLine("Failed to start ro3.exe");
            return 1;
        }
        pipe.DisposeLocalCopyOfClientHandle();

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string proof = string.Join("\r\n",
            "RO3_LAUNCH_PROOF_V1",
            game.Id.ToString(CultureInfo.InvariantCulture),
            now.ToString(CultureInfo.InvariantCulture),
            (now + 30).ToString(CultureInfo.InvariantCulture),
            Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32))) + "\r\n";
        byte[] bytes = Encoding.ASCII.GetBytes(proof);
        pipe.Write(bytes, 0, bytes.Length);
        pipe.Flush();
        Console.WriteLine($"[Ro3Launcher] Handshake sent to ro3.exe (PID {game.Id.ToString(CultureInfo.InvariantCulture)}). Game running!");
        game.WaitForExit();
        return game.ExitCode;
    }
}
