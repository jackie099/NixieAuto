﻿using System;
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
                string ProjectName = Path.GetFileNameWithoutExtension(changedfilefullpath);

                    VideoPath = Path.GetDirectoryName(changedfilefullpath) + @"\" + Path.GetFileNameWithoutExtension(changedfilefullpath) + ".mp4";
                    SubtitlePath = Path.GetDirectoryName(changedfilefullpath) + @"\" + Path.GetFileNameWithoutExtension(changedfilefullpath) + ".ass";

                Debug.WriteLine("Video Path:"+VideoPath);
                Debug.WriteLine("Subtitle Path:" + SubtitlePath);
                startprocess(VideoPath,SubtitlePath,ProjectName);

            }


        }

        // 检测带有flv或者ass的文件是否同时存在
        private bool checkiftheresbothfiles(string filepath, string filename, FileInfo fread)
        {
            //Debug.WriteLine(filepath + "|" + filename);
            string fileextension = Path.GetExtension(filepath);
            //Debug.WriteLine(Path.GetDirectoryName(filepath) +@"\"+ Path.GetFileNameWithoutExtension(filepath) + ".ass");
            if (fileextension == ".mp4")
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
                if (checkiffileexist(Path.GetDirectoryName(filepath) + @"\" + Path.GetFileNameWithoutExtension(filepath) + ".mp4"))
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
        string ScriptPath;

        private void startprocess(string inputfile, string inputass,string ProjectName)
        {
            string NewScript = @"E:\tools\" + ProjectName + ".cmd";
            File.Copy(@"E:\tools\templ.cmd", NewScript);
            string rdScript = File.ReadAllText(NewScript);
            rdScript = rdScript.Replace("ThisIsTheAwesomeVideoPath",inputfile);
            rdScript = rdScript.Replace("ThisIsTheAwesomeSubPath", inputfile);
            rdScript = rdScript.Replace("ThisIsTheAwesomeProjectName", inputfile);
            File.WriteAllText(NewScript,rdScript);
            ScriptPath = NewScript;
            Exe("","","");
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

        private void button2_Click(object sender, EventArgs e)
        {
            Exe("","","");
 
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
            //将cmd的标准输入和输出全部重定向到.NET的程序里 
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
            cmd.StandardInput.WriteLine(ScriptPath);


            //cmd.StandardInput.WriteLine("CD /D \"" + toolpath + "\"");

            //cmd.StandardInput.WriteLine("ffmpeg -i " + ALLinputvideo + " -f wav - | neroaacenc -q 0.7 -lc -if - -ignorelength -of " + ALLinputvideo + "_a.m4a");

            //cmd.StandardInput.WriteLine("echo LoadPlugin(\"" + toolpath + "\\LSMASHSource.dll\") > \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("echo LoadPlugin(\"" + toolpath + "\\VSFilterMod.dll\") >> \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("echo LwlibavVideoSource(" + ALLinputvideo + ") >> \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("echo TextsubMod(" + ALLinputass + ") >> \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("avs4x264mod --x264-binary x264_64_tMod-8bit-all --level 4.1 --bitrate 1600 --pass 1 --stats " + ALLinputvideo + ".stats --opt 1 --ref 3 --deblock -1:-1 --vbv-bufsize 17000 --vbv-maxrate 17000 --b-pyramid none  --weightp 2 --b-adapt 2 --bframes 3 --keyint 250 --min-keyint 1 --direct auto --me esa --merange 62 -m 10 -t 2 --rc-lookahead 50 --qcomp 0.7 --aq-mode 1 --aq-strength 0.8 --psy-rd 0.4:0.1 --no-dct-decimate --fade-compensate 0.1 --stylish --output NUL \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("avs4x264mod --x264-binary x264_64_tMod-8bit-all --level 4.1 --bitrate 1500 --pass 2 --stats " + ALLinputvideo + ".stats --opt 1 --ref 3 --deblock -1:-1 --vbv-bufsize 17000 --vbv-maxrate 17000 --b-pyramid none  --weightp 2 --b-adapt 2 --bframes 3 --keyint 250 --min-keyint 1 --direct auto --me esa --merange 62 -m 10 -t 2 --rc-lookahead 50 --qcomp 0.7 --aq-mode 1 --aq-strength 0.8 --psy-rd 0.4:0.1 --no-dct-decimate --fade-compensate 0.1 --stylish --output NUL \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("avs4x264mod --x264-binary x264_64_tMod-8bit-all --level 4.1 --bitrate 1400 --pass 3 --stats " + ALLinputvideo + ".stats --opt 1 --ref 3 --deblock -1:-1 --vbv-bufsize 17000 --vbv-maxrate 17000 --b-pyramid none  --weightp 2 --b-adapt 2 --bframes 3 --keyint 250 --min-keyint 1 --direct auto --me esa --merange 62 -m 10 -t 2 --rc-lookahead 50 --qcomp 0.7 --aq-mode 1 --aq-strength 0.8 --psy-rd 0.4:0.1 --no-dct-decimate --fade-compensate 0.1 --stylish --output " + ALLinputvideo + "_v.mp4 \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("remuxer -i " + ALLinputvideo + "_v.mp4 -i " + ALLinputvideo + "_a.m4a -o " + ALLinputvideo + "[NixieAuto].flv");

            //cmd.StandardInput.WriteLine("del \"" + temppath + "tmpavs" + randomtmp + ".avs\"");

            //cmd.StandardInput.WriteLine("del " + ALLinputvideo + ".lwi");

            //cmd.StandardInput.WriteLine("del " + ALLinputvideo + ".stats");

            //cmd.StandardInput.WriteLine("del " + ALLinputvideo + ".stats.mbtree");

            //cmd.StandardInput.WriteLine("del " + ALLinputvideo + "_v.mp4");

            //cmd.StandardInput.WriteLine("del " + ALLinputvideo + "_a.m4a");

            //cmd.StandardInput.WriteLine("pause");


            cmd.OutputDataReceived += new DataReceivedEventHandler(cmd_OutputDataReceived);
            cmd.BeginOutputReadLine();
            //
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
            //Debug.WriteLine(e.Data);
            //String rd = "";
            //if (checkiffileexist(ALLinputvideo + ".Progress.txt"))
            //{
            //    rd = File.ReadAllText(ALLinputvideo + ".Progress.txt");
            //}
            //File.WriteAllText(ALLinputvideo + ".Progress.txt", rd + "/n" + e.Data);


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
                    textBox1.AppendText(msg + "\r\n");
                }
            }
        }


    }
}
