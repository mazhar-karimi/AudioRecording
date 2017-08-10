using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System.IO;

namespace Sound1
{
    public partial class Form1 : Form
    {
        int n = 4000; // number of x-axis pints
        WaveIn s_WaveIn;
        private FileStream memoryStream;
        private WaveFileWriter writer;
        Queue<double> myQ;
        public Form1()
        {
            InitializeComponent();

            myQ = new Queue<double>(Enumerable.Repeat(0.0, n).ToList()); // fill myQ w/ zeros
            chart1.ChartAreas[0].AxisY.Minimum = -10000;
            chart1.ChartAreas[0].AxisY.Maximum = 10000;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (memoryStream == null)
                memoryStream = new FileStream(System.Environment.CurrentDirectory + "\\" +
                    Guid.NewGuid().ToString().Replace("-", "") + ".wav", FileMode.CreateNew);

            s_WaveIn = new WaveIn();
            s_WaveIn.WaveFormat = new WaveFormat(44100, 2);

            s_WaveIn.BufferMilliseconds = 1000;
            s_WaveIn.DataAvailable += new EventHandler<WaveInEventArgs>(SendCaptureSamples);
            this.writer = new WaveFileWriter(this.memoryStream, this.s_WaveIn.WaveFormat);
            s_WaveIn.RecordingStopped += s_WaveIn_RecordingStopped;
            s_WaveIn.StartRecording();
            timer2.Enabled = true;

        }

        void s_WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (s_WaveIn != null)
            {
                s_WaveIn.Dispose();
                s_WaveIn = null;
            }

            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }

        void SendCaptureSamples(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                myQ.Enqueue(BitConverter.ToInt16(e.Buffer, i));
                myQ.Dequeue();
            }

            if (writer != null)
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                writer.Flush();
            } 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            s_WaveIn.StopRecording();
            if (memoryStream != null)
            {
                memoryStream.Close();
                memoryStream.Dispose();
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                chart1.Series["Series1"].Points.DataBindY(myQ);
                //chart1.ResetAutoValues();
            }
            catch
            {
                Console.WriteLine("No bytes recorded");
            }
        }


    }
}
