using System;
using System.IO;
using System.Windows.Forms;

namespace FreeTouch
{
    class ScriptFileDialog
    {
        public string ReadScript()
        {
            if (!Directory.Exists(@"data\"))
            {
                Directory.CreateDirectory(@"data\");
            }

            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "data\\";
                fileDialog.Filter = "*.txt|*.txt";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(fileDialog.FileName))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return "";
        }

        public void SaveScript(string content)
        {
            using (SaveFileDialog fileDialog = new SaveFileDialog())
            {
                fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "data\\";
                fileDialog.Filter = "*.txt|*.txt";
                fileDialog.FileName = "script.txt";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(fileDialog.FileName))
                        {
                            writer.WriteAsync(content);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
