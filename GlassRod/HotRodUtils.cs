using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassRod
{
    class HotRodUtils
    {
        public static List<byte> vIntegerToBytes(ulong value)
        {
            List<byte> ret = new List<byte>();
            do
            {
                byte b = (byte)(value % 0x7f);
                value >>= 7;
                if (value > 0)
                    b |= 0x80;
                ret.Add(b);
            } while (value > 0);
            return ret;
        }

        public static byte[] stringToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static ulong bytesToVLong(List<byte> bytes, ref ulong pos)
        {
            ulong ret = 0;
            do
            {
                ret += (ulong)(bytes[(int)pos] & 0x7f);
            } while ((bytes[(int)pos++] & 0x80) != 0);
            return ret;
        }

        public static ushort bytesToUShort(List<byte> bytes, ref ulong pos)
        {
            return (ushort)((int)bytes[(int)pos++] << 8 + bytes[(int)pos++]);
        }

        public static ulong bytesToULong(List<byte> bytes, ref ulong pos)
        {
            ulong ret = 0;
            for (int i=0; i<4; i++)
            {
                ret <<= 8;
                ret += bytes[(int)pos++];
            }
            return ret;
        }
    }
}
