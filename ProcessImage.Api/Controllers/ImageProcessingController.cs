using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using Color = SixLabors.ImageSharp.Color;
using SixLabors.ImageSharp.PixelFormats;
using ProcessImage.Helpers.ProcessImage.Helpers;
using System.Numerics;
using ProcessImage.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using ProcessImage.Helpers;
using ProcessImage.Models.Enum;

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
            var userId = _baseService.GetUserId();
            var logger = LoggerAppender.GetLoggerForApplicationType(
               LoggerAppender.LogApplication.Utilizatori,
               userId
            );
            logger.Info($"Utilizatorul {userId} a început procesarea Resize.");
            string userEmail = _baseService.GetUserEmail();
            var TipProcesare = await _imageProcessingService.GetTipProcesareId();
            var tipProcesareResize = TipProcesare.FirstOrDefault(x => x.Nume == TipProcesareEnum.Resize.ToString());
            if (tipProcesareResize == null)
            {
                throw new InvalidOperationException($"TipProcesare pentru '{TipProcesareEnum.Resize.ToString()}' nu a fost gasit!");

            }
            try
            {
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    tipProcesareResize.Id, // tipProcesareId pentru resize
                    async img =>
                    {
                        int width = (int)(img.Width * (scale / 100.0));
                        int height = (int)(img.Height * (scale / 100.0));
                        img.Mutate(x => x.Resize(width, height));
                        await Task.CompletedTask;
                    }
                );
                logger.Info($"Utilizatorul {userId} a procesat cu succes procesarea Resize.");
                return File(imageBytes, "image/png", $"resized-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                LoggerAppender.LogInformation(LoggerAppender.LogApplication.ProcessImage,
                    userId,
                    LoggerTypeEnum.Error,
                    $"Eroare la procesare: {ex.Message}"
                );
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
            var userId = _baseService.GetUserId();
            string userEmail = _baseService.GetUserEmail();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.Utilizatori,
                userId
            );
            logger.Info($"Utilizatorul {userId} a început procesarea Crop.");
            var TipProcesare = await _imageProcessingService.GetTipProcesareId();
            var tipProcesareCrop = TipProcesare.FirstOrDefault(x => x.Nume == TipProcesareEnum.Crop.ToString());
            if (tipProcesareCrop == null)
            {
                throw new InvalidOperationException($"TipProcesare pentru '{TipProcesareEnum.Crop.ToString()}' nu a fost gasit!");

            }
            try
            {
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    tipProcesareCrop.Id, // tipProcesareId pentru crop
                    async img =>
                    {
                        if (x < 0 || y < 0 || x + width > img.Width || y + height > img.Height)
                            throw new ArgumentException("Coordonatele de decupare sunt în afara imaginii.");

                        img.Mutate(i => i.Crop(new Rectangle(x, y, width, height)));
                        await Task.CompletedTask;
                    }
                );
                logger.Info($"Utilizatorul {userId} a procesat cu succes procesarea Crop.");
                return File(imageBytes, "image/png", $"cropped-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                LoggerAppender.LogInformation(LoggerAppender.LogApplication.ProcessImage,
                    userId,
                    LoggerTypeEnum.Error,
                    $"Eroare la procesare: {ex.Message}"
                );
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
            var userId = _baseService.GetUserId();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.Utilizatori,
                userId
            );
            logger.Info($"Utilizatorul {userId} a început procesarea Rotate.");
            string userEmail = _baseService.GetUserEmail();
            var TipProcesare = await _imageProcessingService.GetTipProcesareId();
            var tipProcesareRotate = TipProcesare.FirstOrDefault(x => x.Nume == TipProcesareEnum.Rotate.ToString());
            if (tipProcesareRotate == null)
            {
                throw new InvalidOperationException($"TipProcesare pentru '{TipProcesareEnum.Rotate.ToString()}' nu a fost gasit!");

            }
            try
            {
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    tipProcesareRotate.Id, // tipProcesareId pentru rotate
                    async img =>
                    {
                        img.Mutate(i => i.Rotate(angle));
                        await Task.CompletedTask;
                    }
                );
                logger.Info($"Utilizatorul {userId} a procesat cu succes procesarea Rotate.");
                return File(imageBytes, "image/png", $"rotated-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                LoggerAppender.LogInformation(LoggerAppender.LogApplication.ProcessImage,
                    
                    userId,
                    LoggerTypeEnum.Error,
                    $"Eroare la procesare: {ex.Message}"
                );
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
            var userId = _baseService.GetUserId();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.Utilizatori,
                userId
            );
            logger.Info($"Utilizatorul {userId} a început procesarea Filter.");
            string userEmail = _baseService.GetUserEmail();
            var TipProcesare = await _imageProcessingService.GetTipProcesareId();
            var tipProcesareFilter = TipProcesare.FirstOrDefault(x => x.Nume == TipProcesareEnum.Filter.ToString());
            if (tipProcesareFilter == null)
            {
                throw new InvalidOperationException($"TipProcesare pentru '{TipProcesareEnum.Filter.ToString()}' nu a fost gasit!");

            }
            try
            {
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    tipProcesareFilter.Id, // tipProcesareId pentru filter
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
                logger.Info($"Utilizatorul {userId} a procesat cu succes procesarea Filter.");
                return File(imageBytes, "image/png", $"filtered-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                LoggerAppender.LogInformation(LoggerAppender.LogApplication.ProcessImage,
                    userId,
                    LoggerTypeEnum.Error,
                    $"Eroare la procesare: {ex.Message}"
                );
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
            var userId = _baseService.GetUserId();
            string userEmail = _baseService.GetUserEmail();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.Utilizatori,
                userId
            );
            logger.Info($"Utilizatorul {userId} a început procesarea Compress.");
            var TipProcesare = await _imageProcessingService.GetTipProcesareId();
            var tipProcesareCompress = TipProcesare.FirstOrDefault(x => x.Nume == TipProcesareEnum.Compress.ToString());
            if (tipProcesareCompress == null)
            {
                throw new InvalidOperationException($"TipProcesare pentru '{TipProcesareEnum.Compress.ToString()}' nu a fost gasit!");
            }

            try
            {
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    tipProcesareCompress.Id, // tipProcesareId pentru compress
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
                logger.Info($"Utilizatorul {userId} a procesat cu succes procesarea Compress");
                return File(imageBytes, "image/jpeg", $"compressed-{DateTime.Now.Ticks}.jpg");
            }
            catch (Exception ex)
            {
                LoggerAppender.LogInformation(LoggerAppender.LogApplication.ProcessImage,
                    userId,
                    LoggerTypeEnum.Error,
                    $"Eroare la procesare: {ex.Message}"
                );
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
            var userId = _baseService.GetUserId();
            string userEmail = _baseService.GetUserEmail();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.Utilizatori,
                userId
            );
            logger.Info($"Utilizatorul {userId} a început procesarea Watermark.");

            //optimazare linii de jos
            var TipProcesare = await _imageProcessingService.GetTipProcesareId();
            var tipProcesareWatermark = TipProcesare.FirstOrDefault(x => x.Nume == TipProcesareEnum.Watermark.ToString());
            if (tipProcesareWatermark == null)
            {
                throw new InvalidOperationException($"TipProcesare pentru '{TipProcesareEnum.Watermark.ToString()}' nu a fost gasit!");
            }

            try
            {
                
                var imageBytes = await _imageProcessingService.ProcessAndSaveImageAsync(
                    image,
                    userId,
                    tipProcesareWatermark.Id, // tipProcesareId pentru watermark
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
                logger.Info($"Utilizatorul {userId} a procesat cu succes procesarea Watermark.");
                return File(imageBytes, "image/png", $"watermarked-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                LoggerAppender.LogInformation(LoggerAppender.LogApplication.ProcessImage,
                    userId,
                    LoggerTypeEnum.Error,
                    $"Eroare la procesare: {ex.Message}"
                );
                return BadRequest($"Eroare la watermark: {ex.Message}");
            }
        }

        [HttpPost("transfer-colors")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferColors([FromForm] IFormFile sourceImage,[FromForm] IFormFile referenceImage)
        {
            if (sourceImage == null || sourceImage.Length == 0)
                return BadRequest("Nu a fost selectata imaginea sursa.");

            if (referenceImage == null || referenceImage.Length == 0)
                return BadRequest("Nu a fost selectata imaginea de referintă.");
            var userId = _baseService.GetUserId();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                  LoggerAppender.LogApplication.Utilizatori,
                  userId
              );
            string userEmail = _baseService.GetUserEmail();
            logger.Info($"Utilizatorul {userId} a început procesarea Transfer Colors.");
            var TipProcesare = await _imageProcessingService.GetTipProcesareId();
            var tipProcesareWatermark = TipProcesare.FirstOrDefault(x => x.Nume == TipProcesareEnum.TransferColor.ToString());
            if (tipProcesareWatermark == null)
            {
                throw new InvalidOperationException($"TipProcesare pentru '{TipProcesareEnum.TransferColor.ToString()}' nu a fost gasit!");
            }

            try
            {
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
                logger.Info($"Utilizatorul {userId} a procesat cu succes Transfer Colors.");
                return File(imageBytes, "image/png", $"color-transfer-{DateTime.Now.Ticks}.png");
            }
            catch (Exception ex)
            {
                LoggerAppender.LogInformation(LoggerAppender.LogApplication.ProcessImage,
                    userId,
                    LoggerTypeEnum.Error,
                    $"Eroare la procesare: {ex.Message}"
                );

                return BadRequest($"Eroare la transfer-colors: {ex.Message}");
            }
        }
        [HttpGet("GetProcessingTypes")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetProcessingTypes()
        {
            try
            {
                var tipuriProcesare = await _imageProcessingService.GetTipProcesareId();
                var result = tipuriProcesare.Select(t => new
                {
                    t.Id,
                    t.Nume,
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Eroare la preluarea tipurilor de procesare: {ex.Message}");
            }
        }
    }
}