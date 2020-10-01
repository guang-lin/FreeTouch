using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace FreeTouch
{
    public partial class Form1 : Form
    {
        AdbProcess adb = new AdbProcess();
        private FreeTouch touch;
        private delegate void ControlDelegate();
        private bool isFirstRun = true;
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
            dataGridView1.Rows.Add(Table.ROW_COUNT);
            //调整控件宽度
            for (int i = 0; i < Table.COLUMN_COUNT; i++)
            {
                with += dataGridView1.Columns[i].Width;
            }
            dataGridView1.Width = with + dataGridView1.RowHeadersWidth + 10;
            Width = dataGridView1.Width + 13;
            label1.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            touch.IsRun = false;
            touch.Close();
        }

        public FreeTouch GetFreeTouch()
        {
            return touch;
        }

        private void Execute()
        {
            int loop = 1;
            Parse parse = new Parse();
            ArrayList cmd = parse.GetArray(dataGridView1);

            if (textBox1.Text.Trim().Length != 0)
            {
                loop = Convert.ToInt16(textBox1.Text);
            }

            try
            {
                if (button1.IsHandleCreated)
                {
                    button1.Invoke((ControlDelegate)delegate
                    {
                        button1.Text = "停止";
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
                                label1.Text = "连接失败！";
                                button1.Text = "运行";
                            });
                        }
                        return;
                    }
                }
                touch.Run(cmd, cmd.Count, loop); //运行

                if (label1.IsHandleCreated)
                {
                    label1.Invoke((ControlDelegate)delegate
                    {
                        label1.Text = "运行结束";
                        button1.Text = "运行";
                    });
                }
            }
            catch (System.ComponentModel.InvalidAsynchronousStateException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private string GetCellValue(int row, int col)
        {
            if (dataGridView1.Rows[row].Cells[col].Value == null)
            {
                return "";
            }
            else
            {
                return dataGridView1.Rows[row].Cells[col].Value.ToString();
            }
        }

        private void ClearDataGridView(DataGridView dataGridView) //清空
        {
            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                for (int j = 0; j < dataGridView.ColumnCount; j++)
                {
                    dataGridView.Rows[i].Cells[j].Value = "";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) //运行
        {
            if (dataGridView1.Rows[0].Cells[0].Value.ToString().Trim().Length == 0)
            {
                return;
            }

            if (button1.Text == "运行")
            {
                ThreadStart threadStart = new ThreadStart(Execute);
                Thread thread = new Thread(threadStart);
                thread.Start();
            }
            else
            {
                touch.IsRun = false;
                button1.Text = "运行";
            }
        }

        private void button3_Click(object sender, EventArgs e) //关于
        {
            AboutForm messageForm = new AboutForm();
            messageForm.StartPosition = FormStartPosition.CenterParent;
            messageForm.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e) //读取脚本
        {
            TextFileDialog fileDialog = new TextFileDialog();
            Parse parse = new Parse();
            string directory = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.ScriptsDirectory;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string script = fileDialog.ReadText(directory);
            if (script.Length == 0)
            {
                return;
            }
            ArrayList cmd = parse.ParseToArray(script, Table.COLUMN_COUNT);
            ClearDataGridView(dataGridView1);
            parse.SetArray(cmd, dataGridView1);
        }

        private void button5_Click(object sender, EventArgs e) //保存脚本
        {
            if (dataGridView1.Rows[0].Cells[0].Value.ToString().Trim().Length == 0)
            {
                return;
            }
            else
            {
                TextFileDialog fileDialog = new TextFileDialog();
                Parse parse = new Parse();
                string script = parse.ParseToScript(parse.GetArray(dataGridView1));
                if (!Directory.Exists(Properties.Resources.ScriptsDirectory))
                {
                    Directory.CreateDirectory(Properties.Resources.ScriptsDirectory);
                }
                string directory = AppDomain.CurrentDomain.BaseDirectory + Properties.Resources.ScriptsDirectory;
                fileDialog.SaveText(script, directory, "");
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            RowIndex = e.RowIndex;
            ColIndex = e.ColumnIndex;

            if (RowIndex > -1)
            {
                if (ColIndex > Table.EVENT && ColIndex < Table.X2)
                {
                    SetPointForm pointForm = new SetPointForm(this); //创建用于选取手机屏幕坐标的窗体对象
                    try
                    {
                        int x = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[Table.X1].Value);
                        int y = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[Table.Y1].Value);
                        pointForm.SetCurrentPoint(x, y);
                        pointForm.StartPosition = FormStartPosition.CenterParent;
                        pointForm.ShowDialog();
                    }
                    catch
                    {
                        pointForm.StartPosition = FormStartPosition.CenterParent;
                        pointForm.ShowDialog();
                    }
                }
                else
                if (ColIndex > Table.Y1 && ColIndex < Table.TEXT)
                {
                    SetPointForm pointForm = new SetPointForm(this);
                    try
                    {
                        int x = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[Table.X2].Value);
                        int y = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[Table.Y2].Value);
                        pointForm.SetCurrentPoint(x, y);
                        pointForm.StartPosition = FormStartPosition.CenterParent;
                        pointForm.ShowDialog();
                    }
                    catch
                    {
                        pointForm.StartPosition = FormStartPosition.CenterParent;
                        pointForm.ShowDialog();
                    }
                }
                else
                if (ColIndex == Table.TEXT)
                {
                    ControlsLocateForm locateForm = new ControlsLocateForm(this);
                    try
                    {
                        if (dataGridView1.Rows[RowIndex].Cells[Table.X1].Value.ToString().Length != 0)
                        {
                            char sign = dataGridView1.Rows[RowIndex].Cells[Table.X1].Value.ToString()[0];
                            if (sign == '+' || sign == '-')
                            {
                                int x = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[Table.X1].Value);
                                int y = Convert.ToInt16(dataGridView1.Rows[RowIndex].Cells[Table.Y1].Value);
                                locateForm.SetOffset(x, y);
                            }
                        }
                        locateForm.SetControlText(dataGridView1.Rows[RowIndex].Cells[Table.TEXT].Value.ToString());
                        locateForm.StartPosition = FormStartPosition.CenterParent;
                        locateForm.ShowDialog();
                    }
                    catch
                    {
                        locateForm.SetControlText(dataGridView1.Rows[RowIndex].Cells[Table.TEXT].Value.ToString());
                        locateForm.StartPosition = FormStartPosition.CenterParent;
                        locateForm.ShowDialog();
                    }
                }
            }
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            RowIndex = e.RowIndex;
            ColIndex = e.ColumnIndex;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            label1.Text = "";
            if (e.RowIndex > -1 && e.ColumnIndex == Table.TIME || e.RowIndex > -1 && e.ColumnIndex == Table.WAIT)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Trim().Length != 0)
                    {
                        double duration = 0;
                        try
                        {
                            duration = Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                            if (e.ColumnIndex == Table.TEXT)
                            {
                                if ((int)duration < 1 || duration > 1000 * 60)
                                {
                                    MessageBox.Show("数值范围无效！有效范围：1 - 60,000", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                                }
                            }
                            else
                            if (e.ColumnIndex == Table.WAIT)
                            {
                                if (duration < 0 || duration > 60 * 60 * 12)
                                {
                                    MessageBox.Show("数值范围无效！有效范围：0 - 43,200", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                                }
                            }
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("数值格式有误！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                        }
                        catch (OverflowException)
                        {
                            if (e.ColumnIndex == Table.TIME)
                            {
                                MessageBox.Show("数值范围无效！有效范围：1 - 60,000", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                            }
                            else
                            if (e.ColumnIndex == Table.WAIT)
                            {
                                MessageBox.Show("数值范围无效！有效范围：0 - 43,200", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            int with = 0;
            for (int i = 0; i < Table.COLUMN_COUNT; i++)
            {
                with += dataGridView1.Columns[i].Width;
            }
            dataGridView1.Width = with + dataGridView1.RowHeadersWidth + 10;
            this.Width = dataGridView1.Width + 15;
        }

        private void dataGridView1_RowHeadersWidthChanged(object sender, EventArgs e)
        {
            int with = 0;
            for (int i = 0; i < Table.COLUMN_COUNT; i++)
            {
                with += dataGridView1.Columns[i].Width;
            }
            dataGridView1.Width = with + dataGridView1.RowHeadersWidth + 10;
            this.Width = dataGridView1.Width + 13;
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            int added = dataGridView1.Rows.Count - Table.ROW_COUNT;
            int start = Table.ROW_COUNT;
            int end = Table.ROW_COUNT + added;
            if(dataGridView1.Rows.Count == Table.ROW_COUNT)
            {
                start = 0;
                end = Table.ROW_COUNT;
            }

            for (int i = start; i < end; i++)
            {
                for (int j = 0; j < Table.COLUMN_COUNT; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = "";
                }
                dataGridView1.Rows[i].Cells[Table.TIME].Value = "200";
                dataGridView1.Rows[i].Cells[Table.COLUMN_COUNT - 1].Value = "0.5";
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
            Table.ROW_COUNT = dataGridView1.Rows.Count;
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("内容有误，无法正确解析！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ClearDataGridView(dataGridView1);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (RowIndex > -1 && ColIndex > -1)
                dataGridView1.Rows[RowIndex].Cells[ColIndex].Value = "";
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ClearDataGridView(dataGridView1);
        }

        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add(5);
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
                            MessageBox.Show("重复次数不正确！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            textBox1.Text = "";
                            return;
                        }
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("数值过大！", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        textBox1.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("重复次数格式有误！应输入整数", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    textBox1.Text = "";
                    return;
                }
            }
            else
            {
                textBox1.Text = "";
            }
        }
    }
}
