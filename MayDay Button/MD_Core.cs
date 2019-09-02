using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Net.NetworkInformation;
using SimpleWifi;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using System.Media;
using System.Security.Principal;
using IWshRuntimeLibrary;


namespace MayDayButton
{
    using File = System.IO.File;
    using ps = Properties.Settings;
    public class MD_Core
    {
        #region WebMD
        //Set this to your network interface name, to specifically enable/disable when fixing internet, or leave empty to do a reset to all adapters
        string Interface = "";
        public static Wifi wifi;

        public string[] showConnectedId()
        {
            string s1, s2;
            try
            {
                Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "wlan show interfaces";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();

                string s = p.StandardOutput.ReadToEnd();
                s1 = s.Substring(s.IndexOf("SSID"));
                s1 = s1.Substring(s1.IndexOf(":"));
                s1 = s1.Substring(2, s1.IndexOf("\n")).Trim();

                s2 = s.Substring(s.IndexOf("Signal"));
                s2 = s2.Substring(s2.IndexOf(":"));
                s2 = s2.Substring(2, s2.IndexOf("\n")).Trim();
                p.WaitForExit();
            }
            catch { s1 = "NA"; s2 = "NA"; }
            return new string[] { s1, s2 };
        }

        //This requires that the connecting network already has a saved profile and has the correct password. 
        //I'm doing it this way in order to prevent the button to be transfered to unwanted devices
        //And then allows said devices to connect to our register network all willy nilly.
        //While theoretically this doesn't matter much, as our network is IP and Mac whitelisted only, 
        //these things can easily be spoofed by someone who really really wants to see all the memes on my NAS drive
        public void Connect(string SSID)
        {
            try
            {
                AccessPoint selectedAP = null;
                IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);
                foreach (AccessPoint ap in accessPoints)
                    if (ap.Name == SSID)
                        selectedAP = ap;
                AuthRequest authRequest = new AuthRequest(selectedAP);
                selectedAP.ConnectAsync(authRequest);
            }
            catch{}
        }

        public string GetLocalIPAddress()
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

        public string RecieveData()
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

        public void FixInternet()
        {
            if (Ping("google.com") && Ping("flowhub.co"))
                MessageBox.Show("It looks like your internet is currently working just fine! If Flowhub is having some issues try restarting Flowhub!");
            else if (Ping("google.com") && !Ping("flowhub.co"))
                MessageBox.Show("It looks like your internet is currently working just fine! But Flowhub is unable to be pinged. It's a Flowhub issue, there's nothing anyone can do.");
            else
            {
                IPReset();

                if (!Ping("google.com") && !Ping("flowhub.co"))
                    AdapterReset();

                if (!Ping("google.com") && !Ping("flowhub.co"))
                {
                    DialogResult dialogResult = MessageBox.Show("The program is unable to connect to Google or Flowhub and will restart the whole register if you press YES, if you can access the internet and do NOT want the register to restart, please press NO.", "WARNING", MessageBoxButtons.YesNo); ;
                    if (dialogResult == DialogResult.Yes)
                        cmd("shutdown /r", false, true);
                }
                else
                {
                    var frm1 = new Form1();
                    frm1.NotiMsg("Try it now!");
                }
            }
        }

        public void IPReset()
        {
            var frm1 = new Form1();
            frm1.AppendLog("Internet flush");
            cmd("netsh winsock reset & " +
                "netsh int ip reset & " +
                "ipconfig /release & " +
                "ipconfig /flushdns & " +
                "ipconfig /renew", false);
        }

