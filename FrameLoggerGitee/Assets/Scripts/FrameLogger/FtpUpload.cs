using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace FrameLogger
{
    public class FtpUpload
    {
        public static void UploadFtpFile(string ip, string port, string userName, string passWord, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return;
            }
            WebClient client = new WebClient();

            client.Credentials = new NetworkCredential(userName, passWord);
            var ftpServerPath = $"ftp://{ip}:{port}/{fileInfo.Name}";
            try
            {
                var uri = new Uri(ftpServerPath);
                client.UploadFileAsync(uri, WebRequestMethods.Ftp.UploadFile, filePath);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }


        public static void UploadFtpFile(string ip, string port, string userName, string password, string fileName, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            var client = new WebClient();
            client.Encoding = new UTF8Encoding(false);
            client.Credentials = new NetworkCredential(userName, password);
            var ftpServerPath = $"ftp://{ip}:{port}/{fileName}";
            try
            {
                var uri = new Uri(ftpServerPath);
                client.UploadData(uri, WebRequestMethods.Ftp.UploadFile, data);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }
    }
}