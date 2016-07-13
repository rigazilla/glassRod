using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassRod
{
    public class OpGetRequest
    {
        public HeaderRequest header;
        public ulong keyLength;
        public List<byte> key;
        public OpGetRequest(HeaderRequest header)
        {
            this.header = header.Clone();
            this.header.opcode = 0x03;
        }
        public List<Byte> toBytes()
        {
            List<Byte> list = header.toBytes();
            list.AddRange(HotRodUtils.vIntegerToBytes(keyLength));
            list.AddRange(key);
            return list;
        }
    }
    /// <summary>
    /// Representation of a HotRod get operation response
    /// </summary>
    public class OpGetResponse
    {
        public HeaderResponse header;
        public ulong valueLength;
        public List<Byte> value;
        public static OpGetResponse fromBytes(List<byte> list)
        {
            OpGetResponse newResp = new OpGetResponse();
            ulong pos = 0;
            newResp.header = HeaderResponse.fromBytes(list, ref pos);
            newResp.valueLength = HotRodUtils.bytesToVLong(list, ref pos);
            if (newResp.valueLength != 0)
            {
                newResp.value = list.GetRange((int)pos, (int)(newResp.valueLength));
                pos += newResp.valueLength;
            }
            return newResp;
        }
    }

}
