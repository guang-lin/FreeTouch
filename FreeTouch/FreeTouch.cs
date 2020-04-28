using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FreeTouch
{
    public class FreeTouch
    {
        private Form1 form1;
        private delegate void ControlDelegate();
        private int x1 = 0, y1 = 0;//触摸点或划动起始点
        private int x2 = 0, y2 = 0;//划动终点
        private int duration = 100;//划动用时
        private string keyCode = "";
        private int delay = 0;//等待执行下一条命令的时长
        private AdbProcess adb;
        private string command = "";

        private const int CLICK = 0;
        private const int SWIPE = 1;
        private const int KEYEVENT = 2;
        private const int COMMAND = 3;

        public bool IsRun { set; get; } = false;

        public FreeTouch(Form1 form1)
        {
            this.form1 = form1;
            adb = new AdbProcess();
        }

        private void SetClickPoint(int x1, int y1)
        {
            this.x1 = x1;
            this.y1 = y1;
        }

        private void SetSwipePoint(int x1, int y1, int x2, int y2, int duration = 100)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.duration = duration;
        }

        private void Touch(int mode)//该函数每被调用一次，则执行一条指令
        {
            switch (mode)
            {
                case FreeTouch.CLICK:
                    adb.Execute(String.Format("shell input touchscreen tap {0} {1}", x1, y1), delay);
                    break;
                case FreeTouch.SWIPE:
                    adb.Execute(String.Format("shell input touchscreen swipe {0} {1} {2} {3} {4}", x1, y1, x2, y2, duration), delay);
                    break;
                case FreeTouch.KEYEVENT:
                    adb.Execute(String.Format("shell input keyevent {0}", keyCode), delay);
                    break;
                case FreeTouch.COMMAND:
                    adb.Execute(command, delay);
                    break;
                default: break;
            }
        }

        public bool Connect()
        {
            adb.Start();
            string output = adb.ExecuteAndReturn("devices", 0);
            if (output != null)
            {
                if (output.Trim().EndsWith("device"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Run(string[,] strCommand, int codeRows, int loop = 1)
        {
            string countInfo = "";
            IsRun = true;
            adb.Start();//启动adb程序

            for (int i = 0; i < loop; i++)
            {
                for (int j = 0; j < codeRows; j++)
                {
                    if (!IsRun)
                    {
                        break;
                    }
                    else
                    {
                        try
                        {
                            delay = Convert.ToInt16(Convert.ToDouble(strCommand[j, 8]) * 1000);
                            switch (strCommand[j, 0])
                            {
                                case "点击":
                                    SetClickPoint(Convert.ToInt16(strCommand[j, 1]), Convert.ToInt16(strCommand[j, 2]));
                                    Touch(FreeTouch.CLICK);
                                    break;
                                case "划动":
                                    SetSwipePoint(Convert.ToInt16(strCommand[j, 1]), Convert.ToInt16(strCommand[j, 2]), Convert.ToInt16(strCommand[j, 3]), Convert.ToInt16(strCommand[j, 4]), (int)Convert.ToDouble(strCommand[j, 5]));
                                    Touch(FreeTouch.SWIPE);
                                    break;
                                case "按键":
                                    keyCode = strCommand[j, 6];
                                    Touch(FreeTouch.KEYEVENT);
                                    break;
                                case "指令":
                                    command = strCommand[j, 7].Trim();
                                    Touch(FreeTouch.COMMAND);
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("请检查输入的参数是否有误！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return false;
                        }
                    }

                    int count = loop - i - 1;
                    if (count == 0)
                    {
                        countInfo = "";
                    }
                    else
                    {
                        countInfo = Convert.ToString(count);
                    }

                    if (form1.textBox1.IsHandleCreated)
                    {
                        form1.textBox1.Invoke((ControlDelegate)delegate
                        {
                            form1.textBox1.Text = countInfo;
                        });
                    }
                }
            }
            return true;
        }

        public void Close()
        {
            adb.Close();
        }

        public Image GetScreenshot()
        {
            if (File.Exists(@"data\compressed.jpg"))
            {
                File.Delete(@"data\compressed.jpg");
            }
            adb.Start();
            //截屏
            adb.Execute("shell screencap -p /mnt/sdcard/screenshot.jpg", 20);
            if (!Directory.Exists(@"data\"))
            {
                Directory.CreateDirectory(@"data\");
            }
            int i = 0;
            while (!File.Exists(@"data\screenshot.jpg"))
            {
                if (i > 5)
                {
                    break;
                }
                else
                {
                    //将截图拉到电脑
                    adb.Execute(@"pull /mnt/sdcard/screenshot.jpg data\", 20);
                    i++;
                }
            }
            adb.Execute("shell rm /mnt/sdcard/screenshot.jpg", 10);
            if (File.Exists(@"data\screenshot.jpg"))
            {
                Compress.CompressImage(@"data\screenshot.jpg", @"data\compressed.jpg");
                File.Delete(@"data\screenshot.jpg");
                Image image = Image.FromFile(@"data\compressed.jpg");
                Image newImage = new Bitmap(image);
                image.Dispose();
                return newImage;
            }
            else
            {
                return null;
            }
        }

        public int[] GetResolution()
        {
            int[] size = { 0, 0 };
            adb.Start();
            string screenSize = adb.ExecuteAndReturn("shell wm size", 0);
            string keyString = "Physical size:";
            int index = screenSize.IndexOf(keyString);
            if (index != -1)
            {
                screenSize = screenSize.Substring(index + keyString.Length);
                screenSize = screenSize.Remove(screenSize.IndexOf("\n"));
                string[] t = new string[2];
                t = screenSize.Split('x');
                size[0] = Convert.ToInt16(t[0]);
                size[1] = Convert.ToInt16(t[1]);
            }
            return size;
        }
    }
}
