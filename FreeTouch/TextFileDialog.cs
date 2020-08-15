using System;
using System.IO;
using System.Windows.Forms;

namespace FreeTouch
{
    class TextFileDialog
    {
        public string ReadText(string directory)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.InitialDirectory = directory;
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

        public void SaveText(string content, string directory, string name)
        {
            using (SaveFileDialog fileDialog = new SaveFileDialog())
            {
                fileDialog.InitialDirectory = directory;
                fileDialog.Filter = "*.txt|*.txt";
                fileDialog.FileName = name;
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
