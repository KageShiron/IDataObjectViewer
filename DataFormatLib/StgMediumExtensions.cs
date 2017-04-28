using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using IStream = DataFormatLib.IStream;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace DataFormatLib
{
    public class InvalidTymedException : Exception
    {
        public InvalidTymedException()
        {
        }

        public InvalidTymedException(string message) : base(message)
        {
        }

        public InvalidTymedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTymedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    
    public static class StgMediumExtensions
    {
        #region P/Invoke

        [DllImport("ole32.dll")]
        static extern void ReleaseStgMedium([In] ref STGMEDIUM pmedium);

        [DllImport("kernel32.dll")]
        private static extern UIntPtr GlobalSize(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("gdi32.dll")]
        private static extern uint GetEnhMetaFileBits(IntPtr hemf, uint cbBuffer, [Out] byte[] lpbBuffer);
        
        [DllImport("gdi32.dll")]
        private static extern uint GetEnhMetaFileBits(IntPtr hemf, uint cbBuffer, IntPtr lpbBuffer);

        [DllImport("gdi32.dll")]
        private static extern uint GetMetaFileBitsEx(IntPtr hmf, uint cbBuffer, [Out] byte[] lpbBuffer);

        [DllImport("gdi32.dll")]
        private static extern uint GetMetaFileBitsEx(IntPtr hmf, uint cbBuffer, IntPtr lpbBuffer);

        [DllImport("ole32.dll")]
        static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out IStream ppstm);

        [DllImport("shell32.dll",CharSet = CharSet.Auto)]
        static extern int DragQueryFile(IntPtr hDrop, int iFile,
            [Out] StringBuilder lpszFile, uint cch);


        #endregion 

        /// <summary>
        /// Unmanagedなメモリ領域を持つStreamを作成します。
        /// 2回目以降の呼び出し結果は未定義となります。
        /// </summary>
        /// <param name="stg">IStreamまたはHGLOBALであるSTGMEDIUM</param>
        /// <param name="autoRelease">作成したStreamが破棄された場合、stg自体を破棄するかを指定します。
        /// trueを指定した場合、呼び出し元は作成したStreamのDispose以外の手段でメモリを開放してはいけません。</param>
        /// <returns></returns>
        public static Stream GetUnmanagedStream(this STGMEDIUM stg , bool autoRelease )
        {
            IStream s;
            switch (stg.tymed)
            {
                case TYMED.TYMED_HGLOBAL:   // create IStream
                    s = StgMediumExtensions.GetIStreamFromHglobal(stg.unionmember,false);
                    break;
                case TYMED.TYMED_ISTREAM:   // cast to IStream
                    s = (IStream)Marshal.GetObjectForIUnknown(stg.unionmember);
                    break;
                default:                    // Error
                    throw new InvalidTymedException(stg.tymed.ToString());
            }
            var cs = new ComStream(s,false,true);

            // Release StgMedium
            if (autoRelease)
            {
                cs.Disposed += (sender, __) =>  // when ComStream disposed
                {
                    // release created IStream
                    if ((stg.tymed & TYMED.TYMED_HGLOBAL) != 0) Marshal.ReleaseComObject(s);
                    stg.Release();
                };
            }
            return cs;
        }

        /// <summary>
        /// stgの内容をコピーしたManged上のMemoryStreamを作成します。
        /// (STGMEDIUMの解放は行いません)
        /// </summary>
        /// <param name="stg">HGLOBAL,ISTREAM,MFPICT,ENHMFのいずれかのSTGMEDIUMを取得します</param>
        /// <param name="header">取得するストリームの先頭に書き込むヘッダ(現在HGLOBALのみ対応)</param>
        /// <returns>MangedメモリにコピーしたStream</returns>
        public static Stream GetManagedStream(this STGMEDIUM stg,byte[] header = null)
        {
            if (stg.tymed != TYMED.TYMED_HGLOBAL && header != null)
            {
                throw new ArgumentException("headerはstg.tymed == TYMED.TYMED_HGLOBALの場合のみ使用可能です",nameof(header));
            }
            switch (stg.tymed)
            {
                case TYMED.TYMED_HGLOBAL:
                    return StgMediumExtensions.CreateStreamFromHglobal(stg.unionmember,header);
                case TYMED.TYMED_FILE:
                    throw new NotImplementedException("WHAT APP USE TYMED_FILE!!??");
                case TYMED.TYMED_ISTREAM:
                    return StgMediumExtensions.CreateStreamFromIStream(stg.unionmember);
                case TYMED.TYMED_ISTORAGE:
                    throw new NotSupportedException();
                case TYMED.TYMED_GDI:
                    throw new NotSupportedException();
                case TYMED.TYMED_MFPICT:
                    //return StgMediumExtensions.CreateStreamFromHglobal(stg.unionmember);
                    return StgMediumExtensions.CreateStreamFromMetaFile(stg.unionmember);
                case TYMED.TYMED_ENHMF:
                    return StgMediumExtensions.CreateStreamFromEnhMetaFile(stg.unionmember);
                case TYMED.TYMED_NULL:
                default:
                    throw new InvalidTymedException(stg.tymed.ToString());
            }
        }

        public static string GetStringUnsafe(this STGMEDIUM stg)
        {
            if(stg.tymed != TYMED.TYMED_HGLOBAL)throw new ArgumentException(nameof(stg));

            return Marshal.PtrToStringUni(stg.unionmember);
        }

        public static string[] GetFiles(this STGMEDIUM stg)
        {
            if (stg.tymed != TYMED.TYMED_HGLOBAL) throw new ArgumentException("tymed is not hglobal");
            
            int count = DragQueryFile(stg.unionmember, -1, null, 0);
            if (count <= 0) return new string[0];
            string[] files = new string[count];
            StringBuilder sb = new StringBuilder(260);
            for (int i = 0; i < count; i++)
            {
                if (DragQueryFile(stg.unionmember, i , sb, 260) > 0)
                {
                    files[i] = sb.ToString();
                }
            }
            return files;
        }



        /// <summary>
        /// STGMEDIUMを解放します
        /// </summary>
        /// <param name="stg">解放するSTGMEDIUM</param>
        public static void Release( this STGMEDIUM stg)
        {
            ReleaseStgMedium( ref stg );
        }

        /// <summary>
        /// HGLOBALの内容をコピーしたMemoryStreamを作成します。
        /// </summary>
        /// <param name="hGlobal">データを格納したHGLOBAL</param>
        /// <param name="header">ストリームの先頭に書き込むデータ</param>
        /// <returns>作成したStream</returns>
        private static Stream CreateStreamFromHglobal(IntPtr hGlobal , byte[] header = null)
        {
            try
            {
                IntPtr locked = GlobalLock(hGlobal);
                uint headerLen = (uint) (header?.Length ?? 0);
                uint size = GlobalSize(locked).ToUInt32() + headerLen ;
                byte[] d = new byte[size];
                if(headerLen > 0)header.CopyTo(d,0);
                Marshal.Copy(locked,d,(int) headerLen,  (int)(size - headerLen));
                return new MemoryStream(d, 0, (int) size, true, true);
            }
            finally
            {
                GlobalUnlock(hGlobal);
            }
        }

        /// <summary>
        /// IStreamの内容をコピーしたMemoryStreamを作成します
        /// </summary>
        /// <param name="iStream">データを格納したIStream</param>
        /// <returns>作成したStream</returns>
        private static Stream CreateStreamFromIStream(IntPtr iStream )
        {
            IStream s = (IStream)Marshal.GetObjectForIUnknown(iStream);
            STATSTG stat;
            s.Stat(out stat, 0);
            byte[] bin = new byte[stat.cbSize];
            long _;
            uint __;

            //TODO: binはHGLOBALである必要がある？sizeは?
            s.Seek(0, 0, out _);
            s.Read(bin, (int)stat.cbSize, out __);

            //TODO: stat.pwcsNameは解放する必要があるか？
            return new MemoryStream(bin);
        }

        /// <summary>
        /// 拡張メタファイルのデータをコピーしてMemoryStreamを作成します
        /// </summary>
        /// <param name="hEnhFile">拡張メタファイルのハンドル</param>
        /// <returns>作成したStream</returns>
        private static Stream CreateStreamFromEnhMetaFile(IntPtr hEnhFile)
        {
            uint size = GetEnhMetaFileBits(hEnhFile, 0, IntPtr.Zero);
            byte[] bin = new byte[size];
            GetEnhMetaFileBits(hEnhFile, size, bin);
            return new MemoryStream(bin);
        }

        /// <summary>
        /// メタファイルのデータをコピーしてMemoryStreamを作成します
        /// </summary>
        /// <param name="hMetaFile">メタファイルのハンドル</param>
        /// <returns>作成したStream</returns>
        private static Stream CreateStreamFromMetaFile(IntPtr hMetaFile)
        {
            uint size = GetMetaFileBitsEx(hMetaFile, 0, IntPtr.Zero);
            byte[] bin = new byte[size];
            GetMetaFileBitsEx(hMetaFile, size, bin);
            return new MemoryStream(bin);
        }

        /// <summary>
        /// HGLOBALからCreateStreamOnHGlobalを利用してStreamを作成します。
        /// </summary>
        /// <param name="hGlobal">HGLOBAL</param>
        /// <param name="deleteOnRelease"></param>
        /// <returns></returns>
        private static IStream GetIStreamFromHglobal(IntPtr hGlobal , bool deleteOnRelease)
        {
            IStream stream;
            CreateStreamOnHGlobal(hGlobal, deleteOnRelease, out stream);
            return stream;
        }
    }
    
}
