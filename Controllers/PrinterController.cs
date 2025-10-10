using Microsoft.AspNetCore.Mvc;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Services;
using System.Text;
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
        // MÉTODO 1: Imprimir CARNET o generar PDF según entorno
        // =======================================================================
        [HttpGet("PrintPdff/{id}")]
        public async Task<IActionResult> PrintPdff(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound("Usuario no encontrado.");

            string nombre = user.Name;
            string documento = user.Identification;
            string rol = "Afiliado";
            string date = DateTime.Now.ToString("dd/MM/yyyy");

            // Detectar si existe la impresora física (solo en local)
            bool printerAvailable = System.IO.File.Exists("/dev/usb/lp0");

            if (printerAvailable)
            {
                // ==============================================================
                // 🔹 LOCAL MODE — Print directly to USB thermal printer
                // ==============================================================
                using (var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write))
                {
                    // Centrar texto
                    fs.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);

                    // Título
                    byte[] title = Encoding.UTF8.GetBytes("---------------\nCARNET RIWI\n\n");
                    fs.Write(title, 0, title.Length);

                    // Cuerpo
                    string bodyText = $"{nombre}\n{documento}\n---------------\n{rol}\n---------------\n{date}\n";
                    byte[] body = Encoding.UTF8.GetBytes(bodyText);
                    fs.Write(body, 0, body.Length);

                    // Alimentar papel
                    byte[] feed = { 0x0A, 0x0A, 0x0A };
                    fs.Write(feed, 0, feed.Length);
                    fs.Flush();
                }

                return Ok("🖨️ Impresión enviada correctamente.");
            }
            else
            {
                // ==============================================================
                // 🔹 RENDER MODE — Generate PDF Ticket (same look & size)
                // ==============================================================
                using (MemoryStream ms = new MemoryStream())
                {
                    // Tamaño 80 mm ancho x 100 mm alto aprox
                    var ticketSize = new Rectangle(226, 400);
                    var document = new Document(ticketSize, 10, 10, 10, 10);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    // Fuente monoespaciada para efecto ticket
                    var font = FontFactory.GetFont(FontFactory.COURIER_BOLD, 10, BaseColor.BLACK);

                    // Título centrado
                    var center = new Paragraph("CARNET RIWI\n---------------\n\n", font)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    document.Add(center);

                    // Datos usuario
                    document.Add(new Paragraph($"{nombre}\n{documento}\n---------------", font)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 5
                    });

                    // Rol y fecha
                    document.Add(new Paragraph($"Rol: {rol}\nFecha: {date}\n---------------\n", font)
                    {
                        Alignment = Element.ALIGN_CENTER
                    });

                    // QR simulado
                    document.Add(new Paragraph($"QR → /Admin/Infor/{user.Id}", font)
                    {
                        Alignment = Element.ALIGN_CENTER
                    });

                    document.Close();
                    return File(ms.ToArray(), "application/pdf", $"Carnet_{nombre}.pdf");
                }
            }
        }

        // =======================================================================
        // MÉTODO 2: Imprimir TURNO o generar PDF según entorno
        // =======================================================================
        [HttpGet("PrintTurn")]
        public async Task<IActionResult> PrintTurn()
        {
            var turno = _context.Turns.Find(1);
            if (turno == null) return NotFound("Turno no encontrado.");

            string turnoActual = turno.TurnRequest;
            string date = DateTime.Now.ToString("dd/MM/yyyy");

            bool printerAvailable = System.IO.File.Exists("/dev/usb/lp0");

            if (printerAvailable)
            {
                // ==============================================================
                // 🔹 LOCAL MODE — Print directly on physical printer
                // ==============================================================
                using (var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write))
                {
                    // Centrar texto
                    fs.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);

                    byte[] title = Encoding.UTF8.GetBytes("----------------\nTU TURNO\n----------------\n\n");
                    fs.Write(title, 0, title.Length);

                    string bodyText = $"{turnoActual}\n---------------\n{date}\n---------------\n";
                    byte[] body = Encoding.UTF8.GetBytes(bodyText);
                    fs.Write(body, 0, body.Length);

                    byte[] feed = { 0x0A, 0x0A, 0x0A };
                    fs.Write(feed, 0, feed.Length);
                    fs.Flush();
                }

                return Ok("🖨️ Turno impreso correctamente.");
            }
            else
            {
                // ==============================================================
                // 🔹 RENDER MODE — Generate Ticket as PDF
                // ==============================================================
                using (MemoryStream ms = new MemoryStream())
                {
                    var ticketSize = new Rectangle(226, 300);
                    var document = new Document(ticketSize, 10, 10, 10, 10);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    var font = FontFactory.GetFont(FontFactory.COURIER_BOLD, 12, BaseColor.BLACK);

                    var title = new Paragraph("----------------\nTU TURNO\n----------------\n\n", font)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    document.Add(title);

                    var body = new Paragraph($"{turnoActual}\n\n---------------\nFecha: {date}\n---------------", font)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    document.Add(body);

                    document.Close();
                    return File(ms.ToArray(), "application/pdf", $"Turno_{turnoActual}.pdf");
                }
            }
        }
    }
}
