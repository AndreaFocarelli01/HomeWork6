using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HomeWorkStat6
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int sampleSize = int.Parse(textBox1.Text);
            int categories = int.Parse(textBox2.Text);

            // Create graphics objects
            var empiricalGraph = pictureBox1.CreateGraphics();
            var theoreticalGraph = pictureBox2.CreateGraphics();
            empiricalGraph.Clear(Color.White);
            theoreticalGraph.Clear(Color.White);

            // Generate random discrete distribution
            var theoreticalProbs = GenerateDistribution(categories);

            // Simulate draws and track frequencies
            var (empiricalFreqs, observations) = SimulateDraws(theoreticalProbs, sampleSize);

            // Calculate statistics using online algorithms
            var theoreticalStats = CalculateTheoretical(theoreticalProbs);
            var empiricalStats = CalculateEmpirical(observations);

            // Visualize distributions
            DrawDistribution(empiricalGraph, empiricalFreqs.Select(f => (double)f / sampleSize).ToArray(),
                "Empirical Distribution", Color.RoyalBlue, empiricalStats);
            DrawDistribution(theoreticalGraph, theoreticalProbs,
                "Theoretical Distribution", Color.Crimson, theoreticalStats);
        }

        private double[] GenerateDistribution(int categories)
        {
            var random = new Random();
            var probs = new double[categories];
            double remainingProb = 1.0;

            // Generate probabilities that sum to 1
            for (int i = 0; i < categories - 1; i++)
            {
                double p = random.NextDouble() * remainingProb * 0.9;
                probs[i] = p;
                remainingProb -= p;
            }
            probs[categories - 1] = remainingProb;

            return probs;
        }

        private (int[] frequencies, List<int> observations) SimulateDraws(double[] probabilities, int sampleSize)
        {
            var random = new Random();
            var frequencies = new int[probabilities.Length];
            var observations = new List<int>(sampleSize);

            for (int i = 0; i < sampleSize; i++)
            {
                double r = random.NextDouble();
                double cumulative = 0;

                for (int j = 0; j < probabilities.Length; j++)
                {
                    cumulative += probabilities[j];
                    if (r <= cumulative)
                    {
                        frequencies[j]++;
                        observations.Add(j);
                        break;
                    }
                }
            }

            return (frequencies, observations);
        }

        private (double mean, double variance) CalculateTheoretical(double[] probabilities)
        {
            // Calculate mean
            double mean = 0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                mean += i * probabilities[i];
            }

            // Calculate variance
            double variance = 0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                variance += Math.Pow(i - mean, 2) * probabilities[i];
            }

            return (mean, variance);
        }

        private (double mean, double variance) CalculateEmpirical(List<int> observations)
        {
            // Welford's online algorithm for mean and variance
            double mean = 0;
            double M2 = 0;
            int n = 0;

            foreach (var value in observations)
            {
                n++;
                double delta = value - mean;
                mean += delta / n;
                double delta2 = value - mean;
                M2 += delta * delta2;
            }

            double variance = n > 1 ? M2 / (n - 1) : 0;
            return (mean, variance);
        }

        private void DrawDistribution(Graphics g, double[] probabilities, string title, Color color,
            (double mean, double variance) stats)
        {
            float width = pictureBox1.Width;
            float height = pictureBox1.Height;
            float margin = 40;
            float graphWidth = width - 2 * margin;
            float graphHeight = height - 2 * margin;
            float barWidth = graphWidth / probabilities.Length - 4;

            // Clear and set up coordinate system
            g.Clear(Color.White);
            g.TranslateTransform(margin, height - margin);
            g.ScaleTransform(1, -1);  // Flip Y-axis

            // Draw axes
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 0, 0, graphWidth, 0);  // X-axis
                g.DrawLine(pen, 0, 0, 0, graphHeight); // Y-axis
            }

            // Draw bars
            using (var brush = new SolidBrush(color))
            {
                for (int i = 0; i < probabilities.Length; i++)
                {
                    float x = i * (barWidth + 4);
                    float barHeight = (float)(probabilities[i] * graphHeight);
                    g.FillRectangle(brush, x, 0, barWidth, barHeight);
                }
            }

            // Reset transform for text
            g.ResetTransform();

            // Draw labels
            using (var font = new Font("Arial", 8))
            {
                // Y-axis labels
                for (int i = 0; i <= 10; i++)
                {
                    float y = height - margin - (i * graphHeight / 10);
                    string label = $"{i * 10}%";
                    g.DrawString(label, font, Brushes.Black, 5, y - 6);
                }

                // X-axis labels
                for (int i = 0; i < probabilities.Length; i++)
                {
                    float x = margin + i * (barWidth + 4);
                    g.DrawString(i.ToString(), font, Brushes.Black, x, height - margin + 5);
                }
            }

            // Draw title and statistics
            using (var titleFont = new Font("Arial", 10, FontStyle.Bold))
            {
                string statsText = $"{title}\nMean: {stats.mean:F3}\nVariance: {stats.variance:F3}";
                g.DrawString(statsText, titleFont, Brushes.Black, width / 4, 10);
            }
        }
    }
}
