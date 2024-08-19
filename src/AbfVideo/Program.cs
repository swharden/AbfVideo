namespace AbfVideo;

public static class Program
{
    public static void Main()
    {
        string abfPath = Path.GetFullPath(@"../../../../../dev/sample-data/3ch.abf");
        if (!File.Exists(abfPath))
            throw new FileNotFoundException(abfPath);

        ScottPlot.Plot plot = GetPlot(abfPath, .3, .4, 0);
        double minutes = 11;
        double seconds = 30;
        double startTime = minutes * 60 + seconds;
        double endTime = startTime + 10;

        bool preview = false;

        if (preview)
        {
            plot.Axes.SetLimitsX(startTime, endTime);
            plot.SavePng("test.png", 600, 400).LaunchInBrowser();
        }
        else
        {
            string saveAs = Path.GetFullPath($"AbfVideo " +
                $"{Path.GetFileNameWithoutExtension(abfPath)} " +
                $"{startTime}sec " +
                $"{DateTime.Now.Ticks}.webm");
            VideoGenerator.Generate(plot, startTime, endTime, saveAs);
            Console.WriteLine(saveAs);
            System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{saveAs}\"");
        }
    }

    static ScottPlot.Plot GetPlot(string abfPath, double divideCh1, double divideCh2, double divideCh3)
    {
        ScottPlot.Plot plot = new();

        AbfSharp.ABF abf = new(abfPath);

        double[] sigEEG = abf.GetAllData(0);
        double[] xs = ScottPlot.Generate.Consecutive(sigEEG.Length, 0.001);
        Filter.Divide(sigEEG, divideCh1);
        Filter.Add(sigEEG, 6);
        plot.Add.ScatterLine(xs, sigEEG);

        double[] sigRESP = abf.GetAllData(1);
        Filter.Divide(sigRESP, divideCh2);
        Filter.Add(sigRESP, 3);
        plot.Add.ScatterLine(xs, sigRESP);

        if (divideCh3 != 0)
        {
            double[] sigECG = abf.GetAllData(2);
            Filter.MovingBaselineSubtract(sigECG, (int)abf.SampleRate);
            Filter.Divide(sigECG, divideCh3);
            plot.Add.ScatterLine(xs, sigECG);
        }

        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
        plot.Axes.Left.FrameLineStyle.IsVisible = false;
        plot.Axes.Right.FrameLineStyle.IsVisible = false;
        plot.Axes.Bottom.FrameLineStyle.IsVisible = false;
        plot.Axes.Top.FrameLineStyle.IsVisible = false;

        ScottPlot.TickGenerators.NumericManual tickGen = new();
        plot.Axes.Bottom.TickGenerator = tickGen;
        for (int i = 0; i < (int)xs.Last(); i += 1)
        {
            int minutes = i / 60;
            int seconds = i - (minutes * 60);
            tickGen.AddMajor(i, $"{minutes}:{seconds:00}");
        }

        plot.Axes.SetLimitsX(0, 10);
        plot.Axes.SetLimitsY(-1, 8);

        return plot;
    }
}
