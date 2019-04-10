using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace MayDayButton
{
    using ps = Properties.Settings;
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(ps.Default.X, -350);
            backgroundWorker1.RunWorkerAsync();
        }

        private void label1_Click(object sender, EventArgs e)
        {
                this.Location = new Point(ps.Default.X, -50);
        }

        //Printer
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("Adobe"))
                process.Kill();
            cmd("net stop spooler");
            cmd(@"del %systemroot%\System32\spool\printers\* /Q");
            cmd("net start spooler");
            MessageBox.Show("Try it now");
        }

        //No internet
        private void button2_Click(object sender, EventArgs e)
        {
            cmd("netsh winsock reset");
            cmd("netsh int ip reset");
            cmd("ipconfig /release");
            cmd("ipconfig /flushdns");
            cmd("ipconfig /renew");
            MessageBox.Show("Try it now");
        }

        public void cmd(string Arguments, bool isHidden = true)
        {
            ProcessStartInfo ProcessInfo;
            Process Process;
            try
            {
                ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + Arguments);
                ProcessInfo.UseShellExecute = false;
                ProcessInfo.CreateNoWindow = isHidden;
                Process = Process.Start(ProcessInfo);
                Process.WaitForExit();
                Process.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Processing ExecuteCommand : " + e.Message);
            }           
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Checks if form is active, if not hides it
            Thread.Sleep(100); 
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Form.ActiveForm != this)
                this.Location = new Point(ps.Default.X, -350);
            backgroundWorker1.RunWorkerAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("Flowhub"))
                process.Kill();
            Process.Start(@"C:\Users\Joe\AppData\Local\FlowhubPos\Update.exe --processStart Flowhub.exe");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }
    }
}
