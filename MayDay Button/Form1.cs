using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;

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
            if (ps.Default.ShouldUpdate)
            {
                try{
                    if (File.Exists(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MayDayButton.exe"))
                        File.Delete(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MayDayButton.exe");
                    File.Copy(@"\\192.168.1.210\Server\MaydayButton\MayDayButton.exe", @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MayDayButton.exe");
                    ps.Default.ShouldUpdate = false;
                    ps.Default.Save();
                    Process.Start(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MayDayButton.exe");
                    Application.Exit();
                }
                catch {
                    backgroundWorker1.RunWorkerAsync();
                    backgroundWorker2.RunWorkerAsync();
                }
            }
            else
            {
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.RunWorkerAsync();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
                this.Location = new Point(ps.Default.X, -50);
        }

        //Printer
        private void button1_Click(object sender, EventArgs e)
        {
            FixPrinters();
        }

        //No internet
        private void button2_Click(object sender, EventArgs e)
        {
            FixInternet();
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
            RestartFlowhub();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }

        #region TCP Commands
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private string RecieveData()
        {
            string Result = "";
            try
            {
                IPAddress ipAd = IPAddress.Parse(GetLocalIPAddress()); //use local m/c IP address, and use the same in the client
                TcpListener myList = new TcpListener(ipAd, 8001);
                myList.Start();

                Console.WriteLine("The server is running at port 8001...");
                Console.WriteLine("The local End point is  :" + myList.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");

                Socket s = myList.AcceptSocket();
                Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                byte[] b = new byte[100];
                int k = s.Receive(b);
                Console.WriteLine("Recieved...");

                for (int i = 0; i < k; i++)
                {
                    Console.Write(Convert.ToChar(b[i]));
                    Result = Result + Convert.ToChar(b[i]);
                }

                ASCIIEncoding asen = new ASCIIEncoding();
                s.Send(asen.GetBytes("The string was recieved by the server."));
                Console.WriteLine("\nSent Acknowledgement");
                /* clean up */
                s.Close();
                myList.Stop();
               
            }
            catch (Exception e)
            {
                Result = "Error";
                Console.WriteLine("Error..... " + e.StackTrace);
            }
            return Result;
        }

        private void FixPrinters()
        {
            AppendLog("Printer fix");
            foreach (var process in Process.GetProcessesByName("Adobe"))
                process.Kill();
            cmd(@"net stop spooler & del %systemroot%\System32\spool\printers\* /Q");
            //cmd(@"del %systemroot%\System32\spool\printers\* /Q");
            cmd("net start spooler");
            MessageBox.Show("Try it now");
        }

        private void FixInternet()
        {
            AppendLog("Internet flush");
            cmd("netsh winsock reset");
            cmd("netsh int ip reset");
            cmd("ipconfig /release");
            cmd("ipconfig /flushdns");
            cmd("ipconfig /renew");
            MessageBox.Show("Try it now");
        }

        private void RestartFlowhub()
        {
            AppendLog("Restarted Flowhub");
            foreach (var process in Process.GetProcessesByName("Flowhub"))
                process.Kill();
            Process.Start(@"C:\Users\Joe\AppData\Local\FlowhubPos\Update.exe --processStart Flowhub.exe");
        }

        private void CloseAdobe()
        {
            AppendLog("Closed Adobe");
            foreach (var process in Process.GetProcessesByName("Adobe"))
                process.Kill();
        }

        private void UpdateEXE()
        {
            AppendLog("Updating");
            Directory.CreateDirectory(@"C:\MayDayButton\");
            //Delete Old Files
            if (File.Exists(@"C:\MayDayButton\MayDay_Old.exe"))
                File.Delete(@"C:\MayDayButton\MayDay_Old.exe");
            Directory.CreateDirectory(@"C:\MayDayButton\");
            if (File.Exists(@"C:\MayDayButton\MayDayButton.exe"))
                File.Delete(@"C:\MayDayButton\MayDayButton.exe");

            //Move Current Program
            try { File.Move("MayDayButton.exe", @"C:\MayDayButton\MayDay_Old.exe"); } catch { }

            //Move File from server to Startup folder and start it
            File.Copy(@"\\192.168.1.210\Server\MaydayButton\MayDayButton.exe", @"C:\MayDayButton\MayDayButton.exe");
            Process.Start(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MayDayButton.exe");
            Application.Exit();

            /*
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileAsync(
                    // Param1 = Link of file
                    new System.Uri("http://192.168.1.210/MaydayButton/MayDayButton.exe"),
                    // Param2 = Path to save
                    @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MayDayButton.exe"
                );
            }
            */
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
            if(e.ProgressPercentage == 100)
            {
                Process.Start(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MayDayButton.exe");
                this.Close();
            }
        }

        public string Command;
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            Command = RecieveData();
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(Command != "" || Command != null)
                AppendLog("Opened Team Viewer");
            backgroundWorker2.RunWorkerAsync();
            switch (Command.ToUpper())
            {
                case "ADOBE":
                    CloseAdobe();
                    break;
                case "FLOWHUB":
                    RestartFlowhub();
                    break;
                case "PRINTER":
                    FixPrinters();
                    break;
                case "PRINTERS":
                    FixPrinters();
                    break;
                case "INTERNET":
                    FixInternet();
                    break;
                case "UPDATE":
                    UpdateEXE();
                    break;
                case "CLOSE":
                    Application.Exit();
                    break;
                case "RESTART":
                    Application.Restart();
                    break;
                case "DLOG":
                    if(File.Exists(Log))
                        File.Delete(Log);
                    break;
                default:
                    break;
            }
            if (Command.ToUpper().Contains("COMMAND $"))
            {
                string Temp = Command.Substring(9);
                cmd(Temp);
            }
            Command = "";
        }
        #endregion

        private void AppendLog(string Text)
        {
            string formatted = String.Format("[{0}]::{1}", DateTime.Now.ToString("MM/dd h:mm tt"), Text);
            if (!File.Exists(Log))
                using (StreamWriter sw = File.CreateText(Log))
                {
                    sw.WriteLine(formatted);
                }
            else
                using (StreamWriter sw = File.AppendText(Log))
                {
                    sw.WriteLine(formatted);
                }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AppendLog("Opened Team Viewer");
            if (Process.GetProcessesByName("Teamviewer").Count() < 1 && File.Exists(@"C:\Program Files (x86)\TeamViewer\TeamViewer.exe"))
                Process.Start(@"C:\Program Files (x86)\TeamViewer\TeamViewer.exe");
            else
                MessageBox.Show("TeamViewer is already open!");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            sendMessage(GetEmail[2], "Help I broke something really bad!");
            string link = SlackLink();
            if (link != "NA")
            {
                MessageBox.Show("You will be contacted via Slack shortly.");
                Process.Start(link); 
            }
        }

        private string[] GetEmail
            => File.ReadAllLines(@"C:\MayDayButton\Email.config");
        
        private string WhatComputer()
        {
            switch(GetLocalIPAddress())
            {
                case "192.168.1.158":
                    return "Express Register";
                case "192.168.1.203":
                    return "Register 2";
                case "192.168.1.151":
                    return "Register 1";
                case "192.168.1.89":
                    return "Label Printer";
                case "192.168.1.138":
                    return "Jessica's Computer";
                default:
                    return "NA";      
            }
        }

        private string SlackLink()
        {
            string[] SlackLinks = File.ReadAllLines(@"\\192.168.1.210\Server\MaydayButton\Slack.config");
            switch (GetLocalIPAddress())
            {
                case "192.168.1.158":
                    return SlackLinks[0];
                case "192.168.1.203":
                    return SlackLinks[1];
                case "192.168.1.151":
                    return SlackLinks[2];
                default:
                    return "NA";
            }
        }

        const string Log = @"C:\MayDayButton\Log.txt";
        private string WhatIveTried()
        {
            if (File.Exists(Log) && File.ReadAllText(Log) != "")
                return "Here's what I've tried: \n" + File.ReadAllText(Log);
            else
                return "I've tried nothing";
        }

        public void sendMessage(string To, string Message, string Subject = "", string Name = "Joe")
        {
            var fromAddress = new MailAddress(GetEmail[0], "MayDay Button");
            var toAddress = new MailAddress(To, Name);
            string fromPassword = GetEmail[1];
            string subject = Subject;
            string body = String.Format("{0}\n{1}\n{2}", Message, WhatComputer(), WhatIveTried());

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
