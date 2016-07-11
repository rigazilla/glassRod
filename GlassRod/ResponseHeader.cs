using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
namespace GlassRod
{
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
    public class HashAwareTopologyInfo
    {
        public ulong topologyId;
        public ushort numKeyOwner;
        public byte hashFuncVersion;
        public ulong hashSpaceSize;
        public ulong numNodes;
        public List<HashAwareNodeAddress> nodes;

        private HashAwareTopologyInfo()
        {
        }

        public static HashAwareTopologyInfo fromBytes(List<Byte> bytes)
        {
            HashAwareTopologyInfo resp = new HashAwareTopologyInfo();
            ulong pos = 0;
            resp.topologyId = HotRodUtils.bytesToVLong(bytes, ref pos);
            resp.numNodes = HotRodUtils.bytesToVLong(bytes, ref pos);
            resp.nodes = new List<HashAwareNodeAddress>();
            for (uint i=0; i < resp.numNodes; i++)
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

    public class ResponseHeader
    {
        public byte magic;
        public ulong messageId;
        public byte opcode;
        public byte status;
        public byte topologyChangeMarker;
        public HashAwareTopologyInfo topologyInfo;
        private ResponseHeader()
        {
        }
        public static ResponseHeader fromBytes(List<Byte> bytes)
        {
            ResponseHeader resp = new ResponseHeader();
            ulong pos = 0;
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
                List<Byte> topologyList = bytes.GetRange((int)pos, bytes.Count);
                HashAwareTopologyInfo.fromBytes(topologyList);
            }
            return resp;
        }
    }
}