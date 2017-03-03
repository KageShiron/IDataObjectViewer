using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace DataFormatLib
{
    public class ComDataObject
    {
        public IComDataObject DataObject { get; }
        private Dictionary<int, Stream> _cache = new Dictionary<int, Stream>();

        public ComDataObject( IComDataObject dataObject )
        {
            DataObject = dataObject;
        }

        #region GetCanonicalFormatEtc

        public FORMATETC GetCanonicalFormatEtc(int format)
            => GetCanonicalFormatEtc(DataObjectUtils.GetFormatEtc(format));

        public FORMATETC GetCanonicalFormatEtc(string format)
            => GetCanonicalFormatEtc(DataObjectUtils.GetFormatEtc(format));

        public FORMATETC GetCanonicalFormatEtc(DataFormatIdentify format)
            => GetCanonicalFormatEtc(format.GetFormatEtc());

        public FORMATETC GetCanonicalFormatEtc(FORMATETC format)
        {
            FORMATETC c;
            var re = DataObject.GetCanonicalFormatEtc(ref format, out c);
            return c;
        }
        #endregion

        #region GetDataPresent

        public bool GetDataPresent(int format)
            => GetDataPresent(DataObjectUtils.GetFormatEtc(format));

        public bool GetDataPresent(string format)
            => GetDataPresent(DataObjectUtils.GetFormatEtc(format));

        public bool GetDataPresent(DataFormatIdentify format)
            => GetDataPresent(format.GetFormatEtc());

        public bool GetDataPresent(FORMATETC format)
            => DataObject.QueryGetData(ref format) == 0;//S_OK
        #endregion GetDataPresent

        public DataObjectFormat[] GetFormats()
        {
            FORMATETC[] fe = new FORMATETC[1] { new FORMATETC() };
            var enumFormatEtc = DataObject.EnumFormatEtc(DATADIR.DATADIR_GET);
            if (enumFormatEtc == null) return null;
            enumFormatEtc.Reset();
            List<DataObjectFormat> fs = new List<DataObjectFormat>();
            while (enumFormatEtc.Next(1, fe, null) == 0) //S_OK
            {
                fs.Add(new DataObjectFormat(fe[0]));
                fe[0].cfFormat = 16;
                //TODO: ptdを開放する必要があるか不明
            }
            Marshal.ReleaseComObject(enumFormatEtc);

            return fs.ToArray();
        }

        #region GetStream
        public Stream GetStream(DataFormatIdentify format, int index = -1, DVASPECT aspect = DVASPECT.DVASPECT_CONTENT)
            => GetStream(format.GetFormatEtc(index, aspect));

        public Stream GetStream(int formatId , int index = -1 , DVASPECT aspect = DVASPECT.DVASPECT_CONTENT)
            => GetStream(DataObjectUtils.GetFormatEtc(formatId,index,aspect));

        public Stream GetStream(string dataFormat, int index = -1, DVASPECT aspect = DVASPECT.DVASPECT_CONTENT)
            => GetStream(DataObjectUtils.GetFormatEtc(dataFormat,index,aspect));

        public Stream GetStream(FORMATETC format)
        {
            Stream st;
            if (_cache.TryGetValue(format.cfFormat , out st))return st;

            STGMEDIUM s = default(STGMEDIUM);

            try
            {
                DataObject.GetData(ref format, out s);
                //Todo: Not implemented ISTORAGE
                if (s.tymed == TYMED.TYMED_ISTORAGE) return new MemoryStream();
                st = s.GetManagedStream();
                _cache.Add(format.cfFormat,st);
                return st;
            }
            finally
            {
                s.Release();
            }
        }

        #endregion GetStream

        #region GetString
        public string GetString(DataFormatIdentify format, Encoding encoding)
            => GetString(format.GetFormatEtc(), encoding);

        public string GetString(int format, Encoding encoding)
            => GetString(DataObjectUtils.GetFormatEtc(format), encoding);

        public string GetString(string dataFormat, Encoding encoding)
            => GetString(DataObjectUtils.GetFormatEtc(dataFormat), encoding);

        public string GetString(FORMATETC dataFormat, Encoding encoding)
        {
            //TODO:安全なものについてはSTGMEDIUM.GetStringUnsafeを使う
            var s = GetStream(dataFormat);
            byte[] b = null;
            if (s is MemoryStream)
            {
                ((MemoryStream) s).GetBuffer();
            }
            else
            {
                b = new byte[s.Length];
                s.Read(b, 0, (int) s.Length);
            }
            return encoding.GetString(b);
        }

        #endregion GetString

        public string[] GetFiles()
        {
            var f = DataObjectUtils.GetFormatEtc( (int)CLIPFORMAT.CF_HDROP );
            STGMEDIUM s = default(STGMEDIUM);

            try
            {
                DataObject.GetData(ref f, out s);
                return s.GetFiles();
            }
            finally
            {
                s.Release();
            }
        }

        public IEnumerable<FileDescriptor> GetFileDescriptors()
            => FileDescriptor.FromFileGroupDescriptor(GetStream(DataFormatIdentifies.CFSTR_FILEDESCRIPTORW));

        public Stream GetFileContent( int index ) => GetStream(DataFormatIdentifies.CFSTR_FILECONTENTS, index);

        public Stream[] GetFileContents()
        {
            var fd = GetFileDescriptors();
            Stream[] s = new Stream[fd.Count()];
            int i = 0;
            foreach (var f in fd)
            {
                s[i] = GetFileContent(i);
                i++;
            }
            return s;
        }

    }
}
