using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace FreeTouch
{
    class AdbProcess
    {
        private Process process;
        private string adbFile = Properties.Resources.StrAdbFile;
        private int wait = 1000 * 10;

        public bool Start()//启动adb
        {
            try
            {
                if (process == null)
                {
                    process = new Process();
                    process.StartInfo.FileName = adbFile;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;//重定向标准输出流
                    process.StartInfo.RedirectStandardError = true;//重定向标准错误流
                    process.StartInfo.CreateNoWindow = true;
                    return process.Start();
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool Execute(string cmd, int delay = 0)
        {
            if (process != null)
            {
                try
                {
                    process.StartInfo.Arguments = cmd;
                    process.Start();
                    process.WaitForExit(wait);
                    Thread.Sleep(delay);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public string ExecuteAndReturn(string cmd, int delay)
        {
            string output = "";

            if (process != null)
            {
                try
                {
                    process.StartInfo.Arguments = cmd;
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();//获取输出信息
                    process.WaitForExit(wait);
                    Thread.Sleep(delay);
                    return output;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void Close()//关闭adb
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == "adb")
                {
                    try
                    {
                        p.Kill();
                        p.Dispose();
                    }
                    catch (Win32Exception e)
                    {
                        Console.WriteLine(e.Message);
                        //System.Windows.Forms.MessageBox.Show(e.Message);
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                        //System.Windows.Forms.MessageBox.Show(e.Message);
                    }
                }
            }
        }
    }
}
