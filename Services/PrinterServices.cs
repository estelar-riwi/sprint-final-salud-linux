namespace sprint_final_salud_linux.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


public class PrinterService
{
    public async Task PrintTextAsync(string text, string printerName = "PrintXD")
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "lp",
            Arguments = $"-d {printerName}",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        // 🔹 Mandamos el texto directo al proceso lp
        await process.StandardInput.WriteLineAsync(text + "\n\n"); 
        process.StandardInput.Close();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        Console.WriteLine($"Salida: {output}");
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Error: {error}");
        }
    }
}