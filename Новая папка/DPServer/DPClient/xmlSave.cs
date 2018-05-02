using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xmlSettings;
using System.Management;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;
namespace DPClient
{
    class xmlSave
    {
        public   XmlSet settings = new XmlSet();
        public bool sysWasChecked = false;
        public void Save(string filename)
        {

            if (!(System.IO.File.Exists(filename)))
            {
                settings.sysinfo1 = new sysinfo();
                settings.guid = Guid.NewGuid();
                
                
                Serialize(settings, filename);
            }
            else
            {

                settings = DeSerialize(filename);

            }

            
            setDeviceInfo();
            
            Serialize(settings, filename);
        }

        private bool setDeviceInfo()
        {
            try
            {
                if (sysWasChecked)
                {
                    setSysInfo();
                    settings.checkSys();
                    settings.error = "Sys is ok";
                }
                else {
                    setSysInfo();
                settings.checkSys();}
                Console.WriteLine(settings.error);
                
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
        private void setSysInfo()

        {
            try
            {
                settings.sysinfo1 = new sysinfo();
                settings.sysinfo1.lastCheckDate = DateTime.Now.ToString();
                ManagementObjectSearcher searcher8 =
        new ManagementObjectSearcher("root\\CIMV2",
        "SELECT * FROM Win32_Processor");

                foreach (ManagementObject queryObj in searcher8.Get())
                {

                    settings.sysinfo1.processorInfo.Name = queryObj["Name"].ToString();
                    settings.sysinfo1.processorInfo.NumberOfCores = queryObj["NumberOfCores"].ToString();
                    settings.sysinfo1.processorInfo.ID = queryObj["ProcessorId"].ToString();
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message + "processor Checking"); }
            try
            {
                ManagementObjectSearcher searcher12 =
        new ManagementObjectSearcher("root\\CIMV2",
        "SELECT * FROM Win32_PhysicalMemory");


                foreach (ManagementObject queryObj in searcher12.Get())
                {
                    ramInfo r = new ramInfo();
                    r.BankLabel = queryObj["BankLabel"].ToString();
                    r.Capacity = Math.Round(System.Convert.ToDouble(queryObj["Capacity"]) / 1024 / 1024 / 1024, 2).ToString() + " gb";
                    r.Speed = queryObj["Speed"].ToString();
                    settings.sysinfo1.ramInfo.Add(r);
                }

            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message + " RAM checking"); }

            try
            {
                ManagementObjectSearcher searcher11 =
        new ManagementObjectSearcher("root\\CIMV2",
        "SELECT * FROM Win32_VideoController");

                foreach (ManagementObject queryObj in searcher11.Get())
                {
                    gpuInfo g = new gpuInfo();

                    g.AdapterRam = queryObj["AdapterRAM"].ToString();
                    g.Caption = queryObj["Caption"].ToString();
                    g.Description = queryObj["Description"].ToString();
                    g.VideoProcessor = queryObj["VideoProcessor"].ToString();
                    settings.sysinfo1.gpuInfo.Add(g);

                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message + " VideoController checking"); }

            try
            {
                ManagementObjectSearcher searcher =
               new ManagementObjectSearcher("root\\CIMV2",
               "SELECT * FROM Win32_Volume");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    driveInfo d = new driveInfo();
                    d.Capacity = Math.Round(System.Convert.ToDouble(queryObj["Capacity"]) / 1024 / 1024 / 1024, 2).ToString() + " gb";
                    d.Caption = queryObj["Caption"].ToString();
                    if (queryObj["DriveLetter"] == null) d.DriveLetter = "??";
                    else 
                        d.DriveLetter = queryObj["DriveLetter"].ToString();
                    d.DriveType = queryObj["DriveType"].ToString();
                    d.FileSystem = queryObj["FileSystem"].ToString();
                    d.FreeSpace = Math.Round(System.Convert.ToDouble(queryObj["FreeSpace"]) / 1024 / 1024 / 1024, 2).ToString() + " gb";
                    settings.sysinfo1.driveInfo.Add(d);
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message + " Drive checking"); }

            try
            {


                ManagementObjectSearcher searcher5 =
           new ManagementObjectSearcher("root\\CIMV2",
               "SELECT * FROM Win32_OperatingSystem");

                foreach (ManagementObject queryObj in searcher5.Get())
                {

                    settings.sysinfo1.OSInfo.BuildNumber = queryObj["BuildNumber"].ToString();
                    settings.sysinfo1.OSInfo.Caption = queryObj["Caption"].ToString();
                    settings.sysinfo1.OSInfo.Name = queryObj["Name"].ToString();
                    settings.sysinfo1.OSInfo.OSType = queryObj["OSType"].ToString();
                    settings.sysinfo1.OSInfo.RegisteredUser = queryObj["RegisteredUser"].ToString();
                    settings.sysinfo1.OSInfo.SerialNumber = queryObj["SerialNumber"].ToString();
                    settings.sysinfo1.OSInfo.ServicePackMajorVersion = queryObj["ServicePackMajorVersion"].ToString();
                    settings.sysinfo1.OSInfo.ServicePackMinorVersion = queryObj["ServicePackMinorVersion"].ToString();
                    settings.sysinfo1.OSInfo.Status = queryObj["Status"].ToString();
                    settings.sysinfo1.OSInfo.SystemDevice = queryObj["SystemDevice"].ToString();
                    settings.sysinfo1.OSInfo.SystemDirectory = queryObj["SystemDirectory"].ToString();
                    settings.sysinfo1.OSInfo.SystemDrive = queryObj["SystemDrive"].ToString();
                    settings.sysinfo1.OSInfo.Version = queryObj["Version"].ToString();
                    settings.sysinfo1.OSInfo.WindowsDirectory = queryObj["WindowsDirectory"].ToString();
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message + " OS checking"); }
        }

        
       

        private int getDeviceHash(string path)
        {
            ManagementClass mClass = new ManagementClass(path);
            

            return mClass.GetHashCode();
        }
        private void Serialize(XmlSet set, string filename)
        {
            XmlSerializer ser = new XmlSerializer(typeof(XmlSet));
            System.IO.TextWriter writer = new System.IO.StreamWriter(filename);
            ser.Serialize(writer, set);
            writer.Close();
        }

        public XmlSet DeSerialize(string filename)
        {
            XmlSerializer ser = new XmlSerializer(typeof(XmlSet));
            XmlSet set = new XmlSet();
            System.IO.TextReader writer = new System.IO.StreamReader(filename);
            set = (XmlSet) ser.Deserialize(writer);
            writer.Close();

            return set;
        }
    }
   
}