        public void AdapterReset()
        {
            try
            {
                var frm1 = new Form1();
                frm1.AppendLog("Adapter Reset");
                if (Interface != "")
                {
                    cmd("netsh interface set interface\"" + Interface + "\" disable", true, true);
                    cmd("netsh interface set interface\"" + Interface + "\" enable", true, true);
                }
                else
                {
                    SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
                    ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
                    foreach (ManagementObject item in searchProcedure.Get())
                    {
                        frm1.NotiMsg((string)item["NetConnectionId"]);
                        cmd("netsh interface set interface \"" + (string)item["NetConnectionId"] + "\" disable & netsh interface set interface \"" + (string)item["NetConnectionId"] + "\" enable", true, true);
                    }
                }
            }
            catch(Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        public bool Ping(string IP)
        {
            bool pingable = false;
            Ping pinger = null;
            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(IP);
                pingable = reply.Status == IPStatus.Success;
            }
            catch { }
            finally { if (pinger != null) { pinger.Dispose(); } }

            return pingable;
        }

        public void RestoreConnection()
        {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = Form1.ServerLocation;
            p.Start();
            p.WaitForInputIdle(100);
            p.Kill();
            p.Dispose();
        }

        public string DisplayUpNetworkConnectionsInfo()
        {
            string Results = "";
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters.Where(a => a.OperationalStatus == OperationalStatus.Up))
            {
                Results += String.Format("\nDescription: {0} \nId: {1} \nIsReceiveOnly: {2} \nName: {3} \nNetworkInterfaceType: {4} " +
                    "\nOperationalStatus: {5} " +
                    "\nSpeed (bits per second): {6} " +
                    "\nSpeed (kilobits per second): {7} " +
                    "\nSpeed (megabits per second): {8} " +
                    "\nSpeed (gigabits per second): {9} " +
                    "\nSupportsMulticast: {10}",
                    adapter.Description,
                    adapter.Id,
                    adapter.IsReceiveOnly,
                    adapter.Name,
                    adapter.NetworkInterfaceType,
                    adapter.OperationalStatus,
                    adapter.Speed,
                    adapter.Speed / 1000,
                    adapter.Speed / 1000 / 1000,
                    adapter.Speed / 1000 / 1000 / 1000,
                    adapter.SupportsMulticast);

                var ipv4Info = adapter.GetIPv4Statistics();
                Results += String.Format("OutputQueueLength: {0}", ipv4Info.OutputQueueLength);
                Results += String.Format("BytesReceived: {0}", ipv4Info.BytesReceived);
                Results += String.Format("BytesSent: {0}", ipv4Info.BytesSent);

                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {

                    Results += String.Format("*** Ethernet or WiFi Network - Speed (bits per seconde): {0}", adapter.Speed);
                }
            }
            return Results;
        }
        #endregion WebMD

        #region SystemMD
        public void AddShortcut()
        {
            try
            {
                string pathToExe = @"C:\MayDayButton\MayDayButton.exe";
                string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", "MayDayButton");

                if (!Directory.Exists(appStartMenuPath))
                    Directory.CreateDirectory(appStartMenuPath);

                string shortcutLocation = Path.Combine(appStartMenuPath, "MayDayButton" + ".lnk");
                if (!File.Exists(shortcutLocation))
                {
                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

                    shortcut.Description = "MayDayButton Shortcut";
                    shortcut.IconLocation = Form1.ServerLocation + "MayDay.ico";
                    shortcut.TargetPath = pathToExe;
                    shortcut.Save();
                }
            }
            catch { //If this failed it most likely just needs admin rights
            }
        }

        public bool IsAdministrator =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent())
              .IsInRole(WindowsBuiltInRole.Administrator);

        public void GetAdmin(bool ShouldLoop = false)
        {
            Process p = new Process();
            try
            {
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                p.StartInfo.FileName = exeName;
                p.StartInfo.Verb = "runas";
                p.Start();
            }
            catch
            {
                if (ShouldLoop)
                    GetAdmin(true);
            }
        }

        public void SetStartup()
        {
#if (!DEBUG)
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\");
                Object o = key.GetValue("MayDayButton");
                if (o == null)
                {
                    key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
                    key.SetValue("MayDayButton", Application.ExecutablePath);
                }
            }
            catch { }//Problems dont exist if we ignore them
