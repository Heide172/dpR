using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlSettings
{
    [Serializable]
    public class XmlSet
    {

       public Guid guid;
       public sysinfo sysinfo1 = new sysinfo();
       public List<sysinfo> sysinfoList = new List<sysinfo>();
        
       public string error = "Sys is ok";
       public void checkSys()
       {
           if (sysinfoList.Count == 0) 
               sysinfoList.Add(sysinfo1);
           try 
           {
               if (sysinfo1.processorInfo.ID != sysinfoList[sysinfoList.Count - 1].processorInfo.ID)
                   error = "Dismatching proccessor, last date was: " + sysinfoList[sysinfoList.Count - 1].lastCheckDate;
               
               if  (sysinfo1.ramInfo.Count != sysinfoList[sysinfoList.Count - 1].ramInfo.Count)
                   error = "Dismatching ram, last date was: "  + sysinfoList[sysinfoList.Count - 1].lastCheckDate;
               
           }
           catch (Exception ex)
           {
               Console.WriteLine(ex.Message);
           }

          
       }

        
    }

[Serializable]
    public class sysinfo
    {
        
       public operatingSystemInfo OSInfo;
       public processorInfo processorInfo;
       public List<ramInfo> ramInfo;
       public List<gpuInfo> gpuInfo;
       public List<driveInfo> driveInfo;
       public string lastCheckDate;

       public sysinfo()
       { 
        OSInfo = new operatingSystemInfo();
        processorInfo = new processorInfo();
        ramInfo = new List<ramInfo>();
        gpuInfo = new List<gpuInfo>();
        driveInfo = new List<driveInfo>();

       }
    }

[Serializable]

public struct processorInfo
{
    public string Name;
    public string NumberOfCores;
    public string ID;
}
[Serializable]
public struct ramInfo
{
    public string BankLabel;
    public string Capacity;
    public string Speed;
}
[Serializable]
public struct gpuInfo
{
    public string AdapterRam;
    public string Caption;
    public string Description;
    public string VideoProcessor;
}
[Serializable]
public struct driveInfo
{
    public string Capacity;
    public string Caption;
    public string DriveLetter;
    public string DriveType;
    public string FileSystem;
    public string FreeSpace;
}
[Serializable]
public struct operatingSystemInfo
{
    public string BuildNumber;
    public string Caption;
    public string Name;
    public string OSType;
    public string RegisteredUser;
    public string SerialNumber;
    public string ServicePackMajorVersion;
    public string ServicePackMinorVersion;
    public string Status;
    public string SystemDevice;
    public string SystemDirectory;
    public string SystemDrive;
    public string Version;
    public string WindowsDirectory;
}
}
