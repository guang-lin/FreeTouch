using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace FreeTouch
{
    public partial class Form1 : Form
    {
        private FreeTouch touch;
        private delegate void ControlDelegate();
        private int ROWS = 50;
        private const int COLS = 9;
        private bool isFirstRun = true;
        private bool isLoadFinished = false;
        public int RowIndex { get; private set; } = 0;
        public int ColIndex { get; private set; } = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int with = 0;
            touch = new FreeTouch(this);
            dataGridView1.Rows.Add(ROWS);

            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = "";
                }
                dataGridView1.Rows[i].Cells[5].Value = "200";
                dataGridView1.Rows[i].Cells[COLS - 1].Value = "0.5";
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
            //调整控件宽度
            for (int i = 0; i < COLS; i++)
            {
                with += dataGridView1.Columns[i].Width;
            }
            dataGridView1.Width = with + dataGridView1.RowHeadersWidth + 10;
            this.Width = dataGridView1.Width + 13;

            label1.Text = "";
            isLoadFinished = true;
        }

        public FreeTouch GetFreeTouch()
        {
            return touch;
        }

        private void Execute()
        {
            string[,] cmdRows = new string[ROWS, COLS];
            int loop = 1;
            int codeRows = 0;

            if (textBox1.Text.Trim().Length != 0)
            {
                loop = Convert.ToInt16(textBox1.Text);
            }
            else
            {
                loop = 1;
            }

            for (int i = 0; i < ROWS; i++)//获取指令字符串数组
            {
                if (dataGridView1.Rows[i].Cells[0].Value.ToString().Trim().Length != 0)
                {
                    for (int j = 0; j < COLS; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value != null)
                        {
                            if (dataGridView1.Rows[i].Cells[j].Value.ToString().Trim().Length == 0)
                            {
                                if (j == 5)
                                {
                                    cmdRows[i, j] = "100";
                                }
                                else
                                if (j == 8)
                                {
                                    cmdRows[i, j] = "0";
                                }
                                else
                                {
                                    cmdRows[i, j] = "";
                                }
                            }
                            else
                            {
                                if (j == 6)
                                {
                                    cmdRows[i, j] = "KEYCODE_" + dataGridView1.Rows[i].Cells[j].Value.ToString();
                                }
                                else
                                {
                                    cmdRows[i, j] = dataGridView1.Rows[i].Cells[j].Value.ToString().Trim();
                                }
                            }
                        }
                        else
                        {
                            if (j == 5)
                            {
                                cmdRows[i, j] = "100";
                            }
                            else
                            if (j == 8)
                            {
                                cmdRows[i, j] = "0";
                            }
                            else
                            {
                                cmdRows[i, j] = "";
                            }
                        }
                    }
                }
                else
                {
                    codeRows = i;
                    break;
                }
            }

            try
            {
                if (button1.IsHandleCreated)
                {
                    button1.Invoke((ControlDelegate)delegate
                    {
                        button1.Text = "停止";
                        label1.ForeColor = Color.Red;
                        if (isFirstRun)
                        {
                            label1.Text = "正在初始化";
                        }
                        else
                        {
                            label1.Text = "正在运行";
                        }
                    });
                }

                if (isFirstRun)
                {
                    isFirstRun = false;
                    if (touch.Connect())
                    {
                        if (button1.IsHandleCreated)
                        {
                            button1.Invoke((ControlDelegate)delegate
                            {
                                label1.ForeColor = Color.Red;
                                label1.Text = "正在运行";
                            });
                        }
                    }
                    else
                    {
                        if (button1.IsHandleCreated)
                        {
                            button1.Invoke((ControlDelegate)delegate
                            {
                                label1.ForeColor = Color.Red;
                                label1.Text = "连接失败！";
                                button1.Text = "运行";
                            });
                        }
                        return;
                    }
                }

                touch.Run(cmdRows, codeRows, loop);//运行

                if (label1.IsHandleCreated)
                {
                    label1.Invoke((ControlDelegate)delegate
                    {
                        label1.ForeColor = Color.Red;
                        label1.Text = "运行结束";
                        button1.Text = "运行";
                    });
                }
            }
            catch(System.ComponentModel.InvalidAsynchronousStateException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)//运行
        {
            if (dataGridView1.Rows[0].Cells[0].Value.ToString().Trim().Length == 0)
            {
                return;
            }

            if (button1.Text == "运行")
            {
                ThreadStart threadStart = new ThreadStart(Execute);
                Thread thread = new Thread(threadStart);//创建线程
                thread.Start();
            }
            else
            {
                touch.IsRun = false;
                button1.Text = "运行";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = "";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string info = "  " + Properties.Resources.StrCommentPart1 + "\n  " + Properties.Resources.StrCommentPart2 + "\n\n  " +
                Properties.Resources.StrVersion + "\n  " +
                Properties.Resources.StrCopyright;
            MessageBox.Show(info, "关于", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            RowIndex = e.RowIndex;
            ColIndex = e.ColumnIndex;

            if (RowIndex > -1)
            {
                if (ColIndex > 0 && ColIndex < 3)
                {
                    SetPointForm pointForm = new SetPointForm(this);//屏幕位置选择窗体对象
                    try
                    {
                        int x = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[1].Value);
                        int y = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[2].Value);
                        pointForm.SetCurrentPoint(x, y);
                        pointForm.ShowDialog();
                    }
                    catch
                    {
                        pointForm.ShowDialog();
                    }
                }
                else
                if (ColIndex > 2 && ColIndex < 5)
                {
                    SetPointForm pointForm = new SetPointForm(this);
                    try
                    {
                        int x = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[3].Value);
                        int y = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[4].Value);
                        pointForm.SetCurrentPoint(x, y);
                        pointForm.ShowDialog();
                    }
                    catch
                    {
                        pointForm.ShowDialog();
                    }
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if(RowIndex > -1 && ColIndex > -1)
                dataGridView1.Rows[RowIndex].Cells[ColIndex].Value = "";
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add(1);
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            RowIndex = e.RowIndex;
            ColIndex = e.ColumnIndex;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string content = "";
            for (int i = 0; i < ROWS; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value.ToString().Trim().Length != 0)
                {
                    for (int j = 0; j < COLS; j++)
                    {
                        content += dataGridView1.Rows[i].Cells[j].Value.ToString() + "%";
                    }
                }
                else
                {
                    break;
                }
            }

            if (!Directory.Exists(@"data\"))
            {
                Directory.CreateDirectory(@"data\");
            }

            if (FileManip.WriteFile(@"data\script.txt", content))
            {
                label1.ForeColor = Color.Green;
                label1.Text = "已保存";
            }
            else
            {
                label1.ForeColor = Color.Red;
                label1.Text = "保存失败";
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            label1.Text = "";
            if(e.RowIndex > -1 && e.ColumnIndex == 5 || e.RowIndex > -1 && e.ColumnIndex == 8)
            {
                if(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    if(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Trim().Length != 0)
                    {
                        double duration = 0;
                        try
                        {
                            duration = Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                            if (e.ColumnIndex == 5)
                            {
                                if ((int)duration < 1 || duration > 1000 * 60)
                                {
                                    MessageBox.Show("输入的数值范围无效！有效范围：1 - 60,000", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                                }
                            }
                            else
                            if(e.ColumnIndex == 8)
                            {
                                if (duration < 0 || duration > 60 * 60 * 12)
                                {
                                    MessageBox.Show("输入的数值范围无效！有效范围：0 - 43,200", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                                }
                            }
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("输入的数值格式有误！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                        }
                        catch (OverflowException)
                        {
                            if (e.ColumnIndex == 5)
                            {
                                MessageBox.Show("输入的数值范围无效！有效范围：1 - 60,000", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                            }
                            else
                            if (e.ColumnIndex == 8)
                            {
                                MessageBox.Show("输入的数值范围无效！有效范围：0 - 43,200", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                            }
                        }
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string content = FileManip.ReadFile(@"data\script.txt");
            if (content == null || content.Length == 0)
            {
                MessageBox.Show("没有已保存的脚本", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string cell = "";
            button2_Click(sender, e);
            int row = 0, col = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if(row > ROWS - 1)
                {
                    dataGridView1.Rows.Add(1);
                }
                cell += content[i];
                if (content[i] == '%')
                {
                    if (content[i - 1] == '%')
                    {
                        dataGridView1.Rows[row].Cells[col].Value = "";
                    }
                    else
                    {
                        dataGridView1.Rows[row].Cells[col].Value = cell.Substring(0, cell.Length - 1);
                    }
                    cell = "";
                    col++;
                    if (col == COLS)
                    {
                        col = 0;
                        row++;
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            touch.IsRun = false;
            touch.Close();
        }

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            int with = 0;
            for (int i = 0; i < COLS; i++)
            {
                with += dataGridView1.Columns[i].Width;
            }
            dataGridView1.Width = with + dataGridView1.RowHeadersWidth + 10;
            this.Width = dataGridView1.Width + 15;
        }

        private void dataGridView1_RowHeadersWidthChanged(object sender, EventArgs e)
        {
            int with = 0;
            for (int i = 0; i < COLS; i++)
            {
                with += dataGridView1.Columns[i].Width;
            }
            dataGridView1.Width = with + dataGridView1.RowHeadersWidth + 10;
            this.Width = dataGridView1.Width + 13;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length != 0)
            {
                if (Regex.IsMatch(textBox1.Text.Trim(), @"^[+-]?\d*$"))
                {
                    try
                    {
                        if (Convert.ToInt16(textBox1.Text.Trim()) < 1)
                        {
                            MessageBox.Show("输入的重复次数不正确！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            textBox1.Text = "";
                            return;
                        }
                    }
                    catch(OverflowException)
                    {
                        MessageBox.Show("输入的重复次数过大！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        textBox1.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("输入的重复次数格式有误！应输入整数", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    textBox1.Text = "";
                    return;
                }
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("脚本内容有误！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            button2_Click(sender, e);
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (isLoadFinished)
            {
                dataGridView1.Rows[ROWS].HeaderCell.Value = (ROWS + 1).ToString();
                ROWS++;
            }
        }
    }
}
