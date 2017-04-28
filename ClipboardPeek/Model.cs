using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using DataFormatLib;
using DataFormatLibWPF;
using Prism.Mvvm;

namespace ClipboardPeek
{
    public class FormatItem
    {
        public DataObjectFormat Format { get; }
        public Stream Stream { get; }
        public object Excetra { get; }
        public Exception Error { get; }

        public FormatItem(DataObjectFormat format, Stream stream = null, object ect = null)
        {
            Format = format;
            Stream = stream;
            Excetra = ect;
        }

        public FormatItem(DataObjectFormat format, Exception e)
        {
            Format = format;
            Error = e;
        }
    }

    public class Model : BindableBase
    {
        private IEnumerable<FormatItem> _dataObjectFormats;
        public IEnumerable<FormatItem> DataObjectFormats
        {
            get { return _dataObjectFormats; }
            set { SetProperty(ref _dataObjectFormats, value); }
        }


        private static FormatItem CreateFormatItem(ComDataObjectWpf dataobj, DataObjectFormat f)
        {
            try
            {
                if (f.NotDataObject)
                {
                    object o = Clipboard.GetData(f.FormatId.DotNetName);
                    var s = o as Stream;
                    if(s != null)return new FormatItem(f,s);
                    else return new FormatItem(f,null,o);
                }
                
                if (f.FormatId.Id == DataFormatIdentifies.CFSTR_FILECONTENTS.Id)
                {
                    return new FormatItem(f, null, dataobj.GetFileContents());
                }
                if (f.FormatId.Id == DataFormatIdentifies.CF_HDROP.Id)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId), dataobj.GetFiles());
                }
                if (f.FormatId.Id == DataFormatIdentifies.CFSTR_FILEDESCRIPTORW.Id)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId), dataobj.GetFileDescriptors());
                }
                if (f.FormatId.Id == DataFormatIdentifies.CF_BITMAP.Id)
                {
                    return new FormatItem(f, null, dataobj.GetBitmap());
                }
                if (f.FormatId == DataFormatIdentifies.CF_DIB || f.FormatId == DataFormatIdentifies.CF_DIBV5)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId), dataobj.GetDib());
                }
                if (f.FormatId.Id == DataFormatIdentifies.CF_ENHMETAFILE.Id)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId), dataobj.GetEnhancedMetafile());
                }
                if (f.FormatId.Id == DataFormatIdentifies.CF_METAFILEPICT.Id)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId)/*, dataobj.GetMetafile()*/);
                }
                return new FormatItem(f, dataobj.GetStream(f.FormatId));
            }
            catch (Exception e)
            {
                return new FormatItem(f, e);
            }
        }

        [DllImport("user32.dll")]
        static extern uint EnumClipboardFormats(uint format);

        [DllImport("user32.dll")]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        static extern int GetClipboardFormatName(uint format, [Out] StringBuilder lpszFormatName, int cchMaxCount);


        public void LoadFromClipboard(bool allFormat)
        {


            var x = new ComDataObjectWpf(Clipboard.GetDataObject() as System.Runtime.InteropServices.ComTypes.IDataObject);
            DataObjectFormats = x.GetFormats(allFormat).Select(y => CreateFormatItem(x, y));


        }
        public void LoadFromDataObject(System.Runtime.InteropServices.ComTypes.IDataObject obj)
        {

            var x = new ComDataObjectWpf(obj);
            DataObjectFormats = x.GetFormats().Select(y => CreateFormatItem(x, y));


        }
    }
}
