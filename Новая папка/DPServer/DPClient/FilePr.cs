using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DPClient
{
    class FilePr
    {
        public string CheckSum(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string Hex = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return Hex;
            }
        }

        public byte[] GetBytes(string path)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(path);


            long size = file.Length;
            byte[] sendData = new Byte[size];
            sendData = File.ReadAllBytes(path);

            return sendData;
        }

    }
}
