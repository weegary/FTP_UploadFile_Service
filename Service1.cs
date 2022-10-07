using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Net;

namespace FTP_UploadFile_Service
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        FTP ftp = new FTP();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 60 * 1000;   
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            if (DateTime.Now.Minute % 5 == 0)
            {
                try
                {
                    string[] files = Directory.GetFiles(@"C:\folder\");
                    string last_ftp_file = ftp.GetLastFile();

                    if (IsLastFileUpdated(files.Last(), last_ftp_file))
                        UploadLastFileToFTP(files.Last());
                }
                catch
                {

                }
            }
        }

        private bool IsLastFileUpdated(string last_local_file, string last_ftp_file)
        {
            if (last_local_file != last_ftp_file)
                return true;
            return false;
        }

        private void UploadLastFileToFTP(string last_local_file)
        {
            ftp.UploadFile(last_local_file);
        }
    }

    class FTP
    {
        string host = "ftp://location.com"; 
        string directory = "folder";
        string user_id = "user_id";
        string password = "password";
        WebClient client = new WebClient();

        public FTP()
        {
            client.Credentials = new NetworkCredential(user_id, password);
        }

        public void UploadFile(string fileName)
        {
            string destination_path = host + directory + "//" + fileName;
            client.UploadFile(destination_path, fileName);
        }

        public string GetLastFile()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host + directory + "//");
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(user_id, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string r = reader.ReadToEnd();
            string status = response.StatusDescription;
            reader.Close();
            response.Close();
            string[] files = r.Split('\n');
            string last_file = files[files.Length-1];
            return last_file.Split()[last_file.Split().Length - 2];
        }
    }
}
