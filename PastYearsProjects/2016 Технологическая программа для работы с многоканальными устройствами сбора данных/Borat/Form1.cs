using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Borat
{
    public partial class Form1 : Form
    {
        int x;

        int pointsCount = 300;

        byte curChNum = 0;

        AmplitudeCalcer RawValues = new AmplitudeCalcer();
        Meaner SignalMeaner = new Meaner { Capacity = 5 };
        Medianer SignalMedianer = new Medianer { Capacity = 5 };


        Device polygraph;
        private string AdcUnitsTitle = "мВ";

        public Form1()
        {
            InitializeComponent();
            RawValues.Capacity = 50;
            RawValues.Put(0);
            RawValues.Put(1);
        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (polygraph != null)
            {
                polygraph.Stop();

            }


            if (polygraph == null)
                if (checkBox2.Checked) polygraph = new BoratOneChanDevice(); else polygraph = new BoratDevice();

            if (!polygraph.TryFindDevice())
            {
                MessageBox.Show("Полиграф не найден");

                return;
            }

            polygraph.OnDatapack += Polygraph_OnDatapack;
            polygraph.Start();


            Text = "Полиграф OK";
            button1.Enabled = false;
            checkBox2.Enabled = false;


            chart1.Series.Clear();
            for (int i = 0; i < 1; i++)
            {
                var ser = new System.Windows.Forms.DataVisualization.Charting.Series();
                ser.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                ser.Color = Color.Black;
                chart1.Series.Add(ser);

            }

            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;

            SetYScale(1);

            //chart1.Annotations.Add(new VerticalLineAnnotation { X = 10, Visible = true, AnchorX = 20, AxisX = chart1.ChartAreas[0].AxisX, AxisY = chart1.ChartAreas[0].AxisY });

            listBox1.SelectedIndex = 0;
            listBox2.SelectedIndex = 3;
            listBox3.SelectedIndex = 0;
            vScrollBar1_Scroll(null, null);

            if (!checkBox2.Checked)
            {
                listBox1.Items.Clear();
                listBox1.Items.Add("трм1");
                listBox1.Items.Add("трм2");
                listBox1.Items.Add("трм3");
                listBox1.Items.Add("кр1");
                listBox1.Items.Add("кр2");
                listBox1.Items.Add("фпг");
                listBox1.Items.Add("дх1");
                listBox1.Items.Add("дх2");
                listBox1.Items.Add("мнж1");
                listBox1.Items.Add("мнж2");
                listBox1.SelectedIndex = 0;

                vScrollBar1.Visible = false;

                AdcUnitsTitle = "уе";
            }
        }

        private void SetYScale(float percent, int middle = 0)
        {
            var maxAdc = (polygraph is BoratOneChanDevice) ? 3300 : 20000;
            var range = maxAdc / percent;
            var minimum = middle - range / 2;
            if (percent < 2) minimum = 0;
            var maximum = minimum + range;
            
            chart1.ChartAreas[0].AxisY.Minimum = minimum;
            chart1.ChartAreas[0].AxisY.Maximum = maximum;
        }

        private void AddDrawDatapack(int[] dp)
        {

            if (polygraph is BoratDevice) dp[0] = dp[curChNum];

            if (checkBox1.Checked) dp[0] = SignalMeaner.PutGet(SignalMedianer.PutGet(dp[0]));

                for (int i = 0; i < 1; i++)
                {
                    var datumIndex = (dp.Length == 16) ? i : 0;
                    RawValues.Put(dp[datumIndex]);

                    var ser = chart1.Series[i];


                ser.Points.AddXY(x, dp[datumIndex]);



                while (ser.Points.Count > pointsCount)
                        ser.Points.RemoveAt(0);
                }
         


            chart1.ChartAreas[0].AxisX.Minimum = x- pointsCount;
            chart1.ChartAreas[0].AxisX.Maximum = x;

           

            x += 1;



            //var ann = chart1.Annotations[0];
            //if (ann.X < chart1.ChartAreas[0].AxisX.Minimum) ann.X = chart1.ChartAreas[0].AxisX.Maximum;

            label3.Text = string.Format("АЦП: {2} {3}\nСреднее: {0} {3}\nРазмах: {1} {3}", RawValues.Mean, RawValues.Ampl, dp[0], AdcUnitsTitle);

            if (checkBoxAutoBias.Checked && polygraph is BoratOneChanDevice)
            {
                var bias = (polygraph as BoratOneChanDevice).Bias;
                var deltaadc = dp[0] - 1650;
                var deltabias = deltaadc / 10;
                bias += deltabias;
                if (bias > 4095) bias = 4095; else if (bias < 0) bias = 0;
                vScrollBar1.Value = bias;
                vScrollBar1_Scroll(null, null);
            }
        }

        private void Polygraph_OnDatapack(object sender, DatapackEventArgs e)
        {
            try { chart1.Invoke(new Action(() => { AddDrawDatapack(e.Datapack); }));
            }
            catch { };

        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (polygraph != null)
                if (polygraph is BoratOneChanDevice)
                {
                    (polygraph as BoratOneChanDevice).Bias = vScrollBar1.Value;
                    label1.Text = "ЦАП: " + vScrollBar1.Value.ToString();
                }
            
        }

        private void vScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (polygraph != null)
            {
                polygraph.Stop();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            curChNum = (byte)listBox1.SelectedIndex;
            if (polygraph is BoratOneChanDevice) (polygraph as BoratOneChanDevice).Channel = curChNum;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            float scale = int.Parse(listBox2.SelectedItem as string);
            if (scale < 0) scale = -1 / scale;
            SetYScale(scale, RawValues.Mean);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            pointsCount = listBox3.SelectedIndex * 100 + 100;
        }
    }
}
