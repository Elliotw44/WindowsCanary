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
        static public bool running = false;
        public form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length != 0 && textBox2.Text.Trim().Length != 0 && !running)
            {
                string user_worker = textBox2.Text.Trim().ToLower() + ":" + textBox1.Text.Trim().ToLower();
                WorkerUpdate workerUpdate = new WorkerUpdate(); ;
                workerUpdate.update(user_worker);
                this.button1.Text = "Monitoring";
                running = true;
                this.progressBar1.Style = ProgressBarStyle.Marquee;
                this.progressBar1.MarqueeAnimationSpeed = 30;
            }
            else
            {
                textBox1.Text = "Enter the worker name from the website here please";
                textBox2.Text = "Enter your username from the website here please";
            }
        }
    }
}
