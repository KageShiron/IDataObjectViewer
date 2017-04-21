using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataFormatLib
{
    public static class InteropUtils
    {

        public static TStruct ReadFrom<TStruct>(Stream s) where TStruct : struct
        {
            int size = Marshal.SizeOf(typeof(TStruct));
            byte[] buffer = new byte[size];
            s.Read(buffer, 0, size);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                return (TStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TStruct));
            }
            finally
            {
                handle.Free();
            }
        }

        public static TStruct ReadFrom<TStruct>(BinaryReader reader) where TStruct : struct
        {
            var buffer = reader.ReadBytes(Marshal.SizeOf(typeof(TStruct)));
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                return (TStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TStruct));
            }
            finally
            {
                handle.Free();
            }
        }

    }
}
