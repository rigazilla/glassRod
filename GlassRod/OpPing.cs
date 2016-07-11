using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassRod
{
    public class OpPing
    {

        public RequestHeader header;
        public OpPing(RequestHeader header)
        {
            header.opcode = 0x17;
            this.header = header;
        }
        public List<Byte> toBytes()
        {
            return header.toBytes();
        }
    }
}
