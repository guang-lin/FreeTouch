using System;
using System.IO;

namespace FreeTouch
{
    class Util
    {
        public string ReadTextFile(string filePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string content = sr.ReadToEnd();
                    return content;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public int[] GetPositionFromXml(string xml, string text)
        {
            int offset = xml.IndexOf(text);
            int[] position = new int[2];
            position[0] = -1;
            position[1] = -1;
            int start;
            int end;

            if (offset < 0)
            {
                return position;
            }
            else
            {
                start = offset;
                end = offset;
            }

            for (int i = offset; i < xml.Length - 1; i++)
            {
                if (xml.Substring(i, 2) == "\"[")
                {
                    start = i + 2;
                    for (; i < xml.Length - 1; i++)
                    {
                        if (xml.Substring(i, 2) == "][")
                        {
                            end = i - 1;
                            break;
                        }
                    }
                    break;
                }
            }

            string[] p = xml.Substring(start, end - start + 1).Split(',');
            position[0] = Convert.ToInt16(p[0]);
            position[1] = Convert.ToInt16(p[1]);
            return position;
        }
    }
}
