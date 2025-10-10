using Microsoft.AspNetCore.Mvc;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Services;
using System.Text;
using System.Drawing.Printing;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace sprint_final_salud_linux.Controllers
{
    public class PrinterController : Controller
    {
        private readonly PrinterService _printerService;
        private readonly MySqlContext _context;

        public PrinterController(MySqlContext context)
        {
            _printerService = new PrinterService();
            _context = context;
        }

        // =======================================================================
        // MÉTODO: Imprimir carnet o generar PDF dependiendo del entorno
        // =======================================================================
        [HttpGet("PrintPdff/{id}")]
        public async Task<IActionResult> PrintPdff(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound("Usuario no encontrado");

            string nombre = user.Name;
            string documento = user.Identification;
            string rol = "Afiliado";
            string date = DateTime.Now.ToString("dd/MM/yyyy");

            // Detectar si existe la impresora física
            bool printerAvailable = System.IO.File.Exists("/dev/usb/lp0");

            if (printerAvailable)
            {
                // ==============================================================
                // 🔹 Modo local — Imprimir directamente en la impresora térmica
                // ==============================================================
                using (var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write))
                {
                    // Centrar texto
                    fs.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);

                    byte[] title = Encoding.UTF8.GetBytes("---------------\nCARNET RIWI\n\n");
                    fs.Write(title, 0, title.Length);

                    string bodyText = $"{nombre}\n{documento}\n---------------\n{rol}\n---------------\n";
                    byte[] body = Encoding.UTF8.GetBytes(bodyText);
                    fs.Write(body, 0, body.Length);

                    byte[] feed = { 0x0A, 0x0A, 0x0A };
                    fs.Write(feed, 0, feed.Length);
                    fs.Flush();
                }

                return Ok("Impresión enviada correctamente.");
            }
            else
            {
                // ==============================================================
                // 🔹 Modo Render — Generar PDF simulado del ticket
                // ==============================================================
                using (MemoryStream ms = new MemoryStream())
                {
                    // Configurar tamaño similar a una impresora térmica (80mm)
                    var ticketSize = new Rectangle(226, 400); // 80mm ≈ 226 puntos
                    var document = new Document(ticketSize, 10, 10, 10, 10);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    // Fuente base
                    var font = FontFactory.GetFont(FontFactory.COURIER_BOLD, 10, BaseColor.BLACK);
                    var center = new Paragraph("CARNET RIWI\n", font)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 8
                    };
                    document.Add(center);

                    // Datos del usuario
                    document.Add(new Paragraph($"{nombre}\n{documento}\n---------------", font)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 5
                    });

                    // Rol
                    document.Add(new Paragraph($"Rol: {rol}", font)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 5
                    });

                    // Fecha
                    document.Add(new Paragraph($"{date}", font)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10
                    });

                    // Simular QR con texto (Render no puede generar ESC/POS QR)
                    document.Add(new Paragraph($"QR → /Admin/Infor/{user.Id}", font)
                    {
                        Alignment = Element.ALIGN_CENTER
                    });

                    document.Close();

                    // Descargar PDF
                    return File(ms.ToArray(), "application/pdf", $"Carnet_{nombre}.pdf");
                }
            }
        }
    }
}
