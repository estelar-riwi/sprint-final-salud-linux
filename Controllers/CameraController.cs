using Microsoft.AspNetCore.Mvc;
using sprint_final_salud_linux.Services;
using sprint_final_salud_linux.Services;

namespace sprint_final_salud_linux.Controllers;

public class CameraController : Controller
{
    private readonly CloudinaryService _cloudinaryService;

    public CameraController(CloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(string photoBase64)
    {
        if (string.IsNullOrEmpty(photoBase64))
            return BadRequest("No se recibió la foto");

        // Quitar el encabezado "data:image/png;base64"
        var base64Data = photoBase64.Split(',')[1];
        var bytes = Convert.FromBase64String(base64Data);

        // Subir a Cloudinary
        var fileName = $"foto_{DateTime.Now.Ticks}.png";
        var url = await _cloudinaryService.UploadImageAsync(bytes, fileName);

        ViewBag.Message = "Foto subida con éxito a Cloudinary";
        ViewBag.ImageUrl = url;

        return View("Index");
    }
}