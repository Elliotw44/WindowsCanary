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
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        //For water marks
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string watermark);
     
        public form1()
        {
            InitializeComponent();
            SendMessage(this.textBox1.Handle, 0x1501, 1, "Enter Worker name");
            SendMessage(this.textBox2.Handle, 0x1501, 1, "Enter User ID");
   
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
                this.textBox1.Enabled = false;
                this.textBox2.Enabled = false;
                this.progressBar1.Style = ProgressBarStyle.Marquee;
                this.progressBar1.MarqueeAnimationSpeed = 30;
            }
            else if (running)
            {
                this.button1.Text = "Begin Monitoring!";
                this.progressBar1.MarqueeAnimationSpeed = 0;
                this.textBox1.Enabled = true;
                this.textBox2.Enabled = true;
                timer1.Stop();
                running = false;
            }
        }
         //run the worker job every 3 minutes
        private void onTimerEvent(object sender, EventArgs e)
        {
                minerQuery.RunWorkerAsync();
        }
        //background worker
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string user_worker = textBox2.Text.Trim().ToLower() + ":" + textBox1.Text.Trim().ToLower();
            bool logging;
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
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void form1_Resize_1(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                Hide();
            }
        }
        //only allow users to enter numbers for their user id
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void form1_Load(object sender, EventArgs e)
        {
            if (CryptoCanary.Properties.Settings.Default.autoStart)
            {
                this.textBox2.Text = CryptoCanary.Properties.Settings.Default.userId;
                this.textBox1.Text = CryptoCanary.Properties.Settings.Default.workerName;
                checkBox2.Checked = true;
                button1.PerformClick();
            }
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length != 0 && textBox2.Text.Trim().Length != 0)
            {
                CryptoCanary.Properties.Settings.Default.userId = textBox2.Text.Trim();
                CryptoCanary.Properties.Settings.Default.workerName = textBox1.Text.Trim();
                CryptoCanary.Properties.Settings.Default.autoStart = true;
                CryptoCanary.Properties.Settings.Default.Save();
                checkBox2.Checked = true;
            }
            else
            {
                CryptoCanary.Properties.Settings.Default.userId = "";
                CryptoCanary.Properties.Settings.Default.workerName = "";
                CryptoCanary.Properties.Settings.Default.autoStart = false;
                CryptoCanary.Properties.Settings.Default.Save();
                checkBox2.Checked = false;
            }


        }
    }
}
