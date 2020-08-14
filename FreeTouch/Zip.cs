using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace FreeTouch
{
    class Zip
    {
        /*
         * 对文件进行压缩与解压（ZIP）
         * 需要引用第三方类库ICSharpCode.SharpZipLib.dll
         */

        public static void Compressed(string filesPath, string zipFilePath)
        {
            if (!Directory.Exists(filesPath))
            {
                Console.WriteLine("Cannot find directory '{0}'", filesPath);
                return;
            }

            try
            {
                string[] filenames = Directory.GetFiles(filesPath);
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
                {
                    s.SetLevel(9); // 压缩级别 0-9
                    //s.Password = "xxxx"; // 设置ZIP压缩文件密码
                    byte[] buffer = new byte[4096]; // 缓冲区大小
                    foreach (string file in filenames)
                    {
                        ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }
                    s.Finish();
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during processing {0}", ex);
            }
        }

        public static void Extract(string zipFilePath, string outFilesPath)
        {
            if (!File.Exists(zipFilePath))
            {
                Console.WriteLine("Cannot find file '{0}'", zipFilePath);
                return;
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    if (!Directory.Exists(outFilesPath))
                    {
                        Directory.CreateDirectory(outFilesPath);
                    }

                    // Create child directory
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(outFilesPath + "\\" + directoryName);
                    }

                    if (fileName != String.Empty)
                    {
                        //Console.WriteLine("Absolute Entry Name: " + outFilesPath + "\\" + theEntry.Name);
                        using (FileStream streamWriter = File.Create(outFilesPath + "\\" + theEntry.Name))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
