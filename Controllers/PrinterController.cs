using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using sprint_final_salud_linux.Data;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace sprint_final_salud_linux.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrinterController : ControllerBase
    {
        private readonly MySqlContext _context;

        public PrinterController(MySqlContext context)
        {
            _context = context;
        }

        // =============================================
        // ============= CARNET ========================
        // =============================================

        [HttpGet("PrintPdff/{id}")]
        public async Task<IActionResult> PrintPdff(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            string nombre = user.Name;
            string documento = user.Identification;
            string rol = "Afiliado";
            string fecha = DateTime.Now.ToString("dd/MM/yyyy");
            string qrUrl = $"https://miapp.com/Admin/Infor/{user.Id}";

            bool enRender = !System.IO.File.Exists("/dev/usb/lp0");

            if (enRender)
            {
                // Render o entorno sin impresora
                var pdfBytes = GenerarCarnetPdf(nombre, documento, rol, fecha, qrUrl);
                return File(pdfBytes, "application/pdf", "carnet.pdf");
            }
            else
            {
                // Local con impresora térmica
                using var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write);

                byte[] center = { 0x1B, 0x61, 0x01 };
                fs.Write(center, 0, center.Length);

                fs.Write(new byte[] { 0x1D, 0x21, 0x11 }, 0, 3);
                fs.Write(Encoding.UTF8.GetBytes("---------------\nCARNET RIWI\n\n"), 0, Encoding.UTF8.GetBytes("---------------\nCARNET RIWI\n\n").Length);

                fs.Write(new byte[] { 0x1D, 0x21, 0x11 }, 0, 3);
                string datos = $"{nombre}\n\n{documento}\n---------------\n{rol}\n---------------\n";
                fs.Write(Encoding.UTF8.GetBytes(datos), 0, Encoding.UTF8.GetBytes(datos).Length);

                // QR ESC/POS
                byte[] sizeQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x08 };
                byte[] errorQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31 };
                byte[] storeQR = { 0x1D, 0x28, 0x6B, (byte)(qrUrl.Length + 3), 0x00, 0x31, 0x50, 0x30 };
                byte[] printQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 };

                fs.Write(sizeQR, 0, sizeQR.Length);
                fs.Write(errorQR, 0, errorQR.Length);
                fs.Write(storeQR, 0, storeQR.Length);
                fs.Write(Encoding.UTF8.GetBytes(qrUrl), 0, qrUrl.Length);
                fs.Write(printQR, 0, printQR.Length);

                fs.Write(Encoding.UTF8.GetBytes("\n\n\n"), 0, 3);
                fs.Flush();

                return Ok("Impreso correctamente.");
            }
        }

        // =============================================
        // ============== TURNO ========================
        // =============================================

        [HttpGet("PrintTurn")]
        public async Task<IActionResult> PrintTurn()
        {
            var turno = await _context.Turns.FindAsync(1); // ajusta si usas otro ID
            if (turno == null) return NotFound();

            string turnoTexto = turno.TurnRequest.ToString();
            string fecha = DateTime.Now.ToString("dd/MM/yyyy");

            bool enRender = !System.IO.File.Exists("/dev/usb/lp0");

            if (enRender)
            {
                var pdfBytes = GenerarTurnoPdf(turnoTexto, fecha);
                return File(pdfBytes, "application/pdf", "turno.pdf");
            }
            else
            {
                using var fs = new FileStream("/dev/usb/lp0", FileMode.Open, FileAccess.Write);

                fs.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);
                fs.Write(Encoding.UTF8.GetBytes("----------------\nTU TURNO\n\n"), 0, Encoding.UTF8.GetBytes("----------------\nTU TURNO\n\n").Length);

                fs.Write(Encoding.UTF8.GetBytes($" {turnoTexto}\n--------------- \n{fecha}\n----------------"), 0,
                    Encoding.UTF8.GetBytes($" {turnoTexto}\n--------------- \n{fecha}\n----------------").Length);

                byte[] feed = { 0x0A, 0x0A, 0x0A };
                fs.Write(feed, 0, feed.Length);
                fs.Write(Encoding.UTF8.GetBytes("\n\n\n"), 0, 3);
                fs.Flush();

                return Ok("Turno impreso correctamente.");
            }
        }

        // =============================================
        // ========== PDF GENERATORS ===================
        // =============================================

        private byte[] GenerarCarnetPdf(string nombre, string documento, string rol, string fecha, string qrData)
        {
            using var stream = new MemoryStream();

            float mmToPoints = 2.83465f;
            float width = 58 * mmToPoints;
            float height = 40 * mmToPoints;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(width, height));
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(column =>
                    {
                        column.Spacing(5);
                        column.Item().AlignCenter().Text("CARNET RIWI").Bold();
                        column.Item().Text($"Nombre: {nombre}");
                        column.Item().Text($"Documento: {documento}");
                        column.Item().Text($"Rol: {rol}");
                        column.Item().Text($"Fecha: {fecha}");
                        column.Item().AlignCenter().Image(GetQrImageBytes(qrData), ImageScaling.FitHeight);
                    });
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }

        private byte[] GenerarTurnoPdf(string turno, string fecha)
        {
            using var stream = new MemoryStream();

            float mmToPoints = 2.83465f;
            float width = 58 * mmToPoints;
            float height = 40 * mmToPoints;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(width, height));
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(column =>
                    {
                        column.Spacing(5);
                        column.Item().AlignCenter().Text("TU TURNO").Bold();
                        column.Item().AlignCenter().Text(turno);
                        column.Item().AlignCenter().Text($"Fecha: {fecha}");
                    });
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }

        // Cambiado para usar SkiaSharp y QRCoder PngByteQRCode (compatible Linux)
        private byte[] GetQrImageBytes(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}