#endif
        }

        //Does system checks, such as Battery, HDD, updates and the like.
        //Should in the future add more checks just for safety
        public void Checks()
        {
            try
            {
                var frm1 = new Form1();
                //Checks for new update, in a try catch in case Windows is a bitch and doesn't allow access to the network drive
                try
                {
                    RestoreConnection();
                    var versionInfo = FileVersionInfo.GetVersionInfo(Form1.ServerLocation + "MayDayButton.exe");
                    string version = versionInfo.ProductVersion.Replace(".", "");
                    if (Int32.Parse(version) > Int32.Parse(Application.ProductVersion.Replace(".", "")))
                        UpdateEXE();
                }
                catch { }

                PowerLineStatus status = SystemInformation.PowerStatus.PowerLineStatus;

                string Result = "";
                //Check Battery %
                PowerStatus p = SystemInformation.PowerStatus;
                int a = (int)(p.BatteryLifePercent * 100);
                if (a == 25 || a == 15 || a == 5)
                    Result += String.Format("Power is {0}, charger needs to be checked.\n", a);

                string temp = Program.CheckHealth();
                if (temp != "")
                    Result += temp + "\n";

                if (Result != "")
                    frm1.sendMessage(frm1.GetEmail[2], Result);


                if (a < 25 && status == PowerLineStatus.Offline)
                {
                    wakeScreen();
                    SystemSounds.Beep.Play();
                    Thread.Sleep(500);
                    SystemSounds.Beep.Play();
                    Thread.Sleep(500);
                    SystemSounds.Beep.Play();
                    Thread.Sleep(500);
                    SystemSounds.Beep.Play();
                    Thread.Sleep(500);
                    SystemSounds.Beep.Play();
                    Thread.Sleep(500);
                    MessageBox.Show("YOUR BATTERY IS STARTING TO GET LOW AND THE DEVICE NEEDS TO BE PLUGGED IN. IF YOU UNPLUGGED THE DEVICE ON PURPOSE STOP!! YOU DO NOT NEED TO BE UNPLUGGING THE DEVICE!!", "BATTERY LOW");
                }
            }
            catch { }
        }


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        public const uint SPI_SETDESKWALLPAPER = 0x14;
        public const uint SPIF_UPDATEINIFILE = 0x1;
        public const uint SPIF_SENDWININICHANGE = 0x2;
        public void DisplayPicture(string file_name, bool update_registry)
        {
            try
            {
                // If we should update the registry,
                // set the appropriate flags.
                uint flags = 0;
                if (update_registry)
                    flags = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE;

                // Set the desktop background to this file.
                if (!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, file_name, flags))
                {
                    MessageBox.Show("SystemParametersInfo failed.",
                        "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying picture " +
                    file_name + ".\n" + ex.Message,
                    "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
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

            public Smart() { }

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

        public void cmd(string Arguments, bool isHidden = false, bool asAdmin = false)
        {
            ProcessStartInfo ProcessInfo;
            Process Process;
            try
            {
                ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + Arguments);
                ProcessInfo.UseShellExecute = true;
                ProcessInfo.CreateNoWindow = isHidden;
                if (asAdmin)
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

        [DllImport("user32.dll")]
        static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);
        public const int MOUSEEVENTF_MOVE = 0x0001;
        public void wakeScreen()
        {
            mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero);
        }

        public void Close(string ProcessName)
        {
            foreach (var process in Process.GetProcessesByName(ProcessName))
                process.Kill();
            if (Process.GetProcessesByName(ProcessName).Count() > 0)
                cmd("taskkill /F /IM " + ProcessName, false, true);
        }

        public void UpdateEXE()
        {
            var frm1 = new Form1();
            ps.Default.ShouldUpdate = false;
            ps.Default.Save();

            exportSettings();

            frm1.AppendLog("Updating");
            Directory.CreateDirectory(@"C:\MayDayButton\");

            File.Delete("MayDayButton_Old.exe");
            File.Move("MayDayButton.exe", "MayDayButton_Old.exe");
            File.Copy(Form1.ServerLocation + @"MayDayButton.exe", "MayDayButton.exe");

            Process.Start(@"C:\MayDayButton\MayDayButton.exe");
            frm1.NotiMsg("Updating....");
        }

        public void DeleteStartup()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.DeleteValue("MayDayButton");
        }

        public void exportSettings(string FileLocation = "Settings.Config")
        {
            File.Delete(FileLocation);
            using (StreamWriter sw = File.CreateText(FileLocation))
            {
                sw.WriteLine(ps.Default.X);
                sw.WriteLine(ps.Default.Y_Adjustment);
                sw.WriteLine(ps.Default.Y_Norm);

                sw.WriteLine(ps.Default.ShouldUpdate);
                sw.WriteLine(ps.Default.HighDPI);
                sw.WriteLine(ps.Default.AdminStart);
            }
        }

        public void importSettings(string FileLocation = "Settings.Config", bool ShouldDelete = false)
        {
            RestoreConnection();
            if (File.Exists(FileLocation))
            {
                string[] Settings = File.ReadAllLines(FileLocation);
                ps.Default.X = Int32.Parse(Settings[0]);
                ps.Default.Y_Adjustment = Int32.Parse(Settings[1]);
                ps.Default.Y_Norm = Int32.Parse(Settings[2]);

                ps.Default.ShouldUpdate = Boolean.Parse(Settings[3]);
                ps.Default.HighDPI = Boolean.Parse(Settings[4]);
                ps.Default.AdminStart = Boolean.Parse(Settings[5]);

                ps.Default.Save();
                if (ShouldDelete)
                    File.Delete(FileLocation);
            }
        }

        //This is gross and I hate it
        string vFalse = @"\\192.168.1.210\Server\MaydayButton\TechVacation[FALSE].txt";
        string vTrue = @"\\192.168.1.210\Server\MaydayButton\TechVacation[TRUE].txt";
        public void setVayCay(bool ForceSet = false)
        {
            if (ForceSet)
            {
                if (File.Exists(vFalse))
                    File.Move(vFalse, vTrue);
                else if (!File.Exists(vTrue))
                    using (StreamWriter sw = File.CreateText(vTrue))
                        sw.WriteLine("");
            }
            else
            {
                if (File.Exists(vFalse))
                    File.Move(vFalse, vTrue);
                else if (File.Exists(vTrue))
                    File.Move(vTrue, vFalse);
                else
                    using (StreamWriter sw = File.CreateText(vFalse))
                        sw.WriteLine("");
            }
        }
        #endregion SystemMD

        #region POSMD
        public void FixPrinters()
        {
            var frm1 = new Form1();
            bool wasOutofPaper = false;
            if (Process.GetProcessesByName("PDFPrint").Count() > 0)
                wasOutofPaper = true;

            CloseAdobe();
            frm1.AppendLog("Printer fix");
            cmd(@"net stop spooler & " +
                @"del /Q /F /S %systemroot%\System32\spool\printers\*.* & " +
                "net start spooler", false, true);
            if (wasOutofPaper)
                MessageBox.Show("It looks like your printer is either out of paper or was out of paper recently. Please double check your paper before proceeding. If you still cannot print from Flowhub, switch to a different transaction then switch back and try to print again. This is a bug in Flowhub.");
            else
                MessageBox.Show("Try it now. If issues persist, disconnect your printer for 20 seconds and reconnect it!");
        }

        public void CloseAdobe()
        {
            var frm1 = new Form1();
            frm1.AppendLog("Closing Adobe processes");
            Close("PDFPrint");
            Close("Adobe");
            Close("Acrobat");
        }

        public void RestartFlowhub()
        {
            var frm1 = new Form1();
            frm1.AppendLog("Restarted Flowhub");
            Close("Flowhub");
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FlowhubPos\Update.exe"))
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FlowhubPos\Update.exe --processStart Flowhub.exe");
            else
                MessageBox.Show("Error, did not locate Flowhub! Please start it manually!");
        }

        public void cleanInternetTemp()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);
            }
            catch { }
        }
        #endregion PosMD
    }
}
