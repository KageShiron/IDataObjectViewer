using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace DataFormatLib
{
    public static class DataObjectUtils
    {
        [DllImport("USER32.DLL", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetClipboardFormatName(int format
            , [Out] StringBuilder lpszFormatName, int cchMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto,SetLastError = true)]
        static extern int RegisterClipboardFormat(string lpszFormat);

        public static string GetFormatName(int formatId)
        {
            StringBuilder sb = new StringBuilder(260);
            if (GetClipboardFormatName(formatId, sb, 260) == 0) return "";//$"Format{formatId}";
            return sb.ToString();
        }

        public static int GetFormatId(string formatName)
        {
            //if (formatName.StartsWith("Format")) return int.Parse(formatName.Substring(6));
            int id = RegisterClipboardFormat(formatName);
            if(id == 0)throw new Win32Exception();
            return id;
        }


        public static FORMATETC GetFormatEtc(short id, int lindex = -1, DVASPECT dwAspect = DVASPECT.DVASPECT_CONTENT)
        {
            FORMATETC f = new FORMATETC
            {
                cfFormat = id,
                dwAspect = dwAspect,
                lindex = lindex,
                tymed = TYMED.TYMED_HGLOBAL | TYMED.TYMED_GDI | TYMED.TYMED_ISTREAM | TYMED.TYMED_ISTORAGE |
                        TYMED.TYMED_GDI | TYMED.TYMED_FILE | TYMED.TYMED_MFPICT | TYMED.TYMED_ENHMF
            };
            return f;
        }

        public static FORMATETC GetFormatEtc(string dataFormat, int lindex = -1, DVASPECT dwAspect = DVASPECT.DVASPECT_CONTENT)
            => GetFormatEtc((short)DataObjectUtils.GetFormatId(dataFormat), lindex,dwAspect);

        public static FORMATETC GetFormatEtc(int id, int lindex = -1, DVASPECT dwAspect = DVASPECT.DVASPECT_CONTENT)
            => GetFormatEtc((short)id, lindex, dwAspect);
        
    }


    public enum CLIPFORMAT : int
    {
        CF_TEXT = 1,
        CF_BITMAP = 2,
        CF_METAFILEPICT = 3,
        CF_SYLK = 4,
        CF_DIF = 5,
        CF_TIFF = 6,
        CF_OEMTEXT = 7,
        CF_DIB = 8,
        CF_PALETTE = 9,
        CF_PENDATA = 10,
        CF_RIFF = 11,
        CF_WAVE = 12,
        CF_UNICODETEXT = 13,
        CF_ENHMETAFILE = 14,
        CF_HDROP = 15,
        CF_LOCALE = 16,
        CF_DIBV5 = 17,
        CF_OWNERDISPLAY = 0x80,
        CF_DSPTEXT = 0x81,
        CF_DSPBITMAP = 0x82,
        CF_DSPMETAFILEPICT = 0x83,
        CF_DSPENHMETAFILE = 0x8E,
    }
}
