using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassRod
{
    /// <summary>
    /// Representation of a HotRod put operation request
    /// </summary>
    public class OpPutRequest
    {
        public HeaderRequest header;
        public ulong keyLength;
        public List<byte> key;
        public byte timeUnits;
        public ulong lifeSpan = 0;
        public ulong maxIdle = 0;
        public ulong valueLength;
        public List<byte> value;
        public OpPutRequest(HeaderRequest header)
        {
            this.header = header.Clone();
            this.header.opcode = 0x01;
        }
        public List<Byte> toBytes()
        {
            List<Byte> list = header.toBytes();
            list.AddRange(HotRodUtils.vIntegerToBytes(keyLength));
            list.AddRange(key);
            list.Add(timeUnits);
            if ( (timeUnits & 0xf0) != 0x70 && (timeUnits & 0xf0) != 0x80)
            {
                list.AddRange(HotRodUtils.vIntegerToBytes(lifeSpan));
            }
            if ((timeUnits & 0x0f) != 0x07 && (timeUnits & 0x0f) != 0x08)
            {
                list.AddRange(HotRodUtils.vIntegerToBytes(maxIdle));
            }
            list.AddRange(HotRodUtils.vIntegerToBytes(valueLength));
            list.AddRange(value);
            return list;
        }
    }
    /// <summary>
    /// Representation of a HotRod put operation response
    /// </summary>
    public class OpPutResponse
    {
        public HeaderResponse header;
        public Byte responseStatus;
        public ulong prevValueLength;
        public List<Byte> prevValue;
        public static OpPutResponse fromBytes(List<byte> list)
        {
            OpPutResponse newResp = new OpPutResponse();
            ulong pos = 0;
            newResp.header = HeaderResponse.fromBytes(list, ref pos);
            newResp.responseStatus = list[(int)pos++];
            newResp.prevValueLength = HotRodUtils.bytesToULong(list, ref pos);
            if (newResp.prevValueLength != 0)
            {
                newResp.prevValue = list.GetRange((int)pos, (int)(newResp.prevValueLength));
                pos += newResp.prevValueLength;
            }
            return newResp;
        }
    }
}
