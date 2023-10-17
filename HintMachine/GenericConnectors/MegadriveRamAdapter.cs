using System;

namespace HintMachine.GenericConnectors
{
    public class InvalidMegadriveAddressException : Exception
    {
        public InvalidMegadriveAddressException(string message) : base(message)
        { }
    }

    public class MegadriveRamAdapter : ProcessRamWatcher
    {
        public MegadriveRamAdapter(BinaryTarget target) : base(target)
        {}

        public new ushort ReadUint16(long address)
        {
            if (address % 2 != 0)
                throw new InvalidMegadriveAddressException("Cannot read a word or long starting at an odd address");

            return BitConverter.ToUInt16(ReadBytes(address, sizeof(ushort)), 0);
        }

        public new uint ReadUint32(long address)
        {
            ushort msWord = ReadUint16(address);
            ushort lsWord = ReadUint16(address+2);
            return (uint)(msWord << 16 + lsWord);
        }

        public new byte ReadUint8(long address)
        {
            ushort word = ReadUint16(address - (address % 2));
            if (address % 2 == 0)
                return (byte)(word >> 8);
            else
                return (byte)(word & 0x00FF);
        }

        public new ulong ReadUint64(long address)
            => throw new NotSupportedException();

        public new sbyte ReadInt8(long address)
            => (sbyte)ReadUint8(address);

        public new short ReadInt16(long address)
            => (short)ReadUint16(address);

        public new int ReadInt32(long address)
            => (int)ReadUint32(address);

        public new long ReadInt64(long address)
            => throw new NotSupportedException();

        public new double ReadDouble(long address)
            => throw new NotSupportedException();

    }
}
