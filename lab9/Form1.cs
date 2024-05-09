using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using System.Windows.Forms;

namespace lab9
{
    public partial class Form1 : Form
    {

        private int currentIndex = 0;
        private int[] N = new int[] { 10, 100, 1000, 10000 };  // Разные значения N
        public Form1()
        {
            InitializeComponent();
            AddDataToDataGridView();
        }

        private void AddDataToDataGridView()
        {
            // Добавление строк с данными
            string[] row0 = { "Все будет отлично", "0,20" };
            string[] row1 = { "Все будет хорошо", "0,20" };
            string[] row2 = { "Все будет неплохо", "0,10" };
            string[] row3 = { "Все будет плохо", "0,10" };
            string[] row4 = { "Все будет отвратительно", "0,25" };
            string[] row5 = { "Все будет ужасно отвратительно", "0,15" };

            probabilitiesDataGridView.Rows.Add(row0);
            probabilitiesDataGridView.Rows.Add(row1);
            probabilitiesDataGridView.Rows.Add(row2);
            probabilitiesDataGridView.Rows.Add(row3);
            probabilitiesDataGridView.Rows.Add(row4);
            probabilitiesDataGridView.Rows.Add(row5);
        }

        private void SetupChart()
        {
            chart1.Series.Clear();

            //две серии: одна для теоретических и одна для эмпирических вероятностей
            var seriesTheoretical = chart1.Series.Add("Theoretical Probabilities");
            seriesTheoretical.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            seriesTheoretical.Color = Color.Blue;

            var seriesEmpirical = chart1.Series.Add("Empirical Probabilities");
            seriesEmpirical.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            seriesEmpirical.Color = Color.Red;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            if (currentIndex >= N.Length)
            {
                MessageBox.Show("Все эксперименты выполнены");
                currentIndex = 0;  // Сбросить индекс, если нужно начать заново
                return;  // Выход, если все эксперименты уже выполнены
            }
            int n = N[currentIndex];
            SetupChart();
            List<double> probabilities = new List<double>();
            List<string> outcomes = new List<string>();

            foreach (DataGridViewRow row in probabilitiesDataGridView.Rows)
            {
                if (row.Cells[1].Value != null)
                {
                    double p = Convert.ToDouble(row.Cells[1].Value);
                    if (p < 0 || p > 1)
                    {
                        MessageBox.Show("Вероятности должны быть в диапазоне от 0 до 1");
                        return;
                    }
                    probabilities.Add(p);
                    outcomes.Add(row.Cells[0].Value.ToString());
                }
            }

            if (Math.Abs(probabilities.Sum() - 1.0) > 0.01) // Проверка суммы вероятностей
            {
                MessageBox.Show("Сумма вероятностей должна быть равна 1");
                return;
            }

            StringBuilder results = new StringBuilder();
            Random rand = new Random();

                var counts = new int[probabilities.Count];

                for (int i = 0; i < n; i++)
                {
                    double r = rand.NextDouble();
                    double cumulative = 0;
                    for (int j = 0; j < probabilities.Count; j++)
                    {
                        cumulative += probabilities[j];
                        if (r <= cumulative)
                        {
                            counts[j]++;
                            break;
                        }
                    }
                }

                // Расчет эмпирических вероятностей, среднего, дисперсии и хи-квадрат
                double empiricalMean = 0;
                double variance = 0;
                double chiSquare = 0;
                for (int j = 0; j < probabilities.Count; j++)
                {
                    double empiricalP = (double)counts[j] / n;
                    empiricalMean += j * empiricalP;
                    variance += j * j * empiricalP;
                    if (counts[j] > 0)
                    {
                        chiSquare += Math.Pow(counts[j] - n * probabilities[j], 2) / (n * probabilities[j]);
                    }
                }
                variance -= empiricalMean * empiricalMean;
            double alpha = 0.05;
            int df = probabilities.Count - 1;
            double takeChiSquareValue = ChiSquared.InvCDF(df, 1 - alpha);
                // Относительные погрешности
                double relativeMeanError = Math.Abs((empiricalMean - probabilities.Sum()) / probabilities.Sum());
                double relativeVarianceError = Math.Abs((variance - probabilities.Sum(p => p * p)) / probabilities.Sum(p => p * p));
                results.AppendLine($"N = {n}: Среднее = {empiricalMean},\n Дисперсия = {variance},\n Хи-квадрат = {chiSquare},\n Относительная погрешность среднего = {relativeMeanError},\n Относительная погрешность дисперсии = {relativeVarianceError},\n Критерий Хи-Квадрат = {takeChiSquareValue}");

            // Расчет статистик и обновление графика
            chart1.Series["Theoretical Probabilities"].Points.Clear();
            chart1.Series["Empirical Probabilities"].Points.Clear();
            for (int j = 0; j < probabilities.Count; j++)
            {
                double empiricalP = (double)counts[j] / n;
                chart1.Series["Theoretical Probabilities"].Points.AddXY(outcomes[j], probabilities[j]);
                chart1.Series["Empirical Probabilities"].Points.AddXY(outcomes[j], empiricalP);
            }

            currentIndex++;

            resultTextBox.Text = results.ToString();
        }
    }
}
