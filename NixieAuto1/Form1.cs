using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace NixieAuto1
{
    public partial class Form1 : Form
    {

        public bool WaitForFile(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();

                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                       "WaitForFile {0} failed to get an exclusive lock: {1}",
                        fullPath, ex.ToString());

                    //if (numTries > 10)
                    //{
                    //    Debug.WriteLine(
                    //        "WaitForFile {0} giving up after 10 tries",
                    //        fullPath);
                    //    return false;
                    //}

                    System.Threading.Thread.Sleep(500);
                }
            }

            Debug.WriteLine("WaitForFile {0} returning true after {1} tries",
                fullPath, numTries);
            return true;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            watch();

        }


        //监控是否有文件变动
        private void watch()
        {
            //     MessageBox.Show("Watching");
            ShowMessage("NixieAuto service started!");
            string path;
            //path = @"C:\Users\Jackie099\ownCloud\NixieAuto";
            path = @"C:\Users\Administrator\ownCloud\NixieAuto";
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            //  watcher.NotifyFilter = NotifyFilters.FileName |
            //                         NotifyFilters.LastWrite |
            //                         NotifyFilters.CreationTime;
            watcher.NotifyFilter = NotifyFilters.Attributes |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.FileName |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.Size |
                                   NotifyFilters.Security;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;

        }
        //DateTime lastRead = DateTime.MinValue;
        //private void OnChanged(object source, FileSystemEventArgs e)
        //{
        //    DateTime lastWriteTime = File.GetLastWriteTime(@"E:\Nixie");
        //    if (lastWriteTime != lastRead)
        //    {
        //        lastRead = lastWriteTime;

        //        string changedfilefullpath = e.FullPath;
        //        string changedfilename = e.Name;
        //        FileInfo fread = new FileInfo(changedfilefullpath);
        //        while (IsFileLocked(fread)) {
        //            System.Threading.Thread.Sleep(1000);
        //        }
        //        Debug.WriteLine("File changed detected:"+changedfilefullpath);

        //    }
        //}
        string lastpath;
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (lastpath == e.FullPath)
            {
                return;
            }
            lastpath = e.FullPath;

            string changedfilefullpath = e.FullPath;
            string changedfilename = e.Name;

            //while (IsFileLocked(fread))
            //{
            //    System.Threading.Thread.Sleep(1000);
            //}


            if (Path.GetExtension(e.FullPath) == ".mp4" || Path.GetExtension(e.FullPath) == ".ass")
            {
                WaitForFile(e.FullPath);
            }
            else
            {
                return;
            }


            FileInfo fread = new FileInfo(changedfilefullpath);
            Debug.WriteLine("File changed detected:" + changedfilefullpath);
            ShowMessage("File name received:" + changedfilefullpath);
            if (checkiftheresbothfiles(changedfilefullpath, changedfilename, fread))
            {
                string FileExtension = Path.GetExtension(changedfilefullpath);
                string VideoPath;
                string SubtitlePath;
                string ProjectName = Path.GetFileNameWithoutExtension(changedfilefullpath);

                VideoPath = Path.GetDirectoryName(changedfilefullpath) + @"\" + Path.GetFileNameWithoutExtension(changedfilefullpath) + ".mp4";
                SubtitlePath = Path.GetDirectoryName(changedfilefullpath) + @"\" + Path.GetFileNameWithoutExtension(changedfilefullpath) + ".ass";

                Debug.WriteLine("Video Path:" + VideoPath);
                Debug.WriteLine("Subtitle Path:" + SubtitlePath);
                ShowMessage("Video path:" + VideoPath);
                ShowMessage("Subtitle path:" + SubtitlePath);
                startprocess(VideoPath, SubtitlePath, ProjectName);

            }
            else
            {
                String FileName = Path.GetFileName(changedfilefullpath);
                String FileNameWithoutEx = Path.GetFileNameWithoutExtension(changedfilefullpath);
                if (GetLast(FileNameWithoutEx,8) == "[upload]")
                {
                    Update(GetFirst(FileNameWithoutEx), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }


        }

        // 检测带有flv或者ass的文件是否同时存在
        private bool checkiftheresbothfiles(string filepath, string filename, FileInfo fread)
        {
            Debug.WriteLine(filepath + "|" + filename);
            string fileextension = Path.GetExtension(filepath);
            Debug.WriteLine(Path.GetDirectoryName(filepath) + @"\" + Path.GetFileNameWithoutExtension(filepath) + ".ass");
            if (fileextension == ".mp4")
            {
                if (File.Exists(Path.GetDirectoryName(filepath) + @"\" + Path.GetFileNameWithoutExtension(filepath) + ".ass"))
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }
            else if (fileextension == ".ass")
            {
                if (File.Exists(Path.GetDirectoryName(filepath) + @"\" + Path.GetFileNameWithoutExtension(filepath) + ".mp4"))
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }
            else
            {
                return (false);
            }
        }




        //开始压制
        string ScriptPath;

        private void startprocess(string inputfile, string inputass, string ProjectName)
        {
            string NewScript = @"D:\Nixiesubs\Enc\" + ProjectName + ".py";
            if (!File.Exists(NewScript))
            {
                File.Copy(@"D:\Nixiesubs\To-bilibili_Enc\To-bilibili_Enc_mod.py", NewScript);
            }
            else
            {
                File.Delete(NewScript);
                File.Copy(@"D:\Nixiesubs\To-bilibili_Enc\To-bilibili_Enc_mod.py", NewScript);
            }

            string rdScript = File.ReadAllText(NewScript, Encoding.UTF8);
            rdScript = rdScript.Replace("ThisIsTheAwesomeVideoPath", "\'" + inputfile.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\'", "\\\'") + "\'");
            rdScript = rdScript.Replace("ThisIsTheAwesomeSubPath", "\'" + inputass.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\'", "\\\'") + "\'");
            //rdScript = rdScript.Replace("ThisIsTheAwesomeProjectName", ProjectName);
            //File.WriteAllText(NewScript,rdScript, Encoding.GetEncoding("gb2312"));
            File.WriteAllText(NewScript, rdScript, Encoding.UTF8);
            ScriptPath = NewScript;
            //Exe("","","");


            //System.Diagnostics.Process.Start("\"" + ScriptPath + "\""+" "+ "\"" + inputfile + "\""+" "+ "\"" + inputass + "\"");


            System.Diagnostics.Process.Start("\"" + ScriptPath + "\"");
            Insert(ProjectName, Path.GetFileName(inputfile), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Working on", ScriptPath.Replace("\\", "\\\\"));
            ShowMessage("Enc started, script path:" + ScriptPath);
        }


        //查看文件是否可用(用于检测是否复制完成)
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {

                ShowMessage("File locked, waiting");
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }



        private void button2_Click(object sender, EventArgs e)
        {
            Exe("", "", "");

        }


        string cmd11;
        string cmd22;
        string cmd33;
        public void Exe(string cmd1, string cmd2, string cmd3)
        {
            cmd11 = cmd1;
            cmd22 = cmd2;
            cmd33 = cmd3;
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            //cmd.StartInfo.Arguments = args;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.UseShellExecute = false;
            //
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //
            ThreadPool.QueueUserWorkItem(new WaitCallback(ExeThread), cmd);
        }
        private void ExeThread(object obj)
        {
            string toolpath = "E:\\tools";
            string temppath = System.IO.Path.GetTempPath();
            string temppathn = System.IO.Path.GetTempFileName();
            Random rdm = new Random();
            string randomtmp = rdm.Next(1, 9999).ToString();
            Process cmd = obj as Process;
            cmd.Start();
            cmd.StandardInput.WriteLine("\"" + ScriptPath + "\"");
            cmd.OutputDataReceived += new DataReceivedEventHandler(cmd_OutputDataReceived);
            cmd.BeginOutputReadLine();
            Application.DoEvents();
            cmd.WaitForExit();
            if (cmd.ExitCode != 0)
            {
                ShowMessage(cmd.StandardOutput.ReadToEnd());
            }
            cmd.Close();
        }
        void cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ShowMessage(e.Data);
        }
        private delegate void ShowMessageHandler(string msg);
        private void ShowMessage(string msg)
        {

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowMessageHandler(ShowMessage), new object[] { msg });
            }
            else
            {
                if (msg != null)
                {
                    textBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + msg + "\r\n");
                    File.AppendAllText(@"C:\inetpub\wwwroot\NixieAuto\log.txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + msg + "\r\n");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            watch();
        }

        //拦截关闭
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShowMessage("NixieAuto is closed");
        }

        //Mysql部分
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;


        public void DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {

            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }
        private bool OpenConnection()
        {
            try
            {
                DBConnect();
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        ShowMessage("DB error: Cannot connect to server");
                        break;

                    case 1045:
                        ShowMessage("DB error: Invalid username/password");
                        break;
                }
                return false;
            }
        }

        //关闭连接
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                ShowMessage("DB error:" + ex.Message);
                return false;
            }
        }

        //加入任务
        public void Insert(string TaskName, string FileName, string StartTime, string EndTime, string ScriptName)
        {
            string query = "INSERT INTO NixieAuto (TaskName, FileName, StartTime,EndTime, ScriptName) VALUES('" + TaskName + "', '" + FileName + "','" + StartTime + "','" + EndTime + "','" + ScriptName + "')";

            if (this.OpenConnection() == true)
            {

                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    ShowMessage("DB error:" + ex.Message);
                }

                this.CloseConnection();
            }
        }

        //更新任务
        public void Update(string TaskName, string EndTime)
        {
            string query = "UPDATE NixieAuto SET EndTime='" + EndTime + "' WHERE TaskName='" + TaskName + "'";

            if (this.OpenConnection() == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.CommandText = query;
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    ShowMessage("DB error:" + ex.Message);
                }

                this.CloseConnection();
            }
        }
        public string GetLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
            {
                return source;
            }
                
            else
            {
                return source.Substring(source.Length - tail_length);
            }
        }
        public string GetFirst(string source)
        {
            if(source.Length <= 8)
            {
                return source;
            }
            else
            {
                return source.Substring(0, source.Length - 8);
            }
        }

    }
}
