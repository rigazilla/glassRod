using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassRod
{
    public class RequestHeader
    {
        public ulong messageId;
        public Byte version;
        public Byte opcode;
        public ulong cacheNameLength;
        public String cacheName;
        public ulong flags;
        public Byte clientIntelligence;
        public ulong topologyId;

        public List<Byte> toBytes()
        {
            List<Byte> list = new List<Byte>();
            list.Add(0xa0);
            list.AddRange(HotRodUtils.vIntegerToBytes(messageId));
            list.Add(version);
            list.Add(opcode);
            list.AddRange(HotRodUtils.vIntegerToBytes(cacheNameLength));
            if (cacheNameLength>0)
            { 
              list.AddRange(HotRodUtils.stringToBytes(cacheName));
            }
            list.AddRange(HotRodUtils.vIntegerToBytes(flags));
            list.Add(clientIntelligence);
            list.AddRange(HotRodUtils.vIntegerToBytes(topologyId));
            return list;
        }
    }
}
