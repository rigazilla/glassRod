using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace GlassRod
{
    /// <summary>
    /// C# representation of the Hotrod messages reqHeader
    /// </summary>
    /// <remarks>Use this class as follow:
    /// instantiate the class, fill all the fields then call the toBytes() method to convert
    /// the object into bytes to be send over the socket</remarks>
    public class HeaderRequest
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

        internal HeaderRequest Clone()
        {
            HeaderRequest newRH = new HeaderRequest();
            foreach (var field in typeof(HeaderRequest).GetFields())
            {
                field.SetValue(newRH, field.GetValue(this));
            }
            return newRH;
        }
    }
    /// <summary>
    /// Representation of a hash aware server node with address and associated hashcode
    /// </summary>
    public class HashAwareNodeAddress
    {
        public string address;
        public ushort port;
        public ulong hashCode;
        public HashAwareNodeAddress(string address, ushort port, ulong hashCode)
        {
            this.address = address;
            this.port = port;
            this.hashCode = hashCode;
        }
    }

    /// <summary>
    /// Representation of a server node address
    /// </summary>
    public class NodeAddress
    {
        public string address;
        public ushort port;
        public NodeAddress(string address, ushort port)
        {
            this.address = address;
            this.port = port;
        }
    }

    /// <summary>
    /// Representation of the hash aware cluster topology information. This information usually comes from the server
    /// </summary>
    /// <remarks>Use this class as follows:
    /// take the topology information bytes that come from the server and pass them to the fromBytes() method
    /// and you'll get a new instance of this class. Segment based topology info obsoletes this class
    /// </remarks>
    public class HashAwareTopologyInfo
    {
        public ulong topologyId;
        public ushort numKeyOwner;
        public byte hashFuncVersion;
        public ulong hashSpaceSize;
        public List<HashAwareNodeAddress> nodes;

        private HashAwareTopologyInfo()
        {
        }

        public static HashAwareTopologyInfo fromBytes(List<Byte> bytes, ref ulong pos)
        {
            HashAwareTopologyInfo resp = new HashAwareTopologyInfo();
            ulong numNodes;
            resp.topologyId = HotRodUtils.bytesToVLong(bytes, ref pos);
            numNodes = HotRodUtils.bytesToVLong(bytes, ref pos);
            resp.nodes = new List<HashAwareNodeAddress>((int)numNodes);
            for (uint i = 0; i < numNodes; i++)
            {
                ulong addressLength = HotRodUtils.bytesToVLong(bytes, ref pos);
                string address = Encoding.UTF8.GetString(bytes.GetRange((int)pos, (int)addressLength).ToArray());
                pos += addressLength;
                ushort port = HotRodUtils.bytesToUShort(bytes, ref pos);
                ulong hashcode = HotRodUtils.bytesToULong(bytes, ref pos);
                resp.nodes.Add(new HashAwareNodeAddress(address, port, hashcode));
            }
            return resp;
        }
    }

    /// <summary>
    /// Representation of the segment based cluster topology information. This information usually comes from the server
    /// </summary>
    /// <remarks>Use this class as follows:
    /// take the topology information bytes that come from the server and pass them to the fromBytes() method
    /// and you'll get a new instance of this class. Segment based topology info obsoletes this class
    /// </remarks>
    public class SegmentBasedTopologyInfo
    {
        public ulong topologyId;
        public ushort numKeyOwner;
        public byte hashFuncVersion;
        public ulong numSegments;
        public ulong hashSpaceSize;
        public NodeAddress[] nodes;
        public List<int>[] segmentsToOwners;
        private SegmentBasedTopologyInfo()
        {
        }

        public static SegmentBasedTopologyInfo fromBytes(List<Byte> bytes, ref ulong pos)
        {
            SegmentBasedTopologyInfo resp = new SegmentBasedTopologyInfo();
            ulong numNodes;
            resp.topologyId = HotRodUtils.bytesToVLong(bytes, ref pos);
            numNodes = HotRodUtils.bytesToVLong(bytes, ref pos);
            resp.nodes = new NodeAddress[numNodes];
            for (uint i = 0; i < numNodes; i++)
            {
                ulong addressLength = HotRodUtils.bytesToVLong(bytes, ref pos);
                string address = Encoding.UTF8.GetString(bytes.GetRange((int)pos, (int)addressLength).ToArray());
                pos += addressLength;
                ushort port = HotRodUtils.bytesToUShort(bytes, ref pos);
                resp.nodes[i]= new NodeAddress(address, port);
            }
            resp.hashFuncVersion = bytes[(int)pos++];
            resp.numSegments = HotRodUtils.bytesToVLong(bytes, ref pos);
            resp.segmentsToOwners = new List<int>[resp.numSegments];
            for (uint i=0; i< resp.numSegments; i++)
            {
                resp.segmentsToOwners[i] = new List<int>();
                int numOwner = bytes[(int)pos++];
                for (uint j=0; j< numOwner; j++)
                {
                    ulong idx = HotRodUtils.bytesToVLong(bytes, ref pos);
                    resp.segmentsToOwners[i].Add((int)idx);
                }
            }
            return resp;
        }
    }

    /// <summary>
    /// Representation of a Hotrod server response reqHeader, the first part of a Hotrod response.
    /// </summary>
    /// <remarks>Use this class as follows:
    /// get the response bytes from the socket pass them to the fromBytes() method and get
    /// a new instance of this class</remarks>
    public class HeaderResponse
    {
        public byte magic;
        public ulong messageId;
        public byte opcode;
        public byte status;
        public byte topologyChangeMarker;
        public SegmentBasedTopologyInfo topologyInfo;
        private HeaderResponse()
        {
        }
        public static HeaderResponse fromBytes(List<Byte> bytes, ref ulong pos)
        {
            HeaderResponse resp = new HeaderResponse();
            if (bytes[0] != 0xA1)
            {
                throw new Exception("Bad magic number received: " + bytes[0]);
            }
            resp.magic = bytes[(int)pos++];

            resp.messageId = HotRodUtils.bytesToVLong(bytes, ref pos);
            resp.opcode = bytes[(int)pos++];
            resp.status = bytes[(int)pos++];
            resp.topologyChangeMarker = bytes[(int)pos++];

            if (resp.topologyChangeMarker != 0)
            {
                HashAwareTopologyInfo.fromBytes(bytes, ref pos);
            }
            return resp;
        }
    }
}
