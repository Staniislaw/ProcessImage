using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using Color = SixLabors.ImageSharp.Color;
using SixLabors.ImageSharp.PixelFormats;

namespace ProcessImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageProcessingController : ControllerBase
    {
        [HttpPost("resize")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Resize([FromForm] IFormFile image, [FromForm] int scale)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectată nicio imagine.");

            using var img = await Image.LoadAsync(image.OpenReadStream());
            int width = (int)(img.Width * (scale / 100.0));
            int height = (int)(img.Height * (scale / 100.0));

            img.Mutate(x => x.Resize(width, height));

            using var ms = new MemoryStream();
            await img.SaveAsync(ms, new PngEncoder());
            ms.Position = 0;

            return File(ms.ToArray(), "image/png", $"resized-{DateTime.Now.Ticks}.png");
        }

        [HttpPost("crop")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Crop([FromForm] IFormFile image, [FromForm] int x, [FromForm] int y, [FromForm] int width, [FromForm] int height)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectată nicio imagine.");
            if (width <= 0 || height <= 0)
                return BadRequest("Lățimea și înălțimea trebuie să fie pozitive.");
            using var img = await Image.LoadAsync(image.OpenReadStream());
            if (x < 0 || y < 0 || x + width > img.Width || y + height > img.Height)
                return BadRequest("Coordonatele de decupare sunt în afara imaginii.");
            img.Mutate(i => i.Crop(new Rectangle(x, y, width, height)));

            using var ms = new MemoryStream();
            await img.SaveAsync(ms, new PngEncoder());
            ms.Position = 0;

            return File(ms.ToArray(), "image/png", $"cropped-{DateTime.Now.Ticks}.png");
        }
        [HttpPost("rotate")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Rotate([FromForm] IFormFile image, [FromForm] float angle)
        {
            using var img = await Image.LoadAsync(image.OpenReadStream());
            img.Mutate(i => i.Rotate(angle));

            using var ms = new MemoryStream();
            await img.SaveAsync(ms, new PngEncoder());
            ms.Position = 0;

            return File(ms.ToArray(), "image/png", $"rotated-{DateTime.Now.Ticks}.png");
        }

        [HttpPost("filter")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Filter([FromForm] IFormFile image, [FromForm] string filterType, [FromForm] int intensity)
        {
            using var img = await Image.LoadAsync(image.OpenReadStream());

            switch (filterType.ToLower())
            {
                case "grayscale":
                    img.Mutate(x => x.Grayscale());
                    break;
                case "invert":
                    img.Mutate(x => x.Invert());
                    break;
                case "blur":
                    img.Mutate(x => x.GaussianBlur(intensity / 10f));
                    break;
                case "brightness":
                    img.Mutate(x => x.Brightness(intensity / 100f));
                    break;
                case "contrast":
                    img.Mutate(x => x.Contrast(intensity / 100f));
                    break;
                case "sepia":
                    img.Mutate(x => x.Sepia());
                    break;
            }

            using var ms = new MemoryStream();
            await img.SaveAsync(ms, new PngEncoder());
            ms.Position = 0;

            return File(ms.ToArray(), "image/png", $"filtered-{DateTime.Now.Ticks}.png");
        }

        [HttpPost("compress")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Compress([FromForm] IFormFile image, [FromForm] int quality)
        {
            using var img = await Image.LoadAsync(image.OpenReadStream());

            using var ms = new MemoryStream();
            await img.SaveAsync(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
            {
                Quality = quality
            });
            ms.Position = 0;

            return File(ms.ToArray(), "image/jpeg", $"compressed-{DateTime.Now.Ticks}.jpg");
        }

        [HttpPost("watermark")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Watermark(
     [FromForm] IFormFile image,
     [FromForm] string text,
     [FromForm] int fontSize = 48,
     [FromForm] string color = "#FFFFFF",
     [FromForm] string position = "BottomRight",
     [FromForm] int opacity = 50)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectată nicio imagine.");

            if (string.IsNullOrEmpty(text))
                return BadRequest("Textul nu poate fi gol.");

            using var img = await Image.LoadAsync(image.OpenReadStream());
            var font = SystemFonts.CreateFont("Arial", fontSize);

            var padding = 20;

            // Estimăm dimensiunea textului (aproximativ)
            float estimatedWidth = text.Length * fontSize * 0.6f;
            float estimatedHeight = fontSize * 1.2f;

            float x = 0, y = 0;

            switch (position.ToLower())
            {
                case "topleft":
                    x = padding;
                    y = padding;
                    break;
                case "topcenter":
                    x = (img.Width - estimatedWidth) / 2;
                    y = padding;
                    break;
                case "topright":
                    x = img.Width - estimatedWidth - padding;
                    y = padding;
                    break;
                case "center":
                    x = (img.Width - estimatedWidth) / 2;
                    y = (img.Height - estimatedHeight) / 2;
                    break;
                case "bottomleft":
                    x = padding;
                    y = img.Height - estimatedHeight - padding;
                    break;
                case "bottomcenter":
                    x = (img.Width - estimatedWidth) / 2;
                    y = img.Height - estimatedHeight - padding;
                    break;
                case "bottomright":
                default:
                    x = img.Width - estimatedWidth - padding;
                    y = img.Height - estimatedHeight - padding;
                    break;
            }

            // Parsăm culoarea și aplicăm opacitatea
            var baseColor = Color.Parse(color);
            var colorWithOpacity = baseColor.WithAlpha(opacity / 100f);

            img.Mutate(ctx =>
            {
                ctx.DrawText(text, font, colorWithOpacity, new PointF(x, y));
            });

            using var ms = new MemoryStream();
            await img.SaveAsync(ms, new PngEncoder());
            ms.Position = 0;

            return File(ms.ToArray(), "image/png", $"watermarked-{DateTime.Now.Ticks}.png");
        }
    }
}
