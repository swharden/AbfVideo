using FFMpegCore;
using FFMpegCore.Pipes;
using SkiaSharp;

namespace AbfVideo;

internal static class VideoGenerator
{
    public static void Generate(ScottPlot.Plot plot, double startTimeSec, double endTimeSec, string saveAs)
    {
        IEnumerable<IVideoFrame> frames = FrameMaker(plot, startTimeSec, endTimeSec);
        RawVideoPipeSource videoFramesSource = new(frames) { FrameRate = 30 };
        bool success = FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(saveAs, overwrite: true, options => options.WithVideoCodec("libvpx-vp9"))
            .ProcessSynchronously();
    }

    static IEnumerable<IVideoFrame> FrameMaker(ScottPlot.Plot plot, double startTimeSec, double endTimeSec)
    {
        int width = 640;
        int height = 480;
        double viewWidthSec = 5;
        double videoDurationSec = endTimeSec - startTimeSec;
        double frameRate = 30;
        int frameCount = (int)(videoDurationSec * frameRate) + 1;

        for (int i = 0; i < frameCount; i++)
        {
            Console.WriteLine($"\rRendering frame {i + 1} of {frameCount}");

            double viewLeft = i / frameRate + startTimeSec;
            double viewRight = viewLeft + viewWidthSec;
            plot.Axes.SetLimitsX(viewLeft, viewRight);

            // render just the portion in view to enhance performance
            foreach (var sp in plot.GetPlottables<ScottPlot.Plottables.Scatter>())
            {
                sp.Data.MinRenderIndex = (int)(viewLeft / 0.001);
                sp.Data.MaxRenderIndex = (int)(viewRight / 0.001);
            }

            using SKBitmap bmp = new(width, height);
            using SKCanvas canvas = new(bmp);
            using SKBitmapFrame frame = new(bmp);
            plot.Render(canvas, width, height);
            yield return frame;
        }
    }
    class SKBitmapFrame(SKBitmap bmp) : IVideoFrame, IDisposable
    {
        public int Width => Source.Width;
        public int Height => Source.Height;
        public string Format => "bgra";
        private readonly SKBitmap Source = (bmp.ColorType == SKColorType.Bgra8888) ? bmp
            : throw new ArgumentException("Bitmap ColorType must be Bgra8888");
        public void Dispose() =>
            Source.Dispose();
        public void Serialize(Stream pipe) =>
            pipe.Write(Source.Bytes, 0, Source.Bytes.Length);
        public Task SerializeAsync(Stream pipe, CancellationToken token) =>
            pipe.WriteAsync(Source.Bytes, 0, Source.Bytes.Length, token);
    }

}
