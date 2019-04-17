﻿using System;
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
using System.Net.Mail;
using Microsoft.Win32;
using System.Management;

namespace MayDayButton
{
    using ps = Properties.Settings;
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int Y = -351;
        public int nY = -526;

        private void Form1_Load(object sender, EventArgs e)
        {
            SetStartup();
            if (ps.Default.HighDPI)
                Y = nY;

            this.AutoScaleDimensions = new Size(96, 96);
            this.Location = new Point(ps.Default.X, Y);
            if (ps.Default.ShouldUpdate)
            {
                try{ UpdateEXE(); }
                catch {}
            }

            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
            backgroundWorker3.RunWorkerAsync();

            if (Process.GetProcessesByName("MayDayButton").Count() > 1)
                Application.Exit();
        }

        private void SetStartup()
        {
#if (!DEBUG)
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("MayDayButton");
                        if (o == null || o.ToString() != Application.ExecutablePath)
                        {
                                RegistryKey rk = Registry.LocalMachine.OpenSubKey
                                  ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                                rk.SetValue("MayDayButton", Application.ExecutablePath);
                        }
                    }
                }
            }
            catch {
                   ProcessStartInfo ProcessInfo;
                   Process Process;
                   ProcessInfo = new ProcessStartInfo(Application.ExecutablePath);
                   ProcessInfo.Verb = "runas";
                   Process = Process.Start(ProcessInfo);
                   Application.Exit();

             }
