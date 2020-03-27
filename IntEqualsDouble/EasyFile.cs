using System;
using System.IO;
using System.Linq;
using System.Text;

namespace IntEqualsDouble
{
    public class EasyFile
    {
        private FileStream fs = null;

        public void WriteBytes(byte[] bytes)
        {
            fs.Write(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// 写入一个EasyFile块。
        /// </summary>
        public void WriteBlock(byte[] bytes)
        {
            byte[] lengthBytes = BitConverter.GetBytes(bytes.Length);
            WriteBytes(lengthBytes);

            WriteBytes(bytes);
        }
        public void WriteInt32(int i)
        {
            WriteBlock(BitConverter.GetBytes(i));
        }

        public byte[] ReadBlock()
        {
            byte[] lengthBytes = BitConverter.GetBytes((int)0);
            fs.Read(lengthBytes, 0, lengthBytes.Length);

            byte[] temp = new byte[BitConverter.ToInt32(lengthBytes, 0)];
            fs.Read(temp, 0, temp.Length);
            return temp;
        }
        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadBlock(), 0);
        }

        public EasyFile(ref FileStream fileStream)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException("The file stream can not be null.");
            }
            this.fs = fileStream;
        }
    }
}
