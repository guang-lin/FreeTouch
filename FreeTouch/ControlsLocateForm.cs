using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FreeTouch
{
    public partial class ControlsLocateForm : Form
    {
        private FreeTouch touch;
        private Form1 form1;
        private int x = -1;
        private int y = -1;
        private int controlX = 0;
        private int controlY = 0;
        private int offsetX = 0;
        private int offsetY = 0;
        private int ratio = 4;
        private int[] resolution = new int[2];

        public ControlsLocateForm(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;
            touch = form1.GetFreeTouch();
        }

        private void ControlsLocateForm_Load(object sender, EventArgs e)
        {
            form1.Cursor = Cursors.WaitCursor;
            resolution = touch.GetResolution();
            if (resolution[0] != 0)
            {
                ratio = 1;
                while (resolution[0] / ratio > 200)
                {
                    ratio++;
                }
                ratio--;
                pictureBox1.Width = resolution[0] / ratio;
                pictureBox1.Height = resolution[1] / ratio;
                this.Height = pictureBox1.Height + 50;
                this.Width = pictureBox1.Width + groupBox1.Width + 50;
                pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                label4.Text = "";
                RefreshScreenshot();
                SetOffset(offsetX, offsetY);
                form1.Cursor = Cursors.Default;
            }
            else
            {
                form1.Cursor = Cursors.Default;
                MessageBox.Show("未能正确连接手机！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
        }

        public void SetOffset(int offsetX, int offsetY)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;

            if (offsetX >= 0)
            {
                textBox1.Text = "+" + offsetX.ToString();
            }
            else
            {
                textBox1.Text = offsetX.ToString();
            }

            if (offsetY >= 0)
            {
                textBox2.Text = "+" + offsetY.ToString();
            }
            else
            {
                textBox2.Text = offsetY.ToString();
            }
        }

        public void SetControlText(string controlText)
        {
            if (controlText.Trim().Length != 0)
            {
                textBox3.Text = controlText;
            }
        }

        private void DrawSymbol()
        {
            if (x != -1 && y != -1)
            {
                Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics g = Graphics.FromImage(bitmap);
                Pen pen = new Pen(Color.Red, 2);
                g.DrawLine(pen, x, y, x + 8, y);
                g.DrawLine(pen, x, y, x - 8, y);
                g.DrawLine(pen, x, y, x, y + 8);
                g.DrawLine(pen, x, y, x, y - 8);
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }
                pictureBox1.Image = bitmap;
            }
        }

        private void ClearSymbol()
        {
            pictureBox1.Image = null;
        }

        private void RefreshScreenshot()
        {
            if (pictureBox1.BackgroundImage != null)
            {
                pictureBox1.BackgroundImage.Dispose();
            }
            pictureBox1.BackgroundImage = touch.GetScreenshot();
        }

        private void button1_Click(object sender, EventArgs e) //确定
        {
            x = (controlX + offsetX) / ratio;
            y = (controlY + offsetY) / ratio;
            DrawSymbol();
        }

        private void button2_Click(object sender, EventArgs e) //完成
        {
            if (textBox3.Text.Trim().Length != 0)
            {
                form1.dataGridView1.Rows[form1.RowIndex].Cells[Table.X1].Value = textBox1.Text.Trim();
                form1.dataGridView1.Rows[form1.RowIndex].Cells[Table.Y1].Value = textBox2.Text.Trim();
                form1.dataGridView1.Rows[form1.RowIndex].Cells[Table.TEXT].Value = textBox3.Text.Trim();
            }
            else
            {
                form1.dataGridView1.Rows[form1.RowIndex].Cells[Table.X1].Value = "";
                form1.dataGridView1.Rows[form1.RowIndex].Cells[Table.Y1].Value = "";
                form1.dataGridView1.Rows[form1.RowIndex].Cells[Table.TEXT].Value = "";
            }
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e) //定位
        {
            if (textBox3.Text.Trim().Length != 0)
            {
                this.Cursor = Cursors.WaitCursor;
                label4.Text = "定位中•••";
                form1.GetFreeTouch().PullWindowDump();
                if (File.Exists(Properties.Resources.WindowDumpFile))
                {
                    Util util = new Util();
                    string xml = util.ReadTextFile(Properties.Resources.WindowDumpFile);
                    int[] position = util.GetPositionFromXml(xml, textBox3.Text.Trim());
                    File.Delete(Properties.Resources.WindowDumpFile);
                    if (position[0] != -1)
                    {
                        controlX = position[0];
                        controlY = position[1];
                        if (position[0] + offsetX <= 0)
                        {
                            x = 0;
                        }
                        else
                        if (position[0] + offsetX >= resolution[0])
                        {
                            x = resolution[0] / ratio;
                        }
                        else
                        {
                            x = (position[0] + offsetX) / ratio;
                        }

                        if (position[1] + offsetY <= 0)
                        {
                            y = 0;
                        }
                        else
                        if (position[1] + offsetY >= resolution[1])
                        {
                            y = resolution[1] / ratio;
                        }
                        else
                        {
                            y = (position[1] + offsetY) / ratio;
                        }

                        RefreshScreenshot();
                        DrawSymbol();
                        this.Cursor = Cursors.Default;
                        label4.Text = "定位完成";
                    }
                    else
                    {
                        RefreshScreenshot();
                        this.Cursor = Cursors.Default;
                        label4.Text = "";
                        MessageBox.Show("没有在当前界面找到显示该内容的控件！", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    label4.Text = "";
                    MessageBox.Show("无法获取界面内容！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                this.Cursor = Cursors.Default;
                label4.Text = "";
                MessageBox.Show("需要输入控件的显示内容！", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e) //取消
        {
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Point point = pictureBox1.PointToClient(Control.MousePosition);
            x = point.X;
            y = point.Y;
            DrawSymbol();
            SetOffset(x * ratio - controlX, y * ratio - controlY);
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length != 0)
            {
                if (Regex.IsMatch(textBox1.Text.Trim(), @"^[+-]?\d*$"))
                {
                    try
                    {
                        if (controlX + Convert.ToInt16(textBox1.Text.Trim()) < 0)
                        {
                            x = 0;
                            offsetX = x * ratio - controlX;
                            DrawSymbol();
                        }
                        else
                        if (controlX + Convert.ToInt16(textBox1.Text.Trim()) > resolution[0])
                        {
                            x = resolution[0] / ratio;
                            offsetX = x * ratio - controlX;
                            DrawSymbol();
                        }
                        else
                        {
                            offsetX = Convert.ToInt16(textBox1.Text.Trim());
                            SetOffset(offsetX, offsetY);
                        }
                    }
                    catch (OverflowException)
                    {
                        textBox1.Text = "+0";
                        SetOffset(0, offsetY);
                    }
                }
                else
                {
                    MessageBox.Show("请输入整数！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    textBox1.Text = "+0";
                    SetOffset(0, offsetY);
                }
            }
            else
            {
                textBox1.Text = "+0";
                SetOffset(0, offsetY);
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text.Trim().Length != 0)
            {
                if (Regex.IsMatch(textBox2.Text.Trim(), @"^[+-]?\d*$"))
                {
                    try
                    {
                        if (controlY + Convert.ToInt16(textBox2.Text.Trim()) < 0)
                        {
                            y = 0;
                            offsetY = y * ratio - controlY;
                            DrawSymbol();
                        }
                        else
                        if (controlY + Convert.ToInt16(textBox2.Text.Trim()) > resolution[1])
                        {
                            y = resolution[1] / ratio;
                            offsetY = y * ratio - controlY;
                            DrawSymbol();
                        }
                        else
                        {
                            offsetY = Convert.ToInt16(textBox2.Text.Trim());
                            SetOffset(offsetX, offsetY);
                        }
                    }
                    catch (OverflowException)
                    {
                        textBox2.Text = "+0";
                        SetOffset(offsetX, 0);
                    }
                }
                else
                {
                    MessageBox.Show("请输入整数！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    textBox2.Text = "+0";
                    SetOffset(offsetX, 0);
                }
            }
            else
            {
                textBox2.Text = "+0";
                SetOffset(offsetX, 0);
            }
        }
    }
}