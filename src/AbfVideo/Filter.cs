namespace AbfVideo;

public static class Filter
{
    public static double[] ForwardFilter(double[] values, int pointCount)
    {
        double[] smooth = new double[values.Length];

        double runningSum = 0;

        for (int i = 0; i < smooth.Length; i++)
        {
            runningSum += values[i];

            if (i >= pointCount)
            {
                runningSum -= values[i - pointCount];
                smooth[i] = runningSum / pointCount;
            }
            else
            {
                smooth[i] = runningSum / (i + 1);
            }
        }

        return smooth;
    }

    public static void MovingBaselineSubtract(double[] values, int pointCount)
    {
        double[] baseline = ForwardFilter(values, pointCount);
        for (int i = 0; i < baseline.Length; i++)
        {
            values[i] -= baseline[i];
        }
    }

    public static void Multiply(double[] values, double by)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] *= by;
        }
    }

    public static void Divide(double[] values, double by)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] /= by;
        }
    }

    public static void Add(double[] values, double by)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] += by;
        }
    }
}
