using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassRod
{
    /// <summary>
    /// Representation of a HotRod Ping operation request
    /// </summary>
    public class OpPingRequest
    {
        public HeaderRequest header;
        public OpPingRequest(HeaderRequest header)
        {
            this.header = header.Clone();
            this.header.opcode = 0x17;
        }
        public List<Byte> toBytes()
        {
            return header.toBytes();
        }
    }
    /// <summary>
    /// Representation of a HotRod Ping operation response
    /// </summary>
    public class OpPingResponse
    {
        public HeaderResponse header;
        public Byte responseStatus;

        public static OpPingResponse fromBytes(List<byte> list)
        {
            OpPingResponse newResp = new OpPingResponse();
            ulong pos = 0;
            newResp.header = HeaderResponse.fromBytes(list, ref pos);
            newResp.responseStatus = list[(int)pos++];
            return newResp;
        }
    }
}
