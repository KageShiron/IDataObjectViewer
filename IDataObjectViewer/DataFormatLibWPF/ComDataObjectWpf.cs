using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DataFormatLib;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace DataFormatLibWPF
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public BitmapCompressionMode biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public void Init()
        {
            biSize = (uint)Marshal.SizeOf(this);
        }
    }
    public enum BitmapCompressionMode : uint
    {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5
    }

    public class ComDataObjectWpf : ComDataObject
    {
        public BitmapSource GetBitmap()
        {
            STGMEDIUM stg = new STGMEDIUM();
            var f = DataFormatIdentifies.CF_BITMAP.GetFormatEtc();
            DataObject.GetData(ref f, out stg);
            try
            {
                if (stg.tymed != TYMED.TYMED_GDI) throw new InvalidTymedException();
                //Imaging.CreateBitmapSourceFromMemorySection()
                return Imaging.CreateBitmapSourceFromHBitmap(stg.unionmember, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                stg.Release();
            }

        }

        private System.Windows.Media.PixelFormat GetDibFormat(ref BITMAPINFOHEADER h)
        {
            switch (h.biBitCount)
            {
                case 1:
                    return PixelFormats.Indexed1;
                case 4:
                    return PixelFormats.Indexed4;
                case 8:
                    return PixelFormats.Indexed8;
                case 24:
                    return PixelFormats.Bgr24;
                case 32:
                    return PixelFormats.Bgra32;
                /*
                case 1:
                    return PixelFormat.Format1bppIndexed;
                case 4:
                    return PixelFormat.Format4bppIndexed;
                case 8:
                    return PixelFormat.Format8bppIndexed;
                case 24:
                    return PixelFormat.Format24bppRgb;
                case 32:
                    return PixelFormat.Format32bppArgb;*/
                default:
                    throw new ApplicationException("Unsupported Bit Count");
            }
        }

        public BitmapSource GetDib()
        {

            STGMEDIUM stg = new STGMEDIUM();
            var f = DataFormatIdentifies.CF_DIB.GetFormatEtc();
            DataObject.GetData(ref f, out stg);
            try
            {
                var header = new byte[] { 0x42, 0x4D, 0, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0 };
                if (stg.tymed != TYMED.TYMED_HGLOBAL) throw new InvalidTymedException();
                using (var stream = stg.GetManagedStream(header) as MemoryStream)
                {
                    var buffer = stream.GetBuffer();
                    var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        var ptr = handle.AddrOfPinnedObject() + 14;
                        var s = (BITMAPINFOHEADER)Marshal.PtrToStructure(ptr, typeof(BITMAPINFOHEADER));
                        if (s.biBitCount == 32)
                        {
                            int stride = ((((s.biWidth * s.biBitCount) + 31) & ~31) >> 3);
                            return
                                new TransformedBitmap(
                                    BitmapSource.Create(s.biWidth, s.biHeight, 96, 96, GetDibFormat(ref s), null,
                                        ptr + (int)s.biSize +
                                        (s.biCompression == BitmapCompressionMode.BI_BITFIELDS ? 16 : 0)
                                        , buffer.Length - 14, ((((s.biWidth * s.biBitCount) + 31) & ~31) >> 3)),
                                    new ScaleTransform(1, -1));
                        }
                        else
                        {
                            BitConverter.GetBytes(buffer.Length).CopyTo(buffer, 2);
                            BitConverter.GetBytes(14  + (int)s.biSize + (s.biClrUsed == 0 ? (1 << (int)(s.biBitCount + 2)) : (int)s.biClrUsed * 4)).CopyTo(buffer, 10);
                            //stream.WriteTo(File.OpenWrite(@"C:\Users\mutsuki\Documents\無題2.bmp"));
                            //return BitmapFrame.Create( File.Open(@"C:\Users\mutsuki\Documents\無題2.bmp",FileMode.Open), BitmapCreateOptions.None, BitmapCacheOption.Default);
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    
                    return BitmapFrame.Create(stream);


                    /*
                try
                {
                    var st = new MemoryStream(buffer.Length + 14);

                    using (var b = new BinaryWriter(st, Encoding.UTF8, true))
                    {

                        st.Seek(0, SeekOrigin.Begin);
                        return BitmapFrame.Create(st, BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.Default);
                        // .Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    Bitmap bmp = null;

                    try
                    {

                        var ptr = handle.AddrOfPinnedObject();
                        var s = (BITMAPINFOHEADER)Marshal.PtrToStructure(ptr, typeof(BITMAPINFOHEADER));
                        int stride = ((((s.biWidth*s.biBitCount) + 31) & ~31) >> 3);
                        bmp = new Bitmap(s.biWidth, 10, -stride,
                            GetDibFormat(ref s), ptr + buffer.Length);// + (int)s.biSize + (s.biCompression == BitmapCompressionMode.BI_BITFIELDS ? 16 : 0) + (int)s.biSizeImage);//+ 40 + (int)s.biClrUsed * 4 + stride * (s.biHeight)) ;
                        return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());

                }
                finally
                {
                    //bmp?.Dispose();
                    handle.Free();
                    GC.KeepAlive(buffer);
                }*/

                }
            }
            finally
            {
                stg.Release();
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource ToWPFBitmap(System.Drawing.Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();

            BitmapSource source;
            try
            {
                source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
            return source;
        }

        public ImageSource GetMetafile()
        {
            //new Metafile()
            STGMEDIUM stg = new STGMEDIUM();
            var f = DataObjectUtils.GetFormatEtc(DataFormatIdentifies.CF_METAFILEPICT.Id);
            DataObject.GetData(ref f, out stg);
            try
            {
                if (stg.tymed != TYMED.TYMED_MFPICT) throw new InvalidTymedException();
                using (var image = new Metafile(stg.unionmember, false))
                {
                    using (var canvas = new Bitmap((int)(image.Size.Width), (int)(image.Size.Height)))
                    using (var graphics = Graphics.FromImage(canvas))
                    {
                        graphics.DrawImage(image, 0, 0, (int)(image.Size.Width), (int)(image.Size.Height));
                        return ToWPFBitmap(canvas);
                    }
                }
            }
            finally
            {
                stg.Release();
            }
        }

        public ImageSource GetEnhancedMetafile()
        {
            STGMEDIUM stg = new STGMEDIUM();
            var f = DataObjectUtils.GetFormatEtc(DataFormatIdentifies.CF_ENHMETAFILE.Id);
            DataObject.GetData(ref f, out stg);
            try
            {
                if (stg.tymed != TYMED.TYMED_ENHMF) throw new InvalidTymedException();
                using (var image = Image.FromStream(stg.GetManagedStream()))
                {
                    using (var canvas = new Bitmap((int)(image.Size.Width), (int)(image.Size.Height)))
                    using (var graphics = Graphics.FromImage(canvas))
                    {
                        graphics.DrawImage(image, 0, 0, (int)(image.Size.Width), (int)(image.Size.Height));
                        return ToWPFBitmap(canvas);
                    }
                }
            }
            finally
            {
                stg.Release();
            }
        }

        public ComDataObjectWpf(IDataObject dataObject) : base(dataObject)
        {
        }
    }
}
