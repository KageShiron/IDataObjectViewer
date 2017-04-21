using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DataFormatLib;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace DataFormatLibWPF
{
    public class ComDataObjectWpf : ComDataObject
    {
        public BitmapSource GetBitmap()
        {
            STGMEDIUM stg = new STGMEDIUM();
            var f = DataObjectUtils.GetFormatEtc( DataFormatIdentifies.CF_BITMAP.Id );
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
                    using (var canvas = new Bitmap((int) (image.Size.Width), (int) (image.Size.Height)))
                    using (var graphics = Graphics.FromImage(canvas))
                    {
                        graphics.DrawImage(image, 0, 0, (int) (image.Size.Width), (int) (image.Size.Height));
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
