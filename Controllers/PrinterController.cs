namespace sprint_final_salud_linux.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using sprint_final_salud_linux.Services;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using sprint_final_salud_linux.Data;




public class PrinterController : Controller
{
    private readonly PrinterService _printerService;
    private readonly MySqlContext _context;
    
   
        
    
    
    public PrinterController(MySqlContext context)
    {
        _printerService = new PrinterService();
        _context = context;
    }
   

    public async Task<IActionResult> Print()
    {
        
        using (var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write))
        using (var writer = new StreamWriter(fs))
        {
            writer.WriteLine("+--------------------------------+");
            writer.WriteLine("|           CARNET RIWI           |");
            writer.WriteLine("+--------------------------------+");
            writer.WriteLine("|          Afiliado               |");
            writer.WriteLine("+--------------------------------+");
            writer.WriteLine("\n\n\n"); // espacio para cortar
            writer.Flush();
        }

        return Ok();
    }

    public async Task<IActionResult> PrintPdff(int id)
    {
        var UserPrint = _context.Users.Find(id);
        string nombre = UserPrint.Name;
        string documento = UserPrint.Identification;
        string rol = "Afiliado";
        string date = DateTime.Now.ToString("dd/MM/yyyy");

        using (var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write))
        {
            // Centrar título
            fs.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);
            byte[] title = Encoding.UTF8.GetBytes("CARNET RIWI\n\n");
            fs.Write(title, 0, title.Length);

            // Alinear izquierda
            fs.Write(new byte[] { 0x1B, 0x61, 0x01}, 0, 3);
            string datos = $"{nombre}\n{documento}\n{rol}\n";
            byte[] body = Encoding.UTF8.GetBytes(datos);
            fs.Write(body, 0, body.Length);

            // --- QR con el documento ---
            // --- Alinear al centro ---
            byte[] center = { 0x1B, 0x61, 0x01 };
            fs.Write(center, 0, center.Length);

// --- Tamaño del QR (8 = grande, puedes probar entre 1 y 16) ---
            byte[] sizeQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x08 };
            fs.Write(sizeQR, 0, sizeQR.Length);

// --- Nivel de corrección de errores (M) ---
            byte[] errorQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31 };
            fs.Write(errorQR, 0, errorQR.Length);

// --- QR con el documento ---
            string qrData = $"http://localhost:5026/Admin/Infor/{UserPrint.Id}";
            byte[] storeQR = { 0x1D, 0x28, 0x6B, (byte)(qrData.Length + 3), 0x00, 0x31, 0x50, 0x30 };
            fs.Write(storeQR, 0, storeQR.Length);
            fs.Write(Encoding.UTF8.GetBytes(qrData), 0, qrData.Length);

// --- Imprimir QR ---
            byte[] printQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 };
            fs.Write(printQR, 0, printQR.Length);

// --- Resetear alineación a la izquierda ---
            byte[] left = { 0x1B, 0x61, 0x00 };
            fs.Write(left, 0, left.Length);

// --- Alimentar papel ---
            byte[] feed = { 0x0A, 0x0A, 0x0A };
            fs.Write(feed, 0, feed.Length);
            // Espacios finales (para cortar o separar)
            fs.Write(Encoding.UTF8.GetBytes("\n\n\n"), 0, 3);

            fs.Flush();
        }

        return Ok();
    }
    
    //-----------------------------------------------------------------------------
    public async Task<IActionResult> PrintTurn(int prnTurn)
    {
        /*var TurnPrint = _context.TurnRequests.Find();*/
        /*int turn = TurnPrint.;*/
        string date = DateTime.Now.ToString("dd/MM/yyyy");
        int printTurn = prnTurn;

        using (var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write))
        {
            // Centrar título
            fs.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);
            byte[] title = Encoding.UTF8.GetBytes("TU TURNO\n\n");
            fs.Write(title, 0, title.Length);

            // Alinear izquierda
            fs.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);
            string data = $"{printTurn} \n {date}";
            byte[] body = Encoding.UTF8.GetBytes(data);
            fs.Write(body, 0, body.Length);

           



// --- Nivel de corrección de errores (M) ---
            byte[] errorQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31 };
            fs.Write(errorQR, 0, errorQR.Length);
            
// --- Alimentar papel ---
            byte[] feed = { 0x0A, 0x0A, 0x0A };
            fs.Write(feed, 0, feed.Length);
            // Espacios finales (para cortar o separar)
            fs.Write(Encoding.UTF8.GetBytes("\n\n\n"), 0, 3);

            fs.Flush();
        }

        return Ok($"Turno {prnTurn} impreso correctamente.");
    }
    
    
}

