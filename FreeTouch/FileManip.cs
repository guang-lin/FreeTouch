using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FreeTouch
{
    class FileManip
    {
        /// <summary>
        /// 将字符串内容写入文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">要写入的内容</param>
        /// <returns>写入成功返回 true，否则返回 false</returns>
        public static bool WriteFile(string path, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                    streamWriter.Write(content);
                    streamWriter.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static string ReadFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                    StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
                    string content = streamReader.ReadToEnd();
                    streamReader.Close();
                    fileStream.Close();
                    return content;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
