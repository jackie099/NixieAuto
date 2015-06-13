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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NixieAuto1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            watch();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //监控是否有文件变动
        private void watch()
        {
            MessageBox.Show("Watching");
            string path;
            path = @"E:\Nixie";
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.FileName |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.CreationTime;
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
            FileInfo fread = new FileInfo(changedfilefullpath);
            while (IsFileLocked(fread))
            {
                System.Threading.Thread.Sleep(1000);
            }
            //Debug.WriteLine("File changed detected:" + changedfilefullpath);
            if (checkiftheresbothfiles(changedfilefullpath, changedfilename, fread))
            {
                string FileExtension = Path.GetExtension(changedfilefullpath);
                string VideoPath;
                string SubtitlePath;

                    VideoPath = Path.GetDirectoryName(changedfilefullpath) + @"\" + Path.GetFileNameWithoutExtension(changedfilefullpath) + ".flv";
                    SubtitlePath = Path.GetDirectoryName(changedfilefullpath) + @"\" + Path.GetFileNameWithoutExtension(changedfilefullpath) + ".ass";

                Debug.WriteLine("Video Path:"+VideoPath);
                Debug.WriteLine("Subtitle Path:" + SubtitlePath);

            }


        }

        // 检测带有flv或者ass的文件是否同时存在
        private bool checkiftheresbothfiles(string filepath, string filename, FileInfo fread)
        {
            //Debug.WriteLine(filepath + "|" + filename);
            string fileextension = Path.GetExtension(filepath);
            //Debug.WriteLine(Path.GetDirectoryName(filepath) +@"\"+ Path.GetFileNameWithoutExtension(filepath) + ".ass");
            if (fileextension == ".flv")
            {
                if (checkiffileexist(Path.GetDirectoryName(filepath) + @"\" + Path.GetFileNameWithoutExtension(filepath) + ".ass"))
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
                if (checkiffileexist(Path.GetDirectoryName(filepath) + @"\" + Path.GetFileNameWithoutExtension(filepath) + ".flv"))
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


        //检查特定文件是否存在
        private bool checkiffileexist(string filepath)
        {
            if (File.Exists(filepath))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        //开始压制
        private void startprocess(string inputfile, string inputass,string outputpath)
        {

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
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
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


    }
}
