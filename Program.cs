using System;
using System.IO;
using System.Linq;
using System.Numerics;
using NumFlat;
using NumFlat.SignalProcessing;

static class Program
{
    static void Main(string[] args)
    {
        var sampleRate = 48000;
        var frameLength = 4096;
        var frameShift = frameLength / 2;
        var window = WindowFunctions.Hann(frameLength);

        var random = new Random(42);
        var signal1 = VectorBuilder.FromFunc(3 * sampleRate, i => random.NextDouble() - 0.5);

        var signal2 = new Vec<double>(signal1.Count);
        signal1.Subvector(0, signal1.Count - 2).CopyTo(signal2.Subvector(2, signal2.Count - 2));

        Console.WriteLine(signal1);
        Console.WriteLine(signal2);

        var stft1 = signal1.Stft(window, frameShift);
        var stft2 = signal2.Stft(window, frameShift);

        using (var writer = new StreamWriter("out.csv"))
        {
            writer.WriteLine("freq,delay");
            for (var t = 0; t < stft1.Spectrogram.Length; t++)
            {
                var spectrum1 = stft1.Spectrogram[t];
                var spectrum2 = stft2.Spectrogram[t];
                var delays = EstimatePerFrequencyDelays(frameLength, spectrum1, spectrum2);
                for (var w = 0; w < spectrum1.Count; w++)
                {
                    var freq = (double)sampleRate * w / frameLength;
                    if (300 <= freq && freq <= 5000)
                    {
                        writer.WriteLine(freq + "," + delays[w]);
                    }
                }
            }
        }
    }

    static Vec<double> EstimatePerFrequencyDelays(int frameLength, Vec<Complex> x, Vec<Complex> y)
    {
        var delays = new Vec<double>(x.Count);
        for (var w = 1; w < x.Count; w++)
        {
            var waveLength = (double)frameLength / w;
            var dp = (x[w] * y[w].Conjugate()).Phase;
            delays[w] = dp / (2 * Math.PI) * waveLength;
        }
        return delays;
    }
}
