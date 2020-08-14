using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FreeTouch
{
    class Parse
    {
        public ArrayList GetArray(DataGridView dataGridView) //从DataGridView获取指令数组
        {
            ArrayList cmd = new ArrayList();

            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                if (dataGridView.Rows[i].Cells[0].Value.ToString().Trim().Length != 0)
                {
                    string[] row = new string[dataGridView.ColumnCount];
                    for (int j = 0; j < dataGridView.ColumnCount; j++)
                    {
                        if (dataGridView.Rows[i].Cells[j].Value != null)
                        {
                            if (dataGridView.Rows[i].Cells[j].Value.ToString().Trim().Length == 0)
                            {
                                if (j == Table.TIME)
                                {
                                    row[j] = "200";
                                }
                                else
                                if (j == Table.WAIT)
                                {
                                    row[j] = "0.5";
                                }
                                else
                                {
                                    row[j] = "";
                                }
                            }
                            else
                            {
                                row[j] = dataGridView.Rows[i].Cells[j].Value.ToString().Trim();
                            }
                        }
                        else
                        {
                            if (j == Table.TIME)
                            {
                                row[j] = "200";
                            }
                            else
                            if (j == Table.WAIT)
                            {
                                row[j] = "0.5";
                            }
                            else
                            {
                                row[j] = "";
                            }
                        }
                    }
                    cmd.Add(row);
                }
                else
                {
                    break;
                }
            }
            return cmd;
        }

        public void SetArray(ArrayList cmd, DataGridView dataGridView)
        {
            for (int i = 0; i < cmd.Count; i++)
            {
                string[] row = (string[])cmd[i];
                for (int j = 0; j < row.Length; j++)
                {
                    if (i >= dataGridView.RowCount)
                    {
                        dataGridView.Rows.Add(1);
                    }
                    dataGridView.Rows[i].Cells[j].Value = row[j];
                }
            }
        }

        public ArrayList ParseToArray(string script, int columnCount)
        {
            ArrayList cmd = new ArrayList();
            string[] rows = script.Split('\n');

            for (int i = 0; i < rows.Length; i++)
            {
                string[] row = new string[columnCount];
                for (int j = 0; j < columnCount; j++)
                {
                    row[j] = "";
                }
                if (Regex.IsMatch(rows[i], @"^\s*click.*"))
                {
                    Match match = Regex.Match(rows[i], @"\(.*\)");
                    string[] param = match.ToString().Substring(1, match.Length - 2).Split(',');
                    row[Table.EVENT] = "点击";
                    if (param.Length == 2)
                    {
                        row[Table.X1] = param[0].Trim();
                        row[Table.Y1] = param[1].Trim();
                    }
                    else
                    if(param.Length == 3)
                    {
                        row[Table.TEXT] = param[0].Trim().Substring(1, param[0].Trim().Length - 2);
                        row[Table.X1] = param[1].Trim();
                        row[Table.Y1] = param[2].Trim();
                    }
                }
                else
                if (Regex.IsMatch(rows[i], @"^\s*swipe.*"))
                {
                    Match match = Regex.Match(rows[i], @"\(.*\)");
                    string[] param = match.ToString().Substring(1, match.Length - 2).Split(',');
                    row[Table.EVENT] = "划动";
                    row[Table.X1] = param[Table.X1 - 1].Trim();
                    row[Table.Y1] = param[Table.Y1 - 1].Trim();
                    row[Table.X2] = param[Table.X2 - 1].Trim();
                    row[Table.Y2] = param[Table.Y2 - 1].Trim();
                    if (param.Length == 5)
                    {
                        row[Table.TIME] = param[4].Trim();
                    }
                }
                else
                if (Regex.IsMatch(rows[i], @"^\s*key.*"))
                {
                    Match match = Regex.Match(rows[i], "\".*\"");
                    row[Table.EVENT] = "按键";
                    row[Table.KEY_CODE] = match.ToString().Trim('"');
                }
                else
                if (Regex.IsMatch(rows[i], @"^\s*adb.*"))
                {
                    Match match = Regex.Match(rows[i], "\".*\"");
                    row[Table.EVENT] = "指令";
                    row[Table.COMMAND] = match.ToString().Trim('"');
                }
                else
                {
                    break;
                }
                int index = rows[i].IndexOf("wait");
                if (index != -1)
                {
                    for (int j = index + 4; j < rows[i].Length; j++)
                    {
                        if (rows[i][j] == '=')
                        {
                            row[Table.WAIT] = rows[i].Substring(j + 1).Trim();
                            break;
                        }
                    }
                }
                cmd.Add(row);
            }
            return cmd;
        }

        public string ParseToScript(ArrayList cmd)
        {
            string script = "";
            for (int i = 0; i < cmd.Count; i++)
            {
                string[] row = (string[])cmd[i];
                if (row[0].Length != 0)
                {
                    switch (row[Table.EVENT])
                    {
                        case "点击":
                            script += "click(";
                            if (row[Table.TEXT].Length != 0)
                            {
                                script += "\"" + row[Table.TEXT] + "\"";
                                if (row[Table.X1].Length != 0)
                                {
                                    if (row[Table.X1][0] == '+' || row[Table.X1][0] == '-')
                                    {
                                        script += ", " + row[Table.X1];
                                        script += ", " + row[Table.Y1];
                                    }
                                }
                            }
                            else
                            {
                                script += row[Table.X1];
                                script += ", " + row[Table.Y1];
                            }
                            script += ")";
                            break;
                        case "划动":
                            script += "swipe(";
                            for (int j = Table.X1; j <= Table.Y2; j++)
                            {
                                script += row[j];
                                if (j != 4)
                                {
                                    script += ", ";
                                }
                            }
                            if (row[Table.TIME].Length != 0)
                            {
                                script += ", " + row[Table.TIME];
                            }
                            script += ")";
                            break;
                        case "按键":
                            script += "key(";
                            script += "\"" + row[Table.KEY_CODE] + "\")";
                            break;
                        case "指令":
                            script += "adb(\"";
                            script += row[Table.COMMAND] + "\")";
                            break;
                        default: break;
                    }
                    if (row[Table.WAIT].Length != 0)
                    {
                        script += " wait = " + row[Table.WAIT];
                    }
                    script += "\n";
                }
                else
                {
                    break;
                }
            }
            return script;
        }
    }
}
