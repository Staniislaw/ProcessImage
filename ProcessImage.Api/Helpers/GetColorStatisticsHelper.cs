namespace ProcessImage.Helpers
{
    using System;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    namespace ProcessImage.Helpers
    {
        public static class ColorHelper
        {
            public class ColorStatistics
            {
                public float MeanR { get; set; }
                public float MeanG { get; set; }
                public float MeanB { get; set; }
                public float StdR { get; set; }
                public float StdG { get; set; }
                public float StdB { get; set; }
            }
            public static ColorStatistics GetColorStatistics(Image<Rgba32> image)
            {
                var stats = new ColorStatistics();
                float sumR = 0, sumG = 0, sumB = 0;
                long pixelCount = 0;

                // Prima trecere: calculează media
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < row.Length; x++)
                        {
                            var pixel = row[x];
                            sumR += pixel.R / 255f;
                            sumG += pixel.G / 255f;
                            sumB += pixel.B / 255f;
                            pixelCount++;
                        }
                    }
                });

                stats.MeanR = sumR / pixelCount;
                stats.MeanG = sumG / pixelCount;
                stats.MeanB = sumB / pixelCount;
                float sumSqR = 0, sumSqG = 0, sumSqB = 0;

                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < row.Length; x++)
                        {
                            var pixel = row[x];
                            float r = pixel.R / 255f;
                            float g = pixel.G / 255f;
                            float b = pixel.B / 255f;

                            sumSqR += (r - stats.MeanR) * (r - stats.MeanR);
                            sumSqG += (g - stats.MeanG) * (g - stats.MeanG);
                            sumSqB += (b - stats.MeanB) * (b - stats.MeanB);
                        }
                    }
                });

                stats.StdR = (float)Math.Sqrt(sumSqR / pixelCount);
                stats.StdG = (float)Math.Sqrt(sumSqG / pixelCount);
                stats.StdB = (float)Math.Sqrt(sumSqB / pixelCount);

                return stats;
            }
            public static float MapColorChannel(float sourceValue, float srcMean, float refMean, float srcStd, float refStd)
            {
                float normalized = srcStd > 0 ? (sourceValue - srcMean) / srcStd : 0;
                return (normalized * refStd) + refMean;
            }
        }
    }


}
