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

        public ComDataObject(IComDataObject dataObject)
        {
            DataObject = dataObject;
        }

        #region GetCanonicalFormatEtc

        public virtual FORMATETC GetCanonicalFormatEtc(int format)
            => GetCanonicalFormatEtc(DataObjectUtils.GetFormatEtc(format));

        public virtual FORMATETC GetCanonicalFormatEtc(string format)
            => GetCanonicalFormatEtc(DataObjectUtils.GetFormatEtc(format));

        public virtual FORMATETC GetCanonicalFormatEtc(DataFormatIdentify format)
            => GetCanonicalFormatEtc(format.GetFormatEtc());

        public virtual FORMATETC GetCanonicalFormatEtc(FORMATETC format)
        {
            FORMATETC c;
            var re = DataObject.GetCanonicalFormatEtc(ref format, out c);
            return c;
        }
        #endregion

        #region GetDataPresent

        public virtual bool GetDataPresent(int format)
            => GetDataPresent(DataObjectUtils.GetFormatEtc(format));

        public virtual bool GetDataPresent(string format)
            => GetDataPresent(DataObjectUtils.GetFormatEtc(format));

        public virtual bool GetDataPresent(DataFormatIdentify format)
            => GetDataPresent(format.GetFormatEtc());

        public virtual bool GetDataPresent(FORMATETC format)
            => DataObject.QueryGetData(ref format) == 0;//S_OK
        #endregion GetDataPresent
        

        public virtual DataObjectFormat[] GetFormats(bool allFormat = false)
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
            if (allFormat)
            {

                FORMATETC f = new FORMATETC();
                f.tymed = TYMED.TYMED_ISTORAGE | TYMED.TYMED_ENHMF | TYMED.TYMED_FILE | TYMED.TYMED_HGLOBAL |
                          TYMED.TYMED_ISTREAM | TYMED.TYMED_MFPICT | TYMED.TYMED_FILE;
                f.dwAspect = DVASPECT.DVASPECT_CONTENT;
                f.lindex = -1;
                for (int i = short.MinValue; i < short.MaxValue; i++)
                {
                    f.cfFormat = (short) i;
                    if (DataObject.QueryGetData(ref f) == 0)
                    {
                        FORMATETC f2;
                        DataObject.GetCanonicalFormatEtc(ref f, out f2);
                        fs.Add(new DataObjectFormat(f2, null, true));
                    }
                }
            }

            return fs.ToArray();
        }

        #region GetStream
        public virtual Stream GetStream(DataFormatIdentify format, int index = -1, DVASPECT aspect = DVASPECT.DVASPECT_CONTENT)
            => GetStream(format.GetFormatEtc(index, aspect));

        public virtual Stream GetStream(int formatId, int index = -1, DVASPECT aspect = DVASPECT.DVASPECT_CONTENT)
            => GetStream(DataObjectUtils.GetFormatEtc(formatId, index, aspect));

        public virtual Stream GetStream(string dataFormat, int index = -1, DVASPECT aspect = DVASPECT.DVASPECT_CONTENT)
            => GetStream(DataObjectUtils.GetFormatEtc(dataFormat, index, aspect));

        public virtual Stream GetStream(FORMATETC format)
        {
            Stream st;
            if (_cache.TryGetValue(format.cfFormat, out st)) return st;

            STGMEDIUM s = default(STGMEDIUM);

            try
            {
                DataObject.GetData(ref format, out s);
                //Todo: Not implemented ISTORAGE
                if (s.tymed == TYMED.TYMED_ISTORAGE) throw new NotImplementedException();    //return new MemoryStream();
                st = s.GetManagedStream();
                _cache.Add(format.cfFormat, st);
                return st;
            }
            finally
            {
                s.Release();
            }
        }

        #endregion GetStream

        #region GetString
        public virtual string GetString(DataFormatIdentify format, Encoding encoding)
            => GetString(format.GetFormatEtc(), encoding);

        public virtual string GetString(int format, Encoding encoding)
            => GetString(DataObjectUtils.GetFormatEtc(format), encoding);

        public virtual string GetString(string dataFormat, Encoding encoding)
            => GetString(DataObjectUtils.GetFormatEtc(dataFormat), encoding);

        public virtual string GetString(FORMATETC dataFormat, Encoding encoding)
        {
            //TODO:安全なものについてはSTGMEDIUM.GetStringUnsafeを使う
            var s = GetStream(dataFormat);
            byte[] b = null;
            if (s is MemoryStream)
            {
                ((MemoryStream)s).GetBuffer();
            }
            else
            {
                b = new byte[s.Length];
                s.Read(b, 0, (int)s.Length);
            }
            return encoding.GetString(b);
        }

        #endregion GetString

        public virtual string[] GetFiles()
        {
            var f = DataObjectUtils.GetFormatEtc((int)CLIPFORMAT.CF_HDROP);
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

        public virtual IEnumerable<FileDescriptor> GetFileDescriptors()
            => FileDescriptor.FromFileGroupDescriptor(GetStream(DataFormatIdentifies.CFSTR_FILEDESCRIPTORW));

        public virtual Stream GetFileContent(int index) => GetStream(DataFormatIdentifies.CFSTR_FILECONTENTS, index);

        public virtual Stream[] GetFileContents()
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
