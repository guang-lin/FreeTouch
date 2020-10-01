using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FreeTouch
{
    public class FreeTouch
    {
        private Form1 form1;
        private delegate void ControlDelegate();
        private int x1 = 0, y1 = 0; //触摸点或划动起始点
        private int x2 = 0, y2 = 0; //划动终点
        private int duration = 100; //划动用时
        private string keyCode = "";
        private int delay = 0; //等待执行下一条命令的时长
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

        private void Touch(int mode) //该函数每被调用一次，则执行一条指令
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

        public bool Run(ArrayList cmd, int cmdRows, int loop = 1)
        {
            string countInfo = "";
            IsRun = true;
            adb.Start(); //启动adb程序

            for (int i = 0; i < loop; i++)
            {
                for (int j = 0; j < cmdRows; j++)
                {
                    if (!IsRun)
                    {
                        break;
                    }
                    else
                    {
                        if (form1.dataGridView1.IsHandleCreated)
                        {
                            form1.dataGridView1.Invoke((ControlDelegate)delegate
                            {
                                form1.dataGridView1.Rows[j].DefaultCellStyle.BackColor = Color.LightGreen;
                            });
                        }
                        try
                        {
                            string[] oneRow = (string[])cmd[j];
                            delay = Convert.ToInt16(Convert.ToDouble(oneRow[Table.WAIT]) * 1000);
                            switch (oneRow[Table.EVENT])
                            {
                                case "点击":
                                    if (oneRow[Table.TEXT].Length != 0)
                                    {
                                        Util util = new Util();
                                        PullWindowDump();
                                        if (File.Exists(Properties.Resources.WindowDumpFile))
                                        {
                                            string xml = util.ReadTextFile(Properties.Resources.WindowDumpFile);
                                            int[] position = util.GetPositionFromXml(xml, oneRow[Table.TEXT]);

                                            if (position[0] != -1)
                                            {
                                                if (oneRow[Table.X1].Length != 0)
                                                {
                                                    if (oneRow[Table.X1][0] == '+' || oneRow[Table.X1][0] == '-')
                                                    {
                                                        SetClickPoint(position[0] + Convert.ToInt16(oneRow[Table.X1]), position[1] + Convert.ToInt16(oneRow[Table.Y1]));
                                                    }
                                                    else
                                                    {
                                                        SetClickPoint(position[0], position[1]);
                                                    }
                                                }
                                                else
                                                {
                                                    SetClickPoint(position[0], position[1]);
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show("未能定位到指定控件，已停止运行", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                if (form1.dataGridView1.IsHandleCreated)
                                                {
                                                    form1.dataGridView1.Invoke((ControlDelegate)delegate
                                                    {
                                                        form1.dataGridView1.Rows[j].DefaultCellStyle.BackColor = Color.White;
                                                    });
                                                }
                                                IsRun = false;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SetClickPoint(Convert.ToInt16(oneRow[Table.X1]), Convert.ToInt16(oneRow[Table.Y1]));
                                    }
                                    Touch(FreeTouch.CLICK);
                                    break;
                                case "划动":
                                    SetSwipePoint(Convert.ToInt16(oneRow[Table.X1]), Convert.ToInt16(oneRow[Table.Y1]), Convert.ToInt16(oneRow[Table.X2]), Convert.ToInt16(oneRow[Table.Y2]), (int)Convert.ToDouble(oneRow[Table.TIME]));
                                    Touch(FreeTouch.SWIPE);
                                    break;
                                case "按键":
                                    keyCode = "KEYCODE_" + oneRow[Table.KEY_CODE];
                                    Touch(FreeTouch.KEYEVENT);
                                    break;
                                case "指令":
                                    command = oneRow[Table.COMMAND].Trim();
                                    Touch(FreeTouch.COMMAND);
                                    break;
                                default:
                                    break;
                            }
                            if (form1.dataGridView1.IsHandleCreated)
                            {
                                form1.dataGridView1.Invoke((ControlDelegate)delegate
                                {
                                    form1.dataGridView1.Rows[j].DefaultCellStyle.BackColor = Color.White;
                                });
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("请检查参数是否存在错误！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            if (form1.dataGridView1.IsHandleCreated)
                            {
                                form1.dataGridView1.Invoke((ControlDelegate)delegate
                                {
                                    form1.dataGridView1.Rows[j].DefaultCellStyle.BackColor = Color.White;
                                });
                            }
                            return false;
                        }
                    }

                    int count = loop - i - 1;
                    if (count <= 0)
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
            if (File.Exists(Properties.Resources.TempDirectory + "\\compressed.png"))
            {
                File.Delete(Properties.Resources.TempDirectory + "\\compressed.png");
            }
            adb.Start();
            // 截屏
            adb.Execute("shell screencap -p /mnt/sdcard/screenshot.png", 50);
            if (!Directory.Exists(Properties.Resources.TempDirectory + "\\"))
            {
                Directory.CreateDirectory(Properties.Resources.TempDirectory + "\\");
            }
            int i = 0;
            while (!File.Exists(Properties.Resources.TempDirectory + "\\screenshot.png"))
            {
                if (i < 5)
                {
                    //将截图拉到电脑
                    adb.Execute(@"pull /mnt/sdcard/screenshot.png " + Properties.Resources.TempDirectory, 200);
                    i++;
                }
                else
                {
                    break;
                }
            }
            adb.Execute("shell rm /mnt/sdcard/screenshot.png", 20);
            if (File.Exists(Properties.Resources.TempDirectory + "\\screenshot.png"))
            {
                Compress.CompressImage(Properties.Resources.TempDirectory + "\\screenshot.png", Properties.Resources.TempDirectory + "\\compressed.png");
                File.Delete(Properties.Resources.TempDirectory + "\\screenshot.png");
                Image image = Image.FromFile(Properties.Resources.TempDirectory + "\\compressed.png");
                Image newImage = new Bitmap(image);
                image.Dispose();
                if (File.Exists(Properties.Resources.TempDirectory + "\\compressed.png"))
                {
                    File.Delete(Properties.Resources.TempDirectory + "\\compressed.png");
                }
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

        public void PullWindowDump()
        {
            adb.Start();
            adb.Execute("shell uiautomator dump /mnt/sdcard/window_dump.xml", 50);
            if (!Directory.Exists(Properties.Resources.TempDirectory + "\\"))
            {
                Directory.CreateDirectory(Properties.Resources.TempDirectory);
            }
            int k = 0;
            while (!File.Exists(Properties.Resources.WindowDumpFile))
            {
                if (k < 5)
                {
                    adb.Execute(@"pull /mnt/sdcard/window_dump.xml " + Properties.Resources.TempDirectory + "\\", 100);
                    k++;
                }
                else
                {
                    break;
                }
            }
            adb.Execute("shell rm /mnt/sdcard/window_dump.xml", 20);
        }
    }
}
