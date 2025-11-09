using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using Color = SixLabors.ImageSharp.Color;
using SixLabors.ImageSharp.PixelFormats;
using ProcessImage.Helpers.ProcessImage.Helpers;
using System.Numerics;
using Data.SDK.Repository;
using ProcessImage.Entities;
using ProcessImage.Services;
using ProcessImage.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ProcessImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ImageProcessingController : ControllerBase
    {
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IBaseService _baseService;

        public ImageProcessingController(
            IImageProcessingService imageProcessingService,
            IBaseService baseService)
        {
            _imageProcessingService = imageProcessingService;
            _baseService = baseService;
        }

        [HttpPost("resize")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Resize([FromForm] IFormFile image, [FromForm] int scale)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectată nicio imagine.");

            try
            {
                var userId = _baseService.GetUserId();
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    1, // tipProcesareId pentru resize
                    async img =>
                    {
                        int width = (int)(img.Width * (scale / 100.0));
                        int height = (int)(img.Height * (scale / 100.0));
                        img.Mutate(x => x.Resize(width, height));
                        await Task.CompletedTask;
                    }
                );

                return File(imageBytes, "image/png", $"resized-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la resize: {ex.Message}");
            }
        }

        [HttpPost("crop")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Crop([FromForm] IFormFile image, [FromForm] int x, [FromForm] int y, [FromForm] int width, [FromForm] int height)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectata nicio imagine.");
            if (width <= 0 || height <= 0)
                return BadRequest("Latimea si inaltimea trebuie să fie pozitive.");

            try
            {
                var userId = _baseService.GetUserId();
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    2, // tipProcesareId pentru crop
                    async img =>
                    {
                        if (x < 0 || y < 0 || x + width > img.Width || y + height > img.Height)
                            throw new ArgumentException("Coordonatele de decupare sunt în afara imaginii.");

                        img.Mutate(i => i.Crop(new Rectangle(x, y, width, height)));
                        await Task.CompletedTask;
                    }
                );

                return File(imageBytes, "image/png", $"cropped-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la crop: {ex.Message}");
            }
        }

        [HttpPost("rotate")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Rotate([FromForm] IFormFile image, [FromForm] float angle)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectata nicio imagine.");

            try
            {
                var userId = _baseService.GetUserId();
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    1, // tipProcesareId pentru rotate
                    async img =>
                    {
                        img.Mutate(i => i.Rotate(angle));
                        await Task.CompletedTask;
                    }
                );

                return File(imageBytes, "image/png", $"rotated-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la rotate: {ex.Message}");
            }
        }

        [HttpPost("filter")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Filter([FromForm] IFormFile image, [FromForm] string filterType, [FromForm] int intensity)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectata nicio imagine.");

            try
            {
                var userId = _baseService.GetUserId();
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    3, // tipProcesareId pentru filter
                    async img =>
                    {
                        switch (filterType?.ToLower())
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
                            default:
                                throw new ArgumentException($"Filtrul '{filterType}' nu este suportat.");
                        }
                        await Task.CompletedTask;
                    }
                );

                return File(imageBytes, "image/png", $"filtered-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la filter: {ex.Message}");
            }
        }

        [HttpPost("compress")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Compress([FromForm] IFormFile image, [FromForm] int quality)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Nu a fost selectata nicio imagine.");

            try
            {
                var userId = _baseService.GetUserId();
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    4, // tipProcesareId pentru compress
                    async img =>
                    {
                        using var ms = new MemoryStream();
                        await img.SaveAsync(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                        {
                            Quality = quality
                        });
                        ms.Position = 0;
                        await Task.CompletedTask;
                    }
                );

                return File(imageBytes, "image/jpeg", $"compressed-{DateTime.Now.Ticks}.jpg");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la compress: {ex.Message}");
            }
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
                return BadRequest("Nu a fost selectata nicio imagine.");

            if (string.IsNullOrEmpty(text))
                return BadRequest("Textul nu poate fi gol.");

            try
            {
                var userId = _baseService.GetUserId();
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    6, // tipProcesareId pentru watermark
                    async img =>
                    {
                        var font = SystemFonts.CreateFont("Arial", fontSize);
                        var padding = 20;
                        float estimatedWidth = text.Length * fontSize * 0.6f;
                        float estimatedHeight = fontSize * 1.2f;

                        float x = 0, y = 0;

                        switch (position?.ToLower())
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

                        var baseColor = Color.Parse(color);
                        var colorWithOpacity = baseColor.WithAlpha(opacity / 100f);

                        img.Mutate(ctx =>
                        {
                            ctx.DrawText(text, font, colorWithOpacity, new PointF(x, y));
                        });

                        await Task.CompletedTask;
                    }
                );

                return File(imageBytes, "image/png", $"watermarked-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la watermark: {ex.Message}");
            }
        }

        [HttpPost("transfer-colors")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferColors(
            [FromForm] IFormFile sourceImage,
            [FromForm] IFormFile referenceImage)
        {
            if (sourceImage == null || sourceImage.Length == 0)
                return BadRequest("Nu a fost selectata imaginea sursa.");

            if (referenceImage == null || referenceImage.Length == 0)
                return BadRequest("Nu a fost selectata imaginea de referintă.");

            try
            {
                var userId = _baseService.GetUserId();
                using var reference = await Image.LoadAsync<Rgba32>(referenceImage.OpenReadStream());
                var refStats = ColorHelper.GetColorStatistics(reference);

                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    sourceImage,
                    userId,
                    10003, 
                    async img =>
                    {
                        var source = img as Image<Rgba32> ?? img.CloneAs<Rgba32>();
                        source.Mutate(x => x.BackgroundColor(Color.White));

                        var srcStats = ColorHelper.GetColorStatistics(source);
                        source.Mutate(ctx =>
                            ctx.ProcessPixelRowsAsVector4(row =>
                            {
                                for (int i = 0; i < row.Length; i++)
                                {
                                    Vector4 v = row[i];
                                    float newR = ColorHelper.MapColorChannel(v.X, srcStats.MeanR, refStats.MeanR, srcStats.StdR, refStats.StdR);
                                    float newG = ColorHelper.MapColorChannel(v.Y, srcStats.MeanG, refStats.MeanG, srcStats.StdG, refStats.StdG);
                                    float newB = ColorHelper.MapColorChannel(v.Z, srcStats.MeanB, refStats.MeanB, srcStats.StdB, refStats.StdB);
                                    row[i] = new Vector4(newR, newG, newB, v.W);
                                }
                            })
                        );

                        await Task.CompletedTask;
                    }
                );

                return File(imageBytes, "image/png", $"color-transfer-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la transfer-colors: {ex.Message}");
            }
        }
    }
}