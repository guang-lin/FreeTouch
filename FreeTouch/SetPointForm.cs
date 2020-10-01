using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FreeTouch
{
    public partial class SetPointForm : Form
    {
        private FreeTouch touch;
        private Form1 form1;
        private int x = -1;
        private int y = -1;
        private int ratio = 4;
        private int[] resolution = new int[2];

        public SetPointForm(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;
            touch = form1.GetFreeTouch();
        }

        private void SetPointForm_Load(object sender, EventArgs e)
        {
            form1.Cursor = Cursors.WaitCursor;
            resolution = touch.GetResolution(); //获取屏幕分辨率
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
                label3.Text = "";
                RefreshScreenshot();
                DrawSymbol();
                if (x != -1)
                {
                    textBox1.Text = (x * ratio).ToString();
                    textBox2.Text = (y * ratio).ToString();
                }
                form1.Cursor = Cursors.Default;
            }
            else
            {
                form1.Cursor = Cursors.Default;
                MessageBox.Show("未能正确连接手机！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        public void SetCurrentPoint(int x, int y)
        {
            this.x = x / ratio;
            this.y = y / ratio;
        }

        private void RefreshScreenshot()
        {
            if (pictureBox1.BackgroundImage != null)
            {
                pictureBox1.BackgroundImage.Dispose(); //释放资源
            }
            pictureBox1.BackgroundImage = touch.GetScreenshot();
        }

        private void DrawSymbol()
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

        private void ClearSymbol()
        {
            pictureBox1.Image = null;
        }

        private void button1_Click(object sender, EventArgs e) //确定
        {
            if (textBox1.Text.Trim().Length != 0 && textBox2.Text.Trim().Length != 0)
            {
                if (form1.ColIndex == Table.X1 || form1.ColIndex == Table.Y1)
                {
                    form1.dataGridView1.Rows[form1.RowIndex].Cells[1].Value = textBox1.Text.Trim();
                    form1.dataGridView1.Rows[form1.RowIndex].Cells[2].Value = textBox2.Text.Trim();
                }
                if (form1.ColIndex == Table.X2 || form1.ColIndex == Table.Y2)
                {
                    form1.dataGridView1.Rows[form1.RowIndex].Cells[3].Value = textBox1.Text.Trim();
                    form1.dataGridView1.Rows[form1.RowIndex].Cells[4].Value = textBox2.Text.Trim();
                }
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e) //刷新
        {
            this.Cursor = Cursors.WaitCursor;
            label3.Text = "加载中•••";
            RefreshScreenshot();
            this.Cursor = Cursors.Default;
            label3.Text = "加载完成";
        }

        private void button3_Click(object sender, EventArgs e) //取消
        {
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Point point = pictureBox1.PointToClient(Control.MousePosition);
            x = point.X;
            y = point.Y;
            DrawSymbol();
            textBox1.Text = (x * ratio).ToString();
            textBox2.Text = (y * ratio).ToString();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length != 0)
            {
                if (Regex.IsMatch(textBox1.Text.Trim(), @"^[+-]?\d*$"))
                {
                    try
                    {
                        if (Convert.ToInt16(textBox1.Text.Trim()) < 0 || Convert.ToInt16(textBox1.Text.Trim()) > resolution[0])
                        {
                            MessageBox.Show("有效范围：0 - " + resolution[0], "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            textBox1.Text = "";
                        }
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("有效范围：0 - " + resolution[0], "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        textBox1.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("请输入整数！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    textBox1.Text = "";
                }
            }
            else
            {
                textBox1.Text = "";
            }

            if (textBox1.Text.Trim().Length != 0 && textBox2.Text.Trim().Length != 0)
            {
                x = Convert.ToInt16(textBox1.Text) / ratio;
                y = Convert.ToInt16(textBox2.Text) / ratio;
                DrawSymbol();
            }
            else
            {
                ClearSymbol();
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
                        if (Convert.ToInt16(textBox2.Text.Trim()) < 0 || Convert.ToInt16(textBox2.Text.Trim()) > resolution[1])
                        {
                            MessageBox.Show("有效范围：0 - " + resolution[1], "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            textBox2.Text = "";
                        }
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("有效范围：0 - " + resolution[1], "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        textBox2.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("请输入整数！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    textBox2.Text = "";
                }
            }
            else
            {
                textBox2.Text = "";
            }

            if (textBox1.Text.Trim().Length != 0 && textBox2.Text.Trim().Length != 0)
            {
                x = Convert.ToInt16(textBox1.Text) / ratio;
                y = Convert.ToInt16(textBox2.Text) / ratio;
                DrawSymbol();
            }
            else
            {
                ClearSymbol();
            }
        }
    }
}