using SimpleWifi;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
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

        MD_Core MD = new MD_Core();

        public static string ServerLocation = @"\\192.168.1.210\server\MayDayButton\";

        public int Y = ps.Default.Y_Norm;
        public int nY = ps.Default.Y_Adjustment;
        public int X = ps.Default.X;

        public int Label_Height { get; set; }

        //Used to allow admin commands from TCP, must be manually setup
        private string SecretPhrase
          => File.ReadAllText(ServerLocation + @"SecretPhrase.txt");
        //ID for Balloon notifications
        private const string APP_ID = "MayDayButton";

        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            AppendLog(e.Exception.Message);
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppendLog((e.ExceptionObject as Exception).Message);
        }
        
        //Checks if MayDayButton is being used by a valid computer with a valid license.
        //For those of you getting it from Github simply remove this and ignore it.
        //I couldn't really care less.
        private void selfDestructPreGame() {
            DateTime thisDate = DateTime.Today;
            bool shouldDestruct = false;
            string licenseFile = ServerLocation + "License.Config";
            string license = (File.Exists(licenseFile) ? File.ReadAllText(licenseFile) : "WaitThisAintNoLicense");
            if (license != "HeyThisShouldBeAndADvancedLicenseCheckButTheDevIsALazyPOSWhoDoesntCareBacuseThisCompanyIsAJoke")
            {

                string licenseFile2 = ServerLocation + "\\LicenseCheck.Config";
                if (!File.Exists(licenseFile2))
                {
                    File.WriteAllText(licenseFile2, thisDate.ToString());
                    shouldDestruct = false;
                }
                else if (File.Exists(licenseFile2))
                {
                    DateTime oldDate = DateTime.Parse(File.ReadAllText(licenseFile2));
                    if (DateTime.Compare(oldDate.AddDays(7), thisDate) <= 0)
                        shouldDestruct = true;
                    else
                        shouldDestruct = false;
                }

                if (shouldDestruct)
                    selfDestruct();
            }
        }

        private void selfDestruct()
        {
            if (ServerLocation == @"\\192.168.1.210\server\MayDayButton\")
            {
                MessageBox.Show("This software has been unlicensed for seven days and will now disable itself. Please contact the creator to purchase a license. If you have questions about the licensing agreement please refer to the Admin Panel for the complete terms.");
#if !DEBUG
            MD.setVayCay();
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button6.Enabled = false;
            label3.Visible = false;
            label1.Text = "Unlicensed Software";
#endif
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                selfDestructPreGame();
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                Process CurrentProcess = Process.GetCurrentProcess();
                foreach (Process p in Process.GetProcesses().Where(n => n.Id != CurrentProcess.Id))
                    if (p.ProcessName == "MayDayButton")
                        p.Kill();
#if DEBUG
                //This is done to make sure that upon running on boot, the location doesn't get jumped to 
                //C:\System32 or whatever bullshit Windows likes to do.
                if (Directory.GetCurrentDirectory() == ServerLocation)
                    Process.Start(@"C:\MayDayButton\MayDayButton.exe");
                else
                    Directory.SetCurrentDirectory(@"C:\MayDayButton\");
#endif
                MD.RestoreConnection();
                if (!MD.IsAdministrator && ps.Default.AdminStart)
                    MD.GetAdmin();

                MD.SetStartup();
                MD.AddShortcut();
                MD.setRebootTask();

                //Manual Update or checks for needed updates from your server
#if !DEBUG
                if (ps.Default.ShouldUpdate)
                    MD.UpdateEXE();
                else
                {
                    try
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(ServerLocation + "MayDayButton.exe");
                        string version = versionInfo.ProductVersion.Replace(".", "");
                        if (Int32.Parse(version) > Int32.Parse(Application.ProductVersion.Replace(".", "")))
                            MD.UpdateEXE();
                    }
                    catch { }
                }
#endif

                //Starts background processes, for monitoring battery, checking for commands, and the like
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.RunWorkerAsync();
                backgroundWorker3.RunWorkerAsync();
                backgroundWorker4.RunWorkerAsync();
                Check_420.RunWorkerAsync();

                if (!File.Exists(@"C:\MayDayButton\toast.png"))
                    File.Copy(ServerLocation + @"toast.png", @"C:\MayDayButton\toast.png");

                MD.importSettings("Settings.Config", true);
                populateInfo();

                if (ps.Default.HighDPI)
                    Y = nY;

                this.AutoScaleDimensions = new Size(96, 96);
                this.Location = new Point(X, Y);
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        //Populates Computer info such as Name, IP Address and connected network and displays it on the main form
        private void populateInfo()
        {
            string Name = System.Environment.MachineName;
            string Version = Application.ProductVersion;
            string IP = MD.GetLocalIPAddress();
            string SSID = MD.showConnectedId()[0];
            label3.Text = String.Format("PC: {0}\nSSID: {3} \nIP: {2} \nv{1}", Name, Version, IP, SSID);
        }

        //Printer Button
        private void button1_Click(object sender, EventArgs e)
        {
            string zebraError = MD.zebraStatuses();
            if (zebraError != "") {
                MessageBox.Show("Errors found for your Zebra Printer(Label Printer). Please correct the following error before trying again:\n---------------\n" + zebraError);
            }
            else
            {
                bool wasOutofPaper = MD.FixPrinters();
                if (wasOutofPaper)
                    MessageBox.Show("It looks like your printer is either out of paper or was out of paper recently. Please double check your paper before proceeding. If you still cannot print from Flowhub, switch to a different transaction then switch back and try to print again. This is a bug in Flowhub.");
                else
                {
                    NotiMsg("Please select the printer that was having issues and in order to print a test.");
                    MD.testPrinter();
                    MessageBox.Show("Try it now. If issues persist, disconnect your printer for 20 seconds and reconnect it!");
                }
            }
        }

        //Internet Button
        //Checks if connected to Wifi before running any fixes, and attempts to notify the user
        public static Wifi wifi;
        private void button2_Click(object sender, EventArgs e)
        {
            wifi = new Wifi();
            if (wifi.NoWifiAvailable)
                MessageBox.Show("I am unable to detect a Wifi adapter in this device... Please contact your tech for more support.");
            else
            {
                if (wifi.ConnectionStatus == WifiStatus.Connected)
                    MD.FixInternet();
                else
                {
                    MD.Connect("FX420");
                    MessageBox.Show("Your wifi network was not connected. Please verify that you are connected to the network before attempting to run troubleshooting methods or before asking for additional support.");
                }
            }
            populateInfo();
        }

        //Flowhub Button
        private void button3_Click(object sender, EventArgs e)
        {
            MD.Close("Flowhub");
            MD.cleanInternetTemp();
            MD.RestartFlowhub();
            if (!MD.Ping("flowhub.co"))
                MessageBox.Show("MayDayButton is unable to communicate with 'Flowhub.co'. This could mean either you aren't connected to the internet or Flowhub is currently experiancing issues. Please Run the internet fix and try again.");
        }

        //Show Options Screen (Form2)
        private void button4_Click(object sender, EventArgs e)
        { 
            Form2 frm = new Form2();
            frm.Label_Height = label1.Height;
            frm.FormClosing += new FormClosingEventHandler(onClose);
            frm.Show();
        }

        private void onClose(object sender, FormClosingEventArgs e)
        {
            //this.AutoScaleDimensions = new Size(96, 96);
            //this.Location = new Point(X, Y);
        }

        //TODO Clean this mess up
#region Handler and background worker for TCP Commands
        public string Command;
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        { 
            Command = MD.RecieveData();
            if (Command == "" || Command == null)
            {
                AppendLog("Command was sent via TCP");
                AppendLog(Command);
            }
            else if (Command.ToUpper().Contains("COMMAND $"))
            {
                //In order for the tech to not need to have the user hit OK for every command adding $ then the secret phrase to the sent command will bypass the check. This phrase should not be a password
                //This phrase cannot be used to activate an admin command, unless set in the source to do so.
                if (File.Exists(ServerLocation + @"SecretPhrase.txt") && Command.Contains("$" + SecretPhrase))
                {
                    string Temp = Command.Substring(9).Replace("$" + SecretPhrase, "").Replace("$Admin", "");
                    MD.cmd(Temp, false, false);
                }
                else
                {
                    string Temp = Command.Substring(9);
                    DialogResult dialogResult = MessageBox.Show(String.Format("This device recieved the command \n-'{0}'-\nIf this was done by your tech please press Yes. Otherwise press no. Unsure? Ask your tech before proceeding.", Temp), "Command was recived, but not yet handeled", MessageBoxButtons.YesNo); ;
                    if (dialogResult == DialogResult.Yes)
                    {
                        if (Temp.Contains("$Admin"))
                            MD.cmd(Temp.Replace("$Admin", ""), false, true);
                        else
                            MD.cmd(Temp);
                    }
                }
            }
            else if (Command.ToUpper().Contains("MSG $"))
            {
                if (Command.ToUpper().Contains("BMSG $"))
                {
                    string Temp = Command.Substring(6);
                    NotiMsg(Temp);
                }
                else
                {
                    string Temp = Command.Substring(5);
                    //MessageBox.Show(Temp);
                    Message = Temp;
                    backgroundWorker2.ReportProgress(99);
                }
            }
            else if (Command.ToUpper().Contains("WALLPAPER"))
            {
                string file_URL = Command.Substring(10);
                string destination = @"C:\MayDayButton\Wallpaper.png";
                File.Delete(destination);
                using (var client = new WebClient())
                    client.DownloadFile(file_URL, destination);
                MD.DisplayPicture(destination, false);
            }
            else
            {
                switch (Command.ToUpper())
                {
                    case "ADOBE":
                    case "A":
                        MD.CloseAdobe();
                        break;
                    case "FLOWHUB":
                        MD.RestartFlowhub();
                        break;
                    case "PRINTER":
                    case "PRINTERS":
                        MD.FixPrinters();
                        break;
                    case "INTERNET":
                        MD.FixInternet();
                        break;
                    case "UPDATE":
                        MD.UpdateEXE();
                        break;
                    case "CLOSE":
                        Application.Exit();
                        break;
                    case "RESTART":
                        Application.Restart();
                        break;
                    case "FUCKOFF":
                        MD.cmd("shutdown /h", true, false);
                        break;
                    case "DLOG":
                        if (File.Exists(Log))
                            File.Delete(Log);
                        break;
                    case "CHECK":
                        MD.Checks();
                        break;
                    case "IMPORT":
                        MD.importSettings(ServerLocation + "\\Settings.Config", false);
                        break;
                    case "DEL":
                        MD.deletePrinters();
                        break;
                    default:
                        break;
                }
            }
            Command = "";
        }

        public string Message = "";
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(300);
            backgroundWorker2.RunWorkerAsync();
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //These functions kinda got depreciated, but I dont remember why
            //ToastNoti("Notice!", Message);
            //StartTimer(30);
            //label1.Text = Message;
            //BalloonNotification("MayDayButton", Message);
        }
#endregion


#region Log and messaging functions
        //Appends time and event to prove bitches wrong
        public void AppendLog(string Text)
        {
            if (File.Exists(Log))
            {
                DateTime logFileEnd = File.GetLastWriteTime(Log);
                if (logFileEnd.ToString("MM-dd-yyyy") != DateTime.Now.ToString("MM-dd-yyyy"))
                    File.Move(Log, @"C:\MayDayButton\Log_" + logFileEnd.ToString("MM-dd-yyyy") + ".txt");
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

        //Send message to tech, and if on a known computer, open slack direct chat with the tech.
        private void button6_Click(object sender, EventArgs e)
        {
             MD.RestoreConnection();
            if (!isOnVayCay)
            {
                ps.Default.Tried2Contact = 0;
                ps.Default.Save();
                if (!File.Exists(@"C:\MayDayButton\Email.config"))
                    File.Copy(ServerLocation + @"Email.config", @"C:\MayDayButton\Email.config");
                sendMessage(GetEmail[2], "Help I broke something really bad!");
                try
                {
                    string link = SlackLink();
                    if (link != "NA")
                    {
                        MessageBox.Show("You will be contacted via Slack shortly. Please make sure to send your name and the issue at hand.");
                        Process.Start(link);
                    }
                    else
                        MessageBox.Show("You will be contaced soon! Please wait at least 15 minutes before calling unless its an emergency!");
                }catch {
                    MessageBox.Show("You will be contaced soon! Please wait at least 15 minutes before calling unless its an emergency!");
                }
            }
            //Runs when TechVaction[false] is renamed to [true]
            //Probably better ways to do this but whatever
            else
            {
                ps.Default.Tried2Contact++;
                ps.Default.Save();
                string promptValue = ShowDialog("Notice", "Joe is on vacation and will only be responding to important issues. Please note your name here and then contact Jessica if there is an issue/");
                if (promptValue != null)
                    AppendLog(promptValue + " tried to contact the tech!");
                else
                    AppendLog("Someone tried to contact the tech, no name was given.");
               
                MessageBox.Show(String.Format(
                            "It looks like your tech has marked himself as being on vacation! Go see your on site manager for more assistance! But dont contact your tech or he will get ur IP addres an brick your xbox u noob scrub.\n\n" +
                            "It looks like on this Register alone you've tried to contact your tech {0} times while he was on vacation. {1}", ps.Default.Tried2Contact, ps.Default.Tried2Contact > 2 ? "That's not very nice. He just wants to enjoy himself away from work for one week." : ""), "Oh No!");
            }
        }

        public static string ShowDialog(string caption, string text)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                AutoScaleMode = AutoScaleMode.Dpi,
                AutoSize = true
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            textLabel.AutoSize = true;
            TextBox textBox = new TextBox() { Left = 50, Top = 60, Width = 400 };
            textBox.UseSystemPasswordChar = true;
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private bool isOnVayCay
            => File.Exists(ServerLocation + @"TechVacation[TRUE].txt");

        public string[] GetEmail
            => File.ReadAllLines(@"C:\MayDayButton\Email.config");

        //TODO have one single Slack Link for all registers
        private string SlackLink()
        {
            string[] SlackLinks = File.ReadAllLines(ServerLocation + @"Slack.config");
            switch (MD.GetLocalIPAddress())
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
            if (File.Exists(Log))
            {
                DateTime logFileEnd = File.GetLastWriteTime(Log);
                if (logFileEnd.ToString("MM-dd-yyyy") != DateTime.Now.ToString("MM-dd-yyyy"))
                    File.Move(Log, @"C:\MayDayButton\Log_" + logFileEnd.ToString("MM-dd-yyyy") + ".txt");
            }
              
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
                string Message2 = String.Format("PC: {0}\nSSID: {3} || {2}\nv{1}", System.Environment.MachineName, Application.ProductVersion, MD.GetLocalIPAddress(), MD.showConnectedId()[0]);
                string body = String.Format("{0}\n---------------\n{1}", Message, Message2);

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

        private void BalloonNotification(string Text, string Title = "Hey Listen!")
        {
            notifyIcon1.BalloonTipTitle = Title;
            notifyIcon1.BalloonTipText = Text;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(18000);
            notifyIcon1.Visible = false;
        }

        private System.Timers.Timer timer = new System.Timers.Timer();

        public void StartTimer(int Minutes)
        {
            try { timer.Stop(); }
            catch { }
            timer = new System.Timers.Timer(60000 * Minutes);
            //timer = new System.Timers.Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(OnElapsed);
            timer.AutoReset = false;
            timer.Start();
        }

        public void OnElapsed(object sender, ElapsedEventArgs e)
        {
            Message = "MAYDAY";
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { OnElapsed(sender, e); });
                return;
            }
            label1.Text = Message;
        }

        public void NotiMsg(string Message)
        {
            Console.WriteLine("NotiMSG");
            if (InvokeRequired)
            {
                Console.WriteLine("Invoking");
                Invoke((MethodInvoker)delegate { NotiMsg(Message); });
                return;
            }
            BalloonNotification(Message);
        }
#endregion

        //Allows the form to be dragged to anywhere without having to go into options
        //Still allows the form to be clicked to be seen
#region Mouse Controls
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                int x = this.Location.X;
                if (x < 0) x = 0;
                int MaxX = Screen.PrimaryScreen.WorkingArea.Width;
                if (x > MaxX - this.Width) x = MaxX - this.Width;
                ps.Default.X = x;
                ps.Default.Save();
                this.Location = new Point(x, -50);
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
                Y = ps.Default.Y_Adjustment;
            else
                Y = ps.Default.Y_Norm;
            if (Form.ActiveForm != this)
                this.Location = new Point(ps.Default.X, Y);
            backgroundWorker1.RunWorkerAsync();
        }
#endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //AppendLog("I did done did get closeded :(");
        }

#region System Checks related to Power, HDD, and the like
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            //Waits 2 hours and then runs battery and HDD check on the device.
            Thread.Sleep(12000000);
        }
        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MD.Checks();
            backgroundWorker3.RunWorkerAsync();
        }

        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(30000);
        }
        private void backgroundWorker4_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PowerLineStatus status = SystemInformation.PowerStatus.PowerLineStatus;
            if (status == PowerLineStatus.Offline)
            {
                BalloonNotification("Why am I unplugged? Do you want me to die?", "What a sad day");
            }
            backgroundWorker4.RunWorkerAsync();
        }
        #endregion

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!ps.Default.HighDPI)
            {
                Label_Height = label1.Height;
                Console.WriteLine(Label_Height);
                int Y = ps.Default.HighDPI ? ps.Default.Y_Adjustment : ps.Default.Y_Norm;
                int MinY = (this.Height * -1) + Label_Height;
                Console.WriteLine(MinY);
                if (Y <= MinY)
                {
                    ps.Default.Y_Norm = MinY;
                }
                ps.Default.Save();
                Console.WriteLine(ps.Default.Y_Norm);
            }
        }

        private void Check_420_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
        }

        private void Check_420_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TimeSpan ts = DateTime.Now.TimeOfDay;
            if ((ts >= TimeSpan.Parse("04:20:00") && ts < TimeSpan.Parse("04:21:00"))
                || (ts >= TimeSpan.Parse("16:20:00") && ts < TimeSpan.Parse("16:21:00")))
            {
                NotiMsg("420 Blaze it");
                Thread.Sleep(60000);
            }
            Check_420.RunWorkerAsync();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(MD.statusReport());
        }
    }
}
