﻿using IWshRuntimeLibrary;
using Microsoft.Win32;
using SimpleWifi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Printing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;
using Zebra.Sdk.Printer;
using Microsoft.Win32.TaskScheduler;

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
                    //Disable and disable specified adapter
                    cmd("netsh interface set interface\"" + Interface + "\" disable & netsh interface set interface\"" + Interface + "\" enable", true, true);
                }
                else
                {
                    //Disable and enable all network adapters found through Win32_NetworkAdapter
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
            catch { Console.WriteLine("Encountered an error while trying to ping your site"); }
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
            catch {
                Console.WriteLine("Failed to create StartMenu shortcut. Most likely just needs admin rights.");
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

        public string statusReport()
        {
            string results = "";
            string[] Values = new string[12];
            //Misc Variables set for the values
            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", "MayDayButton");
            string shortcutLocation = Path.Combine(appStartMenuPath, "MayDayButton" + ".lnk");
            string zebra = zebraStatuses();
            PowerLineStatus status = SystemInformation.PowerStatus.PowerLineStatus;
            PowerStatus p = SystemInformation.PowerStatus;

            //Values
            Values[0] = "Computer Name: " + Environment.MachineName;
            Values[1] = "MayDayButton v" + Application.ProductVersion;
            Values[2] = "SSID: " + showConnectedId()[0];
            Values[3] = "IP Address: " + GetLocalIPAddress();
            Values[4] = File.Exists(shortcutLocation) ? "StartMenu: Shortcut Exists" : "StartMenu: Shortcut Missing";
            Values[5] = ps.Default.HighDPI ? "High DPI: Enabled" : "High DPI: Disabled";
            Values[6] = (zebra == "") ? "No Label Printer Errors" : zebra;
            Values[7] = "Battery Percent: " + (p.BatteryLifePercent * 100).ToString() + "%";
            Values[8] = (status == PowerLineStatus.Offline) ? "Register is NOT plugged in" : "Register is plugged in";
            Values[9] = Ping("flowhub.co") ? "Flowhub(Online)" : "Flowhub(Offline)";
            Values[10] = Ping("google.com") ? "Google(Online)" : "Google(Offline)";
            Values[11] = "Created and maintained by Joe Oliveira\nLicensed freely to anyone who compiles the source code from scratch and removes the licensing checks.\nFor other users please see admin panel for full licensing information.";
            foreach(string value in Values)
                results += value + "\n---------------------------------------------\n";
            return results;
        }

        // Get the service on the local machine
        public void setRebootTask()
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Daily reboot";
                    td.RegistrationInfo.Author = "MayDayButton";
                    td.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Parse("08:00:00") });
                    td.Actions.Add(new ExecAction("cmd.exe", "shutdown /r /n", null));
                    ts.RootFolder.RegisterTaskDefinition(@"MayDay_Reboot", td);
                }
            }
            catch { Console.WriteLine("Failed to create scheduled task. Most likely doesn't have admin rights."); }
        }

        public void delRebootTask()
        {
            try
            {
                using (TaskService ts = new TaskService())
                    ts.RootFolder.DeleteTask("MayDay_Reboot");
            }
            catch { Console.WriteLine("Failed to delete scheduled task. Most likely just doesn't exist."); }
        }

        #endregion SystemMD

        #region POSMD
        public bool FixPrinters()
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

            return wasOutofPaper;
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


        //Trying out various Printer query methods, seeing which work best for our usecase.
        public string listPrintQueues()
        {
            string results = "";
            // Specify that the list will contain only the print queues that are installed as local and are shared
            EnumeratedPrintQueueTypes[] enumerationFlags = {EnumeratedPrintQueueTypes.Local,
                                                EnumeratedPrintQueueTypes.Shared};

            LocalPrintServer printServer = new LocalPrintServer();

            //Use the enumerationFlags to filter out unwanted print queues
            PrintQueueCollection printQueuesOnLocalServer = printServer.GetPrintQueues(enumerationFlags);

            Console.WriteLine("These are your shared, local print queues:\n\n");

            foreach (PrintQueue printer in printQueuesOnLocalServer)
            {
                results += "\tThe shared printer " + printer.Name + " is located at " + printer.Location + "\n";
            }
            return results;
        }

        public void deletePrinters()
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                cmd("printui.exe /q /dl /n " + name, true, true);
            }
        }

        public string listPrinters()
        {
            string results = "";
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                var status = printer.GetPropertyValue("Status");
                var isDefault = printer.GetPropertyValue("Default");
                var isNetworkPrinter = printer.GetPropertyValue("Network");
                var availability = printer.GetPropertyValue("Availability");
                var extStatus = printer.GetPropertyValue("ExtendedPrinterStatus");
                var errorState = printerError(printer);
                results += String.Format("{0} (Status: {1}, Default: {2}, ErrorState: {3})\n------------------------------\n",
                            name, status, isDefault, errorState);
            }
            return results;
        }

        public string zebraStatuses()
        {
            string results = "";
            foreach (DiscoveredUsbPrinter p in UsbDiscoverer.GetZebraUsbPrinters())
            {
                Connection connection = new UsbConnection(p.ToString());
                connection.Open();
                ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                PrinterStatus printerStatus = printer.GetCurrentStatus();
                if (!printerStatus.isReadyToPrint)
                {
                    if (printerStatus.isPaused)
                    {
                        results += "Cannot print, printer is paused.";
                    }
                    else if (printerStatus.isHeadOpen)
                    {
                        results += "Cannot print, printer head is open. Make sure the top lid of the printer is fully secured. If it is make sure that the light is a solid green. If it's flashing green please press the arrow button next to the light.";
                    }
                    else if (printerStatus.isPaperOut)
                    {
                        results += "Cannot print, no paper. Please refill the paper roll.";
                    }
                    else
                    {
                        results += "Cannot print, unknown error. Contact your technician for more help.";
                    }
                    results += "\n------------------------\n";
                }
                Console.WriteLine(results);
                //MessageBox.Show("Errors found for the Zebra Printers!\n\n" + results);
            }
            return results;
        }

        public string printerError(ManagementBaseObject printer)
        {
            switch (printer.GetPropertyValue("DetectedErrorState")){
                default:
                    return "Unkown";
                case 1:
                    return "Other";
                case 2:
                    return "No Error";
                case 3:
                    return "Low Paper";
                case 4:
                    return "No Paper";
                case 5:
                    return "Low Toner";
                case 6:
                    return "No Toner";
                case 7:
                    return "Door Open";
                case 8:
                    return "Jammed";
                case 9:
                    return "Offline";
                case 10:
                    return "Service Requested";
                case 11:
                    return "Output Bin Full";
            }
        }

        public void testPrinter()
        {
            string s = "^XA^LH30,30\n^FO20,10^ADN,90,50^AD^FDTesting!^FS\n^XZ";
            PrintDialog pd = new PrintDialog();
            pd.PrinterSettings = new PrinterSettings();
            if (DialogResult.OK == pd.ShowDialog())
            {
                RawPrinterHelper.SendStringToPrinter(pd.PrinterSettings.PrinterName, s);
            }
        }
        #endregion PosMD
    }

    //Found on StackOverflow, no links cause i was stupid and didnt bookmark it.
    //Main usecase is to print properly formatted Zebra instructions to my label printers.
    //Also seems to work for other printers so that's a plus.
    public static class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "My C#.NET RAW Document";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            // Open the file.
            FileStream fs = new FileStream(szFileName, FileMode.Open);
            // Create a BinaryReader on the file.
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            Byte[] bytes = new Byte[fs.Length];
            bool bSuccess = false;
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }
        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // How many characters are in the string?
            dwCount = szString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
    }
}
