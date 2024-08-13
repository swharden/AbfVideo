namespace AbfVideo;

public static class Program
{
    public static void Main()
    {
        string abfPath = Path.GetFullPath(@"../../../../../dev/sample-data/3ch.abf");
        if (!File.Exists(abfPath))
            throw new FileNotFoundException(abfPath);
        ScottPlot.Plot plot = GetPlot(abfPath, 5, 30, 250);

        bool preview = true;

        if (preview)
        {
            plot.SavePng("test.png", 600, 400).LaunchInBrowser();
        }
        else
        {
            string saveAs = Path.GetFullPath($"output-{DateTime.Now.Ticks}.webm");
            VideoGenerator.Generate(plot, 0, 5, saveAs);
            Console.WriteLine(saveAs);
        }
    }

    static ScottPlot.Plot GetPlot(string abfPath, double scale1, double scale2, double scale3)
    {
        AbfSharp.ABF abf = new(abfPath);

        double[] sigEEG = abf.GetAllData(0);
        Filter.Divide(sigEEG, 5);
        Filter.Add(sigEEG, 6);

        double[] sigRESP = abf.GetAllData(1);
        Filter.Divide(sigRESP, 30);
        Filter.Add(sigRESP, 3);

        double[] sigECG = abf.GetAllData(2);
        Filter.MovingBaselineSubtract(sigECG, (int)abf.SampleRate);
        Filter.Divide(sigECG, 250);

        double[] xs = ScottPlot.Generate.Consecutive(sigEEG.Length, 0.001);

        ScottPlot.Plot plot = new();
        plot.Add.ScatterLine(xs, sigEEG);
        plot.Add.ScatterLine(xs, sigRESP);
        plot.Add.ScatterLine(xs, sigECG);
        /*
        plot.Add.Signal(sigEEG, .001);
        plot.Add.Signal(sigRESP, .001);
        plot.Add.Signal(sigECG, .001);
        */

        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
        plot.Axes.Left.FrameLineStyle.IsVisible = false;
        plot.Axes.Right.FrameLineStyle.IsVisible = false;
        plot.Axes.Bottom.FrameLineStyle.IsVisible = false;
        plot.Axes.Top.FrameLineStyle.IsVisible = false;

        ScottPlot.TickGenerators.NumericManual tickGen = new();
        plot.Axes.Bottom.TickGenerator = tickGen;
        for (int i = 0; i < 100; i += 1)
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
