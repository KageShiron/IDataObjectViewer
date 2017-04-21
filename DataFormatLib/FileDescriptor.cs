using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataFormatLib
{
    public class FileDescriptor
    {
        FILEDESCRIPTOR _fd;
        private FileDescriptor(FILEDESCRIPTOR fd)
        {
            this._fd = fd;
        }
        /*
        public static IEnumerable<FileDescriptor> FromIDataObject(IDataObject obj)
        {
            const string CFSTR_FILEDESCRIPTORW = "FileGroupDescriptorW";
            if (obj.GetDataPresent(CFSTR_FILEDESCRIPTORW))
            {
                return FromFileGroupDescriptor(obj.GetData(CFSTR_FILEDESCRIPTORW) as MemoryStream);
            }
            else throw new ArgumentException("obj have to have FileDescriptor");
        }*/

        public static IEnumerable<FileDescriptor> FromFileGroupDescriptor(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                uint length = br.ReadUInt32();
                var list = new FileDescriptor[length];
                for (int i = 0; i < length; i++)
                {
                    FILEDESCRIPTOR fd = InteropUtils.ReadFrom<FILEDESCRIPTOR>(br);
                    list[i] = new FileDescriptor(fd);
                }
                return list;
            }
        }

        public Guid? Clsid => ValueOrNull(FileDescriptorFlags.FD_CLSID, _fd.clsid);
        public SIZE? Size => ValueOrNull(FileDescriptorFlags.FD_SIZEPOINT, _fd.sizel);
        public POINT? Point => ValueOrNull(FileDescriptorFlags.FD_SIZEPOINT, _fd.pointl);
        public FileAttributes? FileAttributes => ValueOrNull(FileDescriptorFlags.FD_ATTRIBUTES, _fd.dwFileAttributes);
        public DateTime? CreationTime => FILETIME2DateTime(ValueOrNull(FileDescriptorFlags.FD_CREATETIME, _fd.ftCreationTime));
        public DateTime? LastAccessTime => FILETIME2DateTime(ValueOrNull(FileDescriptorFlags.FD_ACCESSTIME, _fd.ftLastAccessTime));
        public DateTime? WriteTime => FILETIME2DateTime(ValueOrNull(FileDescriptorFlags.FD_WRITESTIME, _fd.ftLastWriteTime));
        public ulong? FileSize => ValueOrNull(FileDescriptorFlags.FD_FILESIZE, (((ulong)_fd.nFileSizeHigh) << 32 | _fd.nFileSizeLow));
        public string FileName => _fd.cFileName;

        #region private members

        private TResult? ValueOrNull<TResult>(FileDescriptorFlags flag, TResult value) where TResult : struct
        {
            if ((_fd.dwFlags & flag) == flag) return value; else return null;
        }

        static DateTime? FILETIME2DateTime(System.Runtime.InteropServices.ComTypes.FILETIME? t)
        {
            if (t == null) return null;
            return DateTime.FromFileTime((long)(((ulong)t.Value.dwHighDateTime) << 32) | (uint)t.Value.dwLowDateTime);
        }
        #endregion

        #region InnerClass
        [Flags]
        public enum FileDescriptorFlags : uint
        {
            None = 0,
            FD_CLSID = 0x00000001,
            FD_SIZEPOINT = 0x00000002,
            FD_ATTRIBUTES = 0x00000004,
            FD_CREATETIME = 0x00000008,
            FD_ACCESSTIME = 0x00000010,
            FD_WRITESTIME = 0x00000020,
            FD_FILESIZE = 0x00000040,
            FD_PROGRESSUI = 0x00004000,
            FD_LINKUI = 0x00008000,
            FD_UNICODE = 0x80000000 //Windows Vista and later
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct FILEDESCRIPTOR
        {
            public FileDescriptorFlags dwFlags;
            public Guid clsid;
            public SIZE sizel;
            public POINT pointl;
            public FileAttributes dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public UInt32 nFileSizeHigh;
            public UInt32 nFileSizeLow;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

        }
        #endregion
    }
}
