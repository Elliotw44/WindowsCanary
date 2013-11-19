using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MiningMonitorClientW
{
    public partial class form1 : Form
    {
        public static bool running = false;
        private static System.Timers.Timer timer1;
     
        public form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length != 0 && textBox2.Text.Trim().Length != 0 && !running)
            {
                timer1 = new System.Timers.Timer(180000);
                timer1.Elapsed += new  System.Timers.ElapsedEventHandler(onTimerEvent);
                timer1.Start();
                this.button1.Text = "Stop Monitoring";
                running = true;
                this.progressBar1.Style = ProgressBarStyle.Marquee;
                this.progressBar1.MarqueeAnimationSpeed = 30;
            }
            else if (running)
            {
                this.button1.Text = "Begin Monitoring!";
                this.progressBar1.MarqueeAnimationSpeed = 0;
                timer1.Stop();
                running = false;
            }
            else 
            {
                textBox1.Text = "Enter the worker name from the website here please";
                textBox2.Text = "Enter your username from the website here please";
            }
        }
         //run the worker job every 3 minutes
        private void onTimerEvent(object sender, EventArgs e)
        {
                minerQuery.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string user_worker = textBox2.Text.Trim().ToLower() + ":" + textBox1.Text.Trim().ToLower();
            bool logging = false;
            if (this.checkBox1.Checked)
            {
                logging = true;
            }
            else
            {
                logging = false;
            }
            WorkerUpdate workerUpdate = new WorkerUpdate();
            workerUpdate.update(user_worker, logging);
        }
    }
}
