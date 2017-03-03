using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataFormatLib;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace DataFormatLibWinForms
{
    public class ComDataObjectWinForms : ComDataObject
    {
        public Bitmap GetBitmap()
        {
            STGMEDIUM stg = new STGMEDIUM();
            var f  = DataObjectUtils.GetFormatEtc( DataFormatIdentifies.CF_BITMAP.Id);
            DataObject.GetData( ref f , out stg );
            try
            {
                if (stg.tymed != TYMED.TYMED_GDI) throw new InvalidTymedException();
                using (var bitmap = Image.FromHbitmap(stg.unionmember))
                    return (Bitmap) bitmap.Clone();
            }
            finally
            {
                stg.Release();
            }
        }

        public Metafile GetMetafile()
        {
            STGMEDIUM stg = new STGMEDIUM();
            var f = DataObjectUtils.GetFormatEtc(DataFormatIdentifies.CF_METAFILEPICT.Id);
            DataObject.GetData(ref f, out stg);
            try
            {
                if (stg.tymed != TYMED.TYMED_MFPICT) throw new InvalidTymedException();
                return new Metafile(stg.GetManagedStream());
            }
            finally
            {
                stg.Release();
            }
        }
        public Metafile GetEnhancedMetafile()
        {
            STGMEDIUM stg = new STGMEDIUM();
            var f = DataObjectUtils.GetFormatEtc(DataFormatIdentifies.CF_ENHMETAFILE.Id);
            DataObject.GetData(ref f, out stg);
            try
            {
                if (stg.tymed != TYMED.TYMED_ENHMF) throw new InvalidTymedException();
                return new Metafile(stg.GetManagedStream());
            }
            finally
            {
                stg.Release();
            }
        }

        public ComDataObjectWinForms(IDataObject dataObject) : base(dataObject)
        {
        }
    }
}