#endif
        }

        private void Checks()
        {
            string Result = "";
            //Check Battery %
            PowerStatus p = SystemInformation.PowerStatus;
            int a = (int)(p.BatteryLifePercent * 100);
            if (a < 50)
                Result += String.Format("Power is {0}, charger needs to be checked.\n", a);
            string temp = Program.CheckHealth();
            if (temp != "")
                Result += temp + "\n";

            if (Result != "")
                sendMessage(GetEmail[2], Result);
        }

        private void label1_Click(object sender, EventArgs e)
        {
                //this.Location = new Point(ps.Default.X, -50);
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
                ProcessInfo.Verb = "runas";
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
            if (ps.Default.HighDPI)
                Y = nY;
            if (Form.ActiveForm != this)
                this.Location = new Point(ps.Default.X, Y);
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
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FlowhubPos\Update.exe --processStart Flowhub.exe");
        }

        private void CloseAdobe()
        {
            AppendLog("Closed Adobe");
            foreach (var process in Process.GetProcessesByName("Adobe"))
                process.Kill();
        }

        private void UpdateEXE()
        {
            try
            {
                ps.Default.ShouldUpdate = false;
                ps.Default.Save();
                AppendLog("Updating");
                Directory.CreateDirectory(@"C:\MayDayButton\");
                string Root = @"C:\MayDayButton\";
                if (File.Exists(Root + "MayDayButton_Old.exe"))
                    File.Delete(Root + "MayDayButton_Old.exe");
                File.Move("MayDayButton.exe", Root + "MayDayButton_Old.exe");
                if (File.Exists(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\MayDayButton.exe"))
                    File.Delete(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\MayDayButton.exe");
                //This needs to be changed to where ever you're pulling your update from.
                //For me, I have a NAS system that I have visual studio set to push releases to so this exe will always be updated
                File.Copy(@"\\192.168.1.210\Server\MaydayButton\MayDayButton.exe", Root + "MayDayButton.exe");
                Process.Start(Root + "MayDayButton.exe");
                Application.Exit();
            }
            catch {
                ps.Default.ShouldUpdate = true;
                ps.Default.Save();
            }
        }

        public string Command;
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        { 
            Command = RecieveData();
        }

        //Check string recived by TCP server
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Command == "" || Command == null)
            {
                AppendLog("Command was sent via TCP");
                AppendLog(Command);
            }
            else if (Command.ToUpper().Contains("COMMAND $"))
            {
                string Temp = Command.Substring(9);
                cmd(Temp);
            }
            else if (Command.ToUpper().Contains("MSG $"))
            {
                string Temp = Command.Substring(5);
                MessageBox.Show(Temp);
            }
            else {
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
                        if (File.Exists(Log))
                            File.Delete(Log);
                        break;
                    case "CLEAN":
                        CleanSystem();
                        break;
                    default:
                        break;
                }
            }
            backgroundWorker2.RunWorkerAsync();
            Command = "";
        }
    #endregion

        private void CleanSystem()
        {

        }

        //Appends time and event to prove bitches wrong
        private void AppendLog(string Text)
        {
            if (File.Exists(Log))
            {
                DateTime logFileEnd = File.GetLastWriteTime(Log);
                if (logFileEnd.ToString("MM-dd-yyyy") != DateTime.Now.ToString("MM-dd-yyyy"))
                    File.Delete(Log);
            }
            try
            {
                string formatted = String.Format("[{0}]::{1}", DateTime.Now.ToString("MM/dd h:mm tt"), Text);
                Directory.CreateDirectory(@"C:\MayDayButton\");
                if (!File.Exists(Log))
                    using (StreamWriter sw = File.CreateText(Log))
                        sw.WriteLine(formatted);
                else
                    using (StreamWriter sw = File.AppendText(Log))
                        sw.WriteLine(formatted);

            }
            catch { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AppendLog("Opened Team Viewer");
            if (File.Exists(@"C:\Program Files (x86)\TeamViewer\TeamViewer.exe"))
                MessageBox.Show("TeamViewer isn't installed!");
            else if (Process.GetProcessesByName("Teamviewer").Count() < 1)
                Process.Start(@"C:\Program Files (x86)\TeamViewer\TeamViewer.exe");
            else
                MessageBox.Show("TeamViewer is already open!");
        }

        //Send message to tech, and if on a known computer, open slack direct chat with the tech.
        private void button6_Click(object sender, EventArgs e)
        {
            if (!File.Exists(@"C:\MayDayButton\Email.config"))
                File.Copy(@"\\192.168.1.210\Server\MaydayButton\Email.config", @"C:\MayDayButton\Email.config");
            sendMessage(GetEmail[2], "Help I broke something really bad!");
            string link = SlackLink();
            if (link != "NA")
            {
                MessageBox.Show("You will be contacted via Slack shortly.");
                Process.Start(link);
            }
            else
                MessageBox.Show("You will be contaced soon! Please wait at least 15 minutes before calling unless its an emergency!");
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
                case "192.168.1.161":
                    return "Test Environment";
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
        //Proves bitches wrong
        private string WhatIveTried()
        {
            if (File.Exists(Log) && File.ReadAllText(Log) != "")
                return "Here's what I've tried: \n" + File.ReadAllText(Log);
            else
                return "I've tried nothing";
        }

        //Sends message via gmail smtp
        //Can be used to send SMS via then recivers carrier email to sms converter.
        //ie sprint is Phonenumber@messaging.sprintpcs.com
        public void sendMessage(string To, string Message, string Subject = "", string Name = "Joe")
        {
            try
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
            catch(Exception e) {
                MessageBox.Show("There was an error sending an alert to your tech! Please try again!\n" + e.ToString());
            }
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        
        //Allows the form to be dragged to anywhere without having to go into options
        //Still allows the form to be clicked to be seen
        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                ps.Default.X = this.Location.X;
                ps.Default.Save();
                this.Location = new Point(ps.Default.X, -50);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppendLog("I did done did get closeded :(");
        }

          #region Long HDD Stuff
        public class HDD
        {

            public int Index { get; set; }
            public bool IsOK { get; set; }
            public string Model { get; set; }
            public string Type { get; set; }
            public string Serial { get; set; }
            public Dictionary<int, Smart> NormalAttributes = new Dictionary<int, Smart>() {
                {0x00, new Smart("Invalid")},
                {0x01, new Smart("Raw read error rate")},
                {0x02, new Smart("Throughput performance")},
                {0x03, new Smart("Spinup time")},
                {0x04, new Smart("Start/Stop count")},
                {0x05, new Smart("Reallocated sector count")},
                {0x06, new Smart("Read channel margin")},
                {0x07, new Smart("Seek error rate")},
                {0x08, new Smart("Seek timer performance")},
                {0x09, new Smart("Power-on hours count")},
                {0x0A, new Smart("Spinup retry count")},
                {0x0B, new Smart("Calibration retry count")},
                {0x0C, new Smart("Power cycle count")},
                {0x0D, new Smart("Soft read error rate")},
                {0xB8, new Smart("End-to-End error")},
                {0xBE, new Smart("Airflow Temperature")},
                {0xBF, new Smart("G-sense error rate")},
                {0xC0, new Smart("Power-off retract count")},
                {0xC1, new Smart("Load/Unload cycle count")},
                {0xC2, new Smart("HDD temperature")},
                {0xC3, new Smart("Hardware ECC recovered")},
                {0xC4, new Smart("Reallocation count")},
                {0xC5, new Smart("Current pending sector count")},
                {0xC6, new Smart("Offline scan uncorrectable count")},
                {0xC7, new Smart("UDMA CRC error rate")},
                {0xC8, new Smart("Write error rate")},
                {0xC9, new Smart("Soft read error rate")},
                {0xCA, new Smart("Data Address Mark errors")},
                {0xCB, new Smart("Run out cancel")},
                {0xCC, new Smart("Soft ECC correction")},
                {0xCD, new Smart("Thermal asperity rate (TAR)")},
                {0xCE, new Smart("Flying height")},
                {0xCF, new Smart("Spin high current")},
                {0xD0, new Smart("Spin buzz")},
                {0xD1, new Smart("Offline seek performance")},
                {0xDC, new Smart("Disk shift")},
                {0xDD, new Smart("G-sense error rate")},
                {0xDE, new Smart("Loaded hours")},
                {0xDF, new Smart("Load/unload retry count")},
                {0xE0, new Smart("Load friction")},
                {0xE1, new Smart("Load/Unload cycle count")},
                {0xE2, new Smart("Load-in time")},
                {0xE3, new Smart("Torque amplification count")},
                {0xE4, new Smart("Power-off retract count")},
                {0xE6, new Smart("GMR head amplitude")},
                {0xE7, new Smart("Temperature")},
                {0xF0, new Smart("Head flying hours")},
                {0xFA, new Smart("Read error retry rate")},
                /* slot in any new codes you find in here */
            };

            //Attributes for my register drives, which are Samsung MZMTE128 SSDs
            public Dictionary<int, Smart> Attributes = new Dictionary<int, Smart>() {
                {0x00, new Smart("Invalid")},
                {0x05, new Smart("Reallocated Sector Count")},
                {0x09, new Smart("Power-on Hours")},
                {0x0C, new Smart("Power-on Count")},
                {0xB1, new Smart("Wear leveling Count")},
                {0xB3, new Smart("Used Reserved Block Count (Total)")},
                {0xB5, new Smart("Program Fail Count (Total)")},
                {0xB6, new Smart("Erase Fail Count (Total)")},
                {0xB7, new Smart("Runtime Bad Block (Total)")},
                {0xBB, new Smart("Uncorrectable Error Count")},
                {0xBE, new Smart("Airflow Temperature")},
                {0xC3, new Smart("ECC Error Rate")},
                {0xC7, new Smart("CRC Error Rate")},
                {0xEB, new Smart("POR Recovery Count")},
                {0xF1, new Smart("Total LBAs Written")},
                {0xF2, new Smart("Total LBAs Read")},
            };
        }

        public class Smart
        {
            public bool HasData
            {
                get
                {
                    if (Current == 0 && Worst == 0 && Threshold == 0 && Data == 0)
                        return false;
                    return true;
                }
            }
            public string Attribute { get; set; }
            public int Current { get; set; }
            public int Worst { get; set; }
            public int Threshold { get; set; }
            public int Data { get; set; }
            public bool IsOK { get; set; }

            public Smart(){}

            public Smart(string attributeName)
            {
                this.Attribute = attributeName;
            }
        }

        /// <summary>
        /// Tested against Crystal Disk Info 5.3.1 and HD Tune Pro 3.5 on 15 Feb 2013.
        /// Findings; I do not trust the individual smart register "OK" status reported back frm the drives.
        /// I have tested faulty drives and they return an OK status on nearly all applications except HD Tune. 
        /// After further research I see HD Tune is checking specific attribute values against their thresholds
        /// and and making a determination of their own (which is good) for whether the disk is in good condition or not.
        /// I recommend whoever uses this code to do the same. For example -->
        /// "Reallocated sector count" - the general threshold is 36, but even if 1 sector is reallocated I want to know about it and it should be flagged.   
        /// </summary>
        public class Program
        {
            public static string CheckHealth()
            {
                try
                {

                    // retrieve list of drives on computer (this will return both HDD's and CDROM's and Virtual CDROM's)                    
                    var dicDrives = new Dictionary<int, HDD>();

                    var wdSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

                    // extract model and interface information
                    int iDriveIndex = 0;
                    foreach (ManagementObject drive in wdSearcher.Get())
                    {
                        var hdd = new HDD();
                        hdd.Model = drive["Model"].ToString().Trim();
                        hdd.Type = drive["InterfaceType"].ToString().Trim();
                        dicDrives.Add(iDriveIndex, hdd);
                        iDriveIndex++;
                    }

                    var pmsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

                    // retrieve hdd serial number
                    iDriveIndex = 0;
                    foreach (ManagementObject drive in pmsearcher.Get())
                    {
                        // because all physical media will be returned we need to exit
                        // after the hard drives serial info is extracted
                        if (iDriveIndex >= dicDrives.Count)
                            break;

                        dicDrives[iDriveIndex].Serial = drive["SerialNumber"] == null ? "None" : drive["SerialNumber"].ToString().Trim();
                        iDriveIndex++;
                    }

                    // get wmi access to hdd 
                    var searcher = new ManagementObjectSearcher("Select * from Win32_DiskDrive");
                    searcher.Scope = new ManagementScope(@"\root\wmi");

                    // check if SMART reports the drive is failing
                    searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictStatus");
                    iDriveIndex = 0;
                    foreach (ManagementObject drive in searcher.Get())
                    {
                        dicDrives[iDriveIndex].IsOK = (bool)drive.Properties["PredictFailure"].Value == false;
                        iDriveIndex++;
                    }

                    // retrive attribute flags, value worste and vendor data information
                    searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictData");
                    iDriveIndex = 0;
                    foreach (ManagementObject data in searcher.Get())
                    {
                        Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                        for (int i = 0; i < 30; ++i)
                        {
                            try
                            {
                                int id = bytes[i * 12 + 2];

                                int flags = bytes[i * 12 + 4]; // least significant status byte, +3 most significant byte, but not used so ignored.
                                                               //bool advisory = (flags & 0x1) == 0x0;
                                bool failureImminent = (flags & 0x1) == 0x1;
                                //bool onlineDataCollection = (flags & 0x2) == 0x2;

                                int value = bytes[i * 12 + 5];
                                int worst = bytes[i * 12 + 6];
                                int vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                                if (id == 0) continue;

                                var attr = dicDrives[iDriveIndex].Attributes[id];
                                attr.Current = value;
                                attr.Worst = worst;
                                attr.Data = vendordata;
                                attr.IsOK = failureImminent == false;
                            }
                            catch
                            {
                                // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                            }
                        }
                        iDriveIndex++;
                    }

                    // retreive threshold values foreach attribute
                    searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictThresholds");
                    iDriveIndex = 0;
                    foreach (ManagementObject data in searcher.Get())
                    {
                        Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                        for (int i = 0; i < 30; ++i)
                        {
                            try
                            {

                                int id = bytes[i * 12 + 2];
                                int thresh = bytes[i * 12 + 3];
                                if (id == 0) continue;

                                var attr = dicDrives[iDriveIndex].Attributes[id];
                                attr.Threshold = thresh;
                            }
                            catch
                            {
                                // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                            }
                        }

                        iDriveIndex++;
                    }

                    foreach (var drive in dicDrives)
                    {
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine(" DRIVE ({0}): " + drive.Value.Serial + " - " + drive.Value.Model + " - " + drive.Value.Type, ((drive.Value.IsOK) ? "OK" : "BAD"));
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine("");

                        Console.WriteLine("ID                   Current  Worst  Threshold  Data  Status");
                        foreach (var attr in drive.Value.Attributes)
                        {
                            if (attr.Value.HasData)
                                Console.WriteLine("{0}\t {1}\t {2}\t {3}\t " + attr.Value.Data + " " + ((attr.Value.IsOK) ? "OK" : ""), attr.Value.Attribute, attr.Value.Current, attr.Value.Worst, attr.Value.Threshold);
                        }
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine();
                        if (!drive.Value.IsOK)
                            return "Drive is possibly failing needs to be checked!";
                    }
                }
                catch (ManagementException e)
                {
                    Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
                }
                return "";
            }

        }
    #endregion

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            //Waits 2 hours and then runs battery and HDD check on the device.
            //If things look abnormal message will be sent to Tech.
            Thread.Sleep(12000000);
        }

        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Checks();
            backgroundWorker3.RunWorkerAsync();
        }
    }
}
