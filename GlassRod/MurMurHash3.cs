using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassRod
{
    public class MurMurHash3
    {
        // 128 bit output, 64 bit platform version

        public static ulong READ_SIZE = 16;
        private static ulong C1 = 0x87c37b91114253d5L;
        private static ulong C2 = 0x4cf5ad432745937fL;

        private ulong length;
        private uint seed; // if want to start with a seed, create a constructor
        ulong h1;
        ulong h2;

        private void MixBody(ulong k1, ulong k2)
        {
            h1 ^= MixKey1(k1);

            h1 = h1.RotateLeft(27);
            h1 += h2;
            h1 = h1 * 5 + 0x52dce729;

            h2 ^= MixKey2(k2);

            h2 = h2.RotateLeft(31);
            h2 += h1;
            h2 = h2 * 5 + 0x38495ab5;
        }

        private static ulong MixKey1(ulong k1)
        {
            k1 *= C1;
            k1 = k1.RotateLeft(31);
            k1 *= C2;
            return k1;
        }

        private static ulong MixKey2(ulong k2)
        {
            k2 *= C2;
            k2 = k2.RotateLeft(33);
            k2 *= C1;
            return k2;
        }

        private static ulong MixFinal(ulong k)
        {
            // avalanche bits

            k ^= k >> 33;
            k *= 0xff51afd7ed558ccdL;
            k ^= k >> 33;
            k *= 0xc4ceb9fe1a85ec53L;
            k ^= k >> 33;
            return k;
        }

        public byte[] ComputeHash(byte[] bb)
        {
            ProcessBytes(bb);
            return Hash;
        }

        private void ProcessBytes(byte[] bb)
        {
            h1 = seed;
            this.length = 0L;

            int pos = 0;
            ulong remaining = (ulong)bb.Length;

            // read 128 bits, 16 bytes, 2 longs in eacy cycle
            while (remaining >= READ_SIZE)
            {
                ulong k1 = bb.GetUInt64(pos);
                pos += 8;

                ulong k2 = bb.GetUInt64(pos);
                pos += 8;

                length += READ_SIZE;
                remaining -= READ_SIZE;

                MixBody(k1, k2);
            }

            // if the input MOD 16 != 0
            if (remaining > 0)
                ProcessBytesRemaining(bb, remaining, pos);
        }

        private void ProcessBytesRemaining(byte[] bb, ulong remaining, int pos)
        {
            ulong k1 = 0;
            ulong k2 = 0;
            length += remaining;

            // little endian (x86) processing
            switch (remaining)
            {
                case 15:
                    k2 ^= (ulong)bb[pos + 14] << 48; // fall through
                    goto case 14;
                case 14:
                    k2 ^= (ulong)bb[pos + 13] << 40; // fall through
                    goto case 13;
                case 13:
                    k2 ^= (ulong)bb[pos + 12] << 32; // fall through
                    goto case 12;
                case 12:
                    k2 ^= (ulong)bb[pos + 11] << 24; // fall through
                    goto case 11;
                case 11:
                    k2 ^= (ulong)bb[pos + 10] << 16; // fall through
                    goto case 10;
                case 10:
                    k2 ^= (ulong)bb[pos + 9] << 8; // fall through
                    goto case 9;
                case 9:
                    k2 ^= (ulong)bb[pos + 8]; // fall through
                    goto case 8;
                case 8:
                    k1 ^= bb.GetUInt64(pos);
                    break;
                case 7:
                    k1 ^= (ulong)bb[pos + 6] << 48; // fall through
                    goto case 6;
                case 6:
                    k1 ^= (ulong)bb[pos + 5] << 40; // fall through
                    goto case 5;
                case 5:
                    k1 ^= (ulong)bb[pos + 4] << 32; // fall through
                    goto case 4;
                case 4:
                    k1 ^= (ulong)bb[pos + 3] << 24; // fall through
                    goto case 3;
                case 3:
                    k1 ^= (ulong)bb[pos + 2] << 16; // fall through
                    goto case 2;
                case 2:
                    k1 ^= (ulong)bb[pos + 1] << 8; // fall through
                    goto case 1;
                case 1:
                    k1 ^= (ulong)bb[pos]; // fall through
                    break;
                default:
                    throw new Exception("Something went wrong with remaining bytes calculation.");
            }

            h1 ^= MixKey1(k1);
            h2 ^= MixKey2(k2);
        }

        public byte[] Hash
        {
            get
            {
                h1 ^= length;
                h2 ^= length;

                h1 += h2;
                h2 += h1;

                h1 = MurMurHash3.MixFinal(h1);
                h2 = MurMurHash3.MixFinal(h2);

                h1 += h2;
                h2 += h1;

                var hash = new byte[MurMurHash3.READ_SIZE];

                Array.Copy(BitConverter.GetBytes(h1), 0, hash, 0, 8);
                Array.Copy(BitConverter.GetBytes(h2), 0, hash, 8, 8);

                return hash;
            }
        }
        private long fmix64(long k)
        {
            k ^= (long)((ulong)k >> 33);
            k *= unchecked((long)0xff51afd7ed558ccd);
            k ^= (long)((ulong)k >> 33);
            k *= unchecked((long)0xc4ceb9fe1a85ec53);
            k ^= (long)((ulong)k >> 33);
            return k;
        }

        public long MurmurHash3_x64_64(sbyte[] key, uint len, uint seed)
        {
            // Exactly the same as MurmurHash3_x64_128, except it only returns state.h1
            byte[] data = (byte[])(Array)key;
            uint nblocks = len / 16;

            long h1 = unchecked((long)0x9368e53c2f6af274 ^ seed);
            long h2 = 0x586dcd208f7cd3fd ^ seed;

            long c1 = unchecked((long)0x87c37b91114253d5);
            long c2 = 0x4cf5ad432745937f;


            for (int i = 0; i < nblocks; i++)
            {
                long ik1 = BitConverter.ToInt64(data, (i * 2 + 0) * 8);
                long ik2 = BitConverter.ToInt64(data, (i * 2 + 1) * 8);

                // bmix

                ik1 *= c1;
                ik1 = IntHelpers.RotateLeft(ik1, 23);
                ik1 *= c2;
                h1 ^= ik1;
                h1 += h2;

                h2 = IntHelpers.RotateLeft(h2, 41);


                ik2 *= c2;
                ik2 = IntHelpers.RotateLeft(ik2, 23);
                ik2 *= c1;
                h2 ^= ik2;
                h2 += h1;

                h1 = h1 * 3 + 0x52dce729;
                h2 = h2 * 3 + 0x38495ab5;

                c1 = c1 * 5 + 0x7b7d159c;
                c2 = c2 * 5 + 0x6bce6396;
            }

            //----------
            // tail

            sbyte[] tail = new sbyte[data.Length - nblocks * 16];
            Buffer.BlockCopy(data, (int)nblocks * 16, tail, 0, tail.Length);

            long k1 = 0;
            long k2 = 0;

            switch (len & 15)
            {
                case 15:
                    k2 ^= (long)(tail[14]) << 48;
                    goto case 14;
                case 14:
                    k2 ^= (long)(tail[13]) << 40;
                    goto case 13;
                case 13:
                    k2 ^= (long)(tail[12]) << 32;
                    goto case 12;
                case 12:
                    k2 ^= (long)(tail[11]) << 24;
                    goto case 11;
                case 11:
                    k2 ^= (long)(tail[10]) << 16;
                    goto case 10;
                case 10:
                    k2 ^= (long)(tail[9]) << 8;
                    goto case 9;
                case 9:
                    k2 ^= (long)(tail[8]) << 0;
                    goto case 8;
                case 8:
                    k1 ^= (long)(tail[7]) << 56;
                    goto case 7;
                case 7:
                    k1 ^= (long)(tail[6]) << 48;
                    goto case 6;
                case 6:
                    k1 ^= (long)(tail[5]) << 40;
                    goto case 5;
                case 5:
                    k1 ^= (long)(tail[4]) << 32;
                    goto case 4;
                case 4:
                    k1 ^= (long)(tail[3]) << 24;
                    goto case 3;
                case 3:
                    k1 ^= (long)(tail[2]) << 16;
                    goto case 2;
                case 2:
                    k1 ^= (long)(tail[1]) << 8;
                    goto case 1;
                case 1:
                    k1 ^= (long)(tail[0]) << 0;
                    break;
            };

            if ((len & 15) != 0)
            {
                // bmix
                k1 *= c1;
                k1 = IntHelpers.RotateLeft(k1, 23);
                k1 *= c2;
                h1 ^= k1;
                h1 += h2;

                h2 = IntHelpers.RotateLeft(h2, 41);

                k2 *= c2;
                k2 = IntHelpers.RotateLeft(k2, 23);
                k2 *= c1;
                h2 ^= k2;
                h2 += h1;

                h1 = h1 * 3 + 0x52dce729;
                h2 = h2 * 3 + 0x38495ab5;

                c1 = c1 * 5 + 0x7b7d159c;
                c2 = c2 * 5 + 0x6bce6396;
            }

            //----------
            // finalization
            h2 ^= len;

            h1 += h2;
            h2 += h1;

            h1 = fmix64(h1);
            h2 = fmix64(h2);

            h1 += h2;
            h2 += h1;

            return h1;
        }
        public int MurmurHash3_x64_64(int key)
        {
            // Obtained by inlining MurmurHash3_x64_32(byte[], 9001) and removing all the unused code
            // (since we know the input is always 4 bytes and we only need 4 bytes of output)
            sbyte b0 = (sbyte)key;
            sbyte b1 = (sbyte)((uint)key >> 8);
            sbyte b2 = (sbyte)((uint)key >> 16);
            sbyte b3 = (sbyte)((uint)key >> 24);

            long h1 = unchecked((long)0x9368e53c2f6af274 ^ 9001);
            long h2 = 0x586dcd208f7cd3fd ^ 9001;

            long c1 = unchecked ((long)0x87c37b91114253d5);
            long c2 = 0x4cf5ad432745937f;

            long k1 = 0;
            long k2 = 0;

            k1 ^= (long)b3 << 24;
            k1 ^= (long)b2 << 16;
            k1 ^= (long)b1 << 8;
            k1 ^= b0;

            // bmix
            k1 *= c1;
            k1 = IntHelpers.RotateLeft(k1, 23);
            k1 *= c2;
            h1 ^= k1;
            h1 += h2;

            h2 = IntHelpers.RotateLeft(h2, 41);

            k2 *= c2;
            k2 = IntHelpers.RotateLeft(k2, 23);
            k2 *= c1;
            h2 ^= k2;
            h2 += h1;

            h1 = h1 * 3 + 0x52dce729;
            h2 = h2 * 3 + 0x38495ab5;

            c1 = c1 * 5 + 0x7b7d159c;
            c2 = c2 * 5 + 0x6bce6396;

            h2 ^= 4;

            h1 += h2;
            h2 += h1;

            h1 = fmix64(h1);
            h2 = fmix64(h2);

            h1 += h2;
            h2 += h1;

            return (int)((ulong)h1 >> 32);
        }
    }

    public static class IntHelpers
{
    public static ulong RotateLeft(this ulong original, int bits)
    {
        return (original << bits) | (original >> (64 - bits));
    }

    public static ulong RotateRight(this ulong original, int bits)
    {
        return (original >> bits) | (original << (64 - bits));
    }

    public static long RotateLeft(this long original, int bits)
    {
        return (original << bits) | (long)((ulong)original >> (64 - bits));
    }

    public static long RotateRight(this long original, int bits)
    {
        return (long)((ulong)original >> bits) | (original << (64 - bits));
    }

    unsafe public static ulong GetUInt64(this byte[] bb, int pos)
    {
        //// we only read aligned longs, so a simple casting is enough
        fixed (byte* pbyte = &bb[pos])
        {
            return *((ulong*)pbyte);
        }
    }
}
}
