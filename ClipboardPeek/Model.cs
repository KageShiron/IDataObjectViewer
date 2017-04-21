using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public FormatItem( DataObjectFormat format , Stream stream = null , object ect = null )
        {
            Format = format;
            Stream = stream;
            Excetra = ect;
        }

        public FormatItem(DataObjectFormat format,Exception e)
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
            set { SetProperty(ref _dataObjectFormats, value);  }
        }


        private static FormatItem CreateFormatItem(ComDataObjectWpf dataobj, DataObjectFormat f)
        {
            try
            {
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
                if (f.FormatId == DataFormatIdentifies.CF_DIB)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId), dataobj.GetDib());
                }
                if (f.FormatId.Id == DataFormatIdentifies.CF_ENHMETAFILE.Id)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId), dataobj.GetEnhancedMetafile());
                }
                if (f.FormatId.Id == DataFormatIdentifies.CF_METAFILEPICT.Id)
                {
                    return new FormatItem(f, dataobj.GetStream(f.FormatId), dataobj.GetMetafile());
                }
                return new FormatItem(f, dataobj.GetStream(f.FormatId));
            }
            catch (Exception e)
            {
                return new FormatItem(f,e);
            }
        }

        public void LoadFromClipboard()
        {
            var x = new ComDataObjectWpf(Clipboard.GetDataObject() as System.Runtime.InteropServices.ComTypes.IDataObject);
            DataObjectFormats = x.GetFormats().Select(y => CreateFormatItem(x,y) );
        }
    }
}
