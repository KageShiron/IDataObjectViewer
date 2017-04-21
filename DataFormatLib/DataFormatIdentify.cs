using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace DataFormatLib
{
    public static class DataFormatIdentifies
    {
        private static List<DataFormatIdentify> _formats;
        private static object _formatsLock = new object();

        /// <summary>
        /// Create new DataFormatIdentify and add cache.
        /// </summary>
        /// <param name="nativeName"></param>
        /// <returns></returns>
        private static DataFormatIdentify CreateDataFormatIdentify(string nativeName)
        {
            lock (_formatsLock)
            {
                var id = new DataFormatIdentify(DataObjectUtils.GetFormatId(nativeName), nativeName, "");
                _formats.Add(id);
                return id;
            }
        }

        /// <summary>
        /// Create new DataFormatIdentify and add cache.
        /// </summary>
        /// <param name="nativeName"></param>
        /// <returns></returns>
        private static DataFormatIdentify CreateDataFormatIdentify(int id)
        {
            lock (_formatsLock)
            {
                string name = DataObjectUtils.GetFormatName(id);
                var cid = new DataFormatIdentify(id, name, "");
                _formats.Add(cid);
                return cid;
            }
        }

        internal static DataFormatIdentify FromNativeName(string nativeName)
        {
            EnsurePredefined();
            lock (_formatsLock)
            {
                var val = _formats.FirstOrDefault(
                    x => string.Compare(x.NativeName, nativeName, StringComparison.OrdinalIgnoreCase) == 0);
                return val ?? CreateDataFormatIdentify(nativeName);
            }
        }

        internal static DataFormatIdentify FromId(int id)
        {
            EnsurePredefined();
            lock (_formatsLock)
            {
                var val = _formats.FirstOrDefault(x => x.Id == id);
                return val ?? CreateDataFormatIdentify(id);
            }
        }

        private static void EnsurePredefined()
        {
            lock (_formatsLock)
            {
                if (_formats != null) return;
                var fmts = Enum.GetValues(typeof(CLIPFORMAT));
                _formats = new List<DataFormatIdentify>(fmts.Length);
                foreach (CLIPFORMAT c in fmts)
                {
                    _formats.Add(new DataFormatIdentify((int) c, "", c.ToString()));
                }
                _formats.AddRange(new[]
                {
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Shell IDList Array"), "Shell IDList Array",
                        "CFSTR_SHELLIDLIST"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Shell Object Offsets"),
                        "Shell Object Offsets", "CFSTR_SHELLIDLISTOFFSET"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Net Resource"), "Net Resource",
                        "CFSTR_NETRESOURCES"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("FileGroupDescriptor"), "FileGroupDescriptor",
                        "CFSTR_FILEDESCRIPTORA"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("FileGroupDescriptorW"),
                        "FileGroupDescriptorW", "CFSTR_FILEDESCRIPTORW"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("FileContents"), "FileContents",
                        "CFSTR_FILECONTENTS"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("FileNameW"), "FileNameW", "CFSTR_FILENAMEW"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("PrinterFriendlyName"), "PrinterFriendlyName",
                        "CFSTR_PRINTERGROUP"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("FileNameMap"), "FileNameMap",
                        "CFSTR_FILENAMEMAPA"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("FileNameMapW"), "FileNameMapW",
                        "CFSTR_FILENAMEMAPW"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("UniformResourceLocator"),
                        "UniformResourceLocator", "CFSTR_SHELLURL"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("UniformResourceLocator"),
                        "UniformResourceLocator", "CFSTR_INETURLA"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("UniformResourceLocatorW"),
                        "UniformResourceLocatorW", "CFSTR_INETURLW"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Preferred DropEffect"),
                        "Preferred DropEffect", "CFSTR_PREFERREDDROPEFFECT"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Performed DropEffect"),
                        "Performed DropEffect", "CFSTR_PERFORMEDDROPEFFECT"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Paste Succeeded"), "Paste Succeeded",
                        "CFSTR_PASTESUCCEEDED"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("InShellDragLoop"), "InShellDragLoop",
                        "CFSTR_INDRAGLOOP"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("MountedVolume"), "MountedVolume",
                        "CFSTR_MOUNTEDVOLUME"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("PersistedDataObject"), "PersistedDataObject",
                        "CFSTR_PERSISTEDDATAOBJECT"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("TargetCLSID"), "TargetCLSID",
                        "CFSTR_TARGETCLSID"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Logical Performed DropEffect"),
                        "Logical Performed DropEffect", "CFSTR_LOGICALPERFORMEDDROPEFFECT"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Autoplay Enumerated IDList Array"),
                        "Autoplay Enumerated IDList Array", "CFSTR_AUTOPLAY_SHELLIDLISTS"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("UntrustedDragDrop"), "UntrustedDragDrop",
                        "CFSTR_UNTRUSTEDDRAGDROP"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("File Attributes Array"),
                        "File Attributes Array", "CFSTR_FILE_ATTRIBUTES_ARRAY"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("InvokeCommand DropParam"),
                        "InvokeCommand DropParam", "CFSTR_INVOKECOMMAND_DROPPARAM"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("DropHandlerCLSID"), "DropHandlerCLSID",
                        "CFSTR_SHELLDROPHANDLER"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("DropDescription"), "DropDescription",
                        "CFSTR_DROPDESCRIPTION"),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("ZoneIdentifier"), "ZoneIdentifier",
                        "CFSTR_ZONEIDENTIFIER"),


                    new DataFormatIdentify(DataObjectUtils.GetFormatId("Xaml"), "Xaml",
                        ""),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("XamlPackage"), "XamlPackage",
                        ""),
                    new DataFormatIdentify(DataObjectUtils.GetFormatId("ApplicationTrust"), "ApplicationTrust",
                        ""),

                });
            }
        }

        public static readonly DataFormatIdentify CF_TEXT = DataFormatIdentifies.FromId(1);
        public static readonly DataFormatIdentify CF_BITMAP = DataFormatIdentifies.FromId(2);
        public static readonly DataFormatIdentify CF_METAFILEPICT = DataFormatIdentifies.FromId(3);
        public static readonly DataFormatIdentify CF_SYLK = DataFormatIdentifies.FromId(4);
        public static readonly DataFormatIdentify CF_DIF = DataFormatIdentifies.FromId(5);
        public static readonly DataFormatIdentify CF_TIFF = DataFormatIdentifies.FromId(6);
        public static readonly DataFormatIdentify CF_OEMTEXT = DataFormatIdentifies.FromId(7);
        public static readonly DataFormatIdentify CF_DIB = DataFormatIdentifies.FromId(8);
        public static readonly DataFormatIdentify CF_PALETTE = DataFormatIdentifies.FromId(9);
        public static readonly DataFormatIdentify CF_PENDATA = DataFormatIdentifies.FromId(10);
        public static readonly DataFormatIdentify CF_RIFF = DataFormatIdentifies.FromId(11);
        public static readonly DataFormatIdentify CF_WAVE = DataFormatIdentifies.FromId(12);
        public static readonly DataFormatIdentify CF_UNICODETEXT = DataFormatIdentifies.FromId(13);
        public static readonly DataFormatIdentify CF_ENHMETAFILE = DataFormatIdentifies.FromId(14);
        public static readonly DataFormatIdentify CF_HDROP = DataFormatIdentifies.FromId(15);
        public static readonly DataFormatIdentify CF_LOCALE = DataFormatIdentifies.FromId(16);
        public static readonly DataFormatIdentify CF_DIBV5 = DataFormatIdentifies.FromId(17);
        public static readonly DataFormatIdentify CF_OWNERDISPLAY = DataFormatIdentifies.FromId(0x80);
        public static readonly DataFormatIdentify CF_DSPTEXT = DataFormatIdentifies.FromId(0x81);
        public static readonly DataFormatIdentify CF_DSPBITMAP = DataFormatIdentifies.FromId(0x82);
        public static readonly DataFormatIdentify CF_DSPMETAFILEPICT = DataFormatIdentifies.FromId(0x83);
        public static readonly DataFormatIdentify CF_DSPENHMETAFILE = DataFormatIdentifies.FromId(0x8E);

        public static readonly DataFormatIdentify CFSTR_SHELLIDLIST = DataFormatIdentifies.FromNativeName("Shell IDList Array");
        public static readonly DataFormatIdentify CFSTR_SHELLIDLISTOFFSET = DataFormatIdentifies.FromNativeName("Shell Object Offsets");
        public static readonly DataFormatIdentify CFSTR_NETRESOURCES = DataFormatIdentifies.FromNativeName("Net Resource");
        public static readonly DataFormatIdentify CFSTR_FILEDESCRIPTORA = DataFormatIdentifies.FromNativeName("FileGroupDescriptor");
        public static readonly DataFormatIdentify CFSTR_FILEDESCRIPTORW = DataFormatIdentifies.FromNativeName("FileGroupDescriptorW");
        public static readonly DataFormatIdentify CFSTR_FILECONTENTS = DataFormatIdentifies.FromNativeName("FileContents");
        public static readonly DataFormatIdentify CFSTR_FILENAMEA = DataFormatIdentifies.FromNativeName("FileName");
        public static readonly DataFormatIdentify CFSTR_FILENAMEW = DataFormatIdentifies.FromNativeName("FileNameW");
        public static readonly DataFormatIdentify CFSTR_PRINTERGROUP = DataFormatIdentifies.FromNativeName("PrinterFriendlyName");
        public static readonly DataFormatIdentify CFSTR_FILENAMEMAPA = DataFormatIdentifies.FromNativeName("FileNameMap");
        public static readonly DataFormatIdentify CFSTR_FILENAMEMAPW = DataFormatIdentifies.FromNativeName("FileNameMapW");
        public static readonly DataFormatIdentify CFSTR_SHELLURL = DataFormatIdentifies.FromNativeName("UniformResourceLocator");
        public static readonly DataFormatIdentify CFSTR_INETURLA = DataFormatIdentifies.FromNativeName("UniformResourceLocator");
        public static readonly DataFormatIdentify CFSTR_INETURLW = DataFormatIdentifies.FromNativeName("UniformResourceLocatorW");
        public static readonly DataFormatIdentify CFSTR_PREFERREDDROPEFFECT = DataFormatIdentifies.FromNativeName("Preferred DropEffect");
        public static readonly DataFormatIdentify CFSTR_PERFORMEDDROPEFFECT = DataFormatIdentifies.FromNativeName("Performed DropEffect");
        public static readonly DataFormatIdentify CFSTR_PASTESUCCEEDED = DataFormatIdentifies.FromNativeName("Paste Succeeded");
        public static readonly DataFormatIdentify CFSTR_INDRAGLOOP = DataFormatIdentifies.FromNativeName("InShellDragLoop");
        public static readonly DataFormatIdentify CFSTR_MOUNTEDVOLUME = DataFormatIdentifies.FromNativeName("MountedVolume");
        public static readonly DataFormatIdentify CFSTR_PERSISTEDDATAOBJECT = DataFormatIdentifies.FromNativeName("PersistedDataObject");
        public static readonly DataFormatIdentify CFSTR_TARGETCLSID = DataFormatIdentifies.FromNativeName("TargetCLSID");
        public static readonly DataFormatIdentify CFSTR_LOGICALPERFORMEDDROPEFFECT = DataFormatIdentifies.FromNativeName("Logical Performed DropEffect");
        public static readonly DataFormatIdentify CFSTR_AUTOPLAY_SHELLIDLISTS = DataFormatIdentifies.FromNativeName("Autoplay Enumerated IDList Array");
        public static readonly DataFormatIdentify CFSTR_UNTRUSTEDDRAGDROP = DataFormatIdentifies.FromNativeName("UntrustedDragDrop");
        public static readonly DataFormatIdentify CFSTR_FILE_ATTRIBUTES_ARRAY = DataFormatIdentifies.FromNativeName("File Attributes Array");
        public static readonly DataFormatIdentify CFSTR_INVOKECOMMAND_DROPPARAM = DataFormatIdentifies.FromNativeName("InvokeCommand DropParam");
        public static readonly DataFormatIdentify CFSTR_SHELLDROPHANDLER = DataFormatIdentifies.FromNativeName("DropHandlerCLSID");
        public static readonly DataFormatIdentify CFSTR_DROPDESCRIPTION = DataFormatIdentifies.FromNativeName("DropDescription");
        public static readonly DataFormatIdentify CFSTR_ZONEIDENTIFIER = DataFormatIdentifies.FromNativeName("ZoneIdentifier");
    }

    public class DataFormatIdentify
        {
            /// <summary>
            /// データフォーマットの識別子
            /// </summary>
            public int Id { get; private set; }
            /// <summary>
            /// データフォーマットの名前。名前がない場合、String.Empty
            /// </summary>
            public string NativeName { get; private set; }
            /// <summary>
            /// windows.hで定数が定義されている場合、その名前。定数が定義されていない場合、String.Empty
            /// </summary>
            public string ConstantName { get; private set; }

            /// <summary>
            /// .NET FrameworkのSystem.Windows.DataFormat/System.Windows.Forms.DataFormats関連で呼ばれている名前を取得します。
            /// </summary>
            public string DotNetName
            {
                get
                {
                    switch ((CLIPFORMAT)Id)
                    {
                        case CLIPFORMAT.CF_TEXT:
                            return "Text";
                        case CLIPFORMAT.CF_UNICODETEXT:
                            return "UnicodeText";
                        case CLIPFORMAT.CF_DIB:
                            return "DeviceIndependentBitmap";
                        case CLIPFORMAT.CF_BITMAP:
                            return "Bitmap";
                        case CLIPFORMAT.CF_ENHMETAFILE:
                            return "EnhancedMetafile";
                        case CLIPFORMAT.CF_METAFILEPICT:
                            return "MetaFilePict";
                        case CLIPFORMAT.CF_SYLK:
                            return "SymbolicLink";
                        case CLIPFORMAT.CF_DIF:
                            return "DataInterchangeFormat";
                        case CLIPFORMAT.CF_TIFF:
                            return "TaggedImageFileFormat";
                        case CLIPFORMAT.CF_OEMTEXT:
                            return "OEMText";
                        case CLIPFORMAT.CF_PALETTE:
                            return "Palette";
                        case CLIPFORMAT.CF_PENDATA:
                            return "PenData";
                        case CLIPFORMAT.CF_RIFF:
                            return "RiffAudio";
                        case CLIPFORMAT.CF_WAVE:
                            return "WaveAudio";
                        case CLIPFORMAT.CF_HDROP:
                            return "FileDrop";
                        case CLIPFORMAT.CF_LOCALE:
                            return "Locale";
                    }
                    if (NativeName == "") return "Format" + Id;
                    return NativeName;
                }
            }

            internal DataFormatIdentify(int id, string name, string constantName = "")
            {
                Id = (short)id;
                NativeName = name;
                ConstantName = constantName;
            }

            public static DataFormatIdentify FromNativeName(string name)
            {
                return DataFormatIdentifies.FromNativeName(name);
            }

            public static DataFormatIdentify FromId(int id)
            {
                return DataFormatIdentifies.FromId(id);
            }

            public static DataFormatIdentify FromDotNetName(string name)
            {
                int num;
                if (name.StartsWith("Format") && int.TryParse(name.Substring(6) , out num)) return FromId(num);
                switch (name)
                {
                    case "Text":
                        return FromId((int) CLIPFORMAT.CF_TEXT);
                    case "UnicodeText":
                        return FromId((int) CLIPFORMAT.CF_UNICODETEXT);
                    case "DeviceIndependentBitmap":
                        return FromId((int) CLIPFORMAT.CF_DIB);
                    case "Bitmap":
                        return FromId((int) CLIPFORMAT.CF_BITMAP);
                    case "EnhancedMetafile":
                        return FromId((int) CLIPFORMAT.CF_ENHMETAFILE);
                    case "MetaFilePict":
                        return FromId((int) CLIPFORMAT.CF_METAFILEPICT);
                    case "SymbolicLink":
                        return FromId((int) CLIPFORMAT.CF_SYLK);
                    case "DataInterchangeFormat":
                        return FromId((int) CLIPFORMAT.CF_DIF);
                    case "TaggedImageFileFormat":
                        return FromId((int) CLIPFORMAT.CF_TIFF);
                    case "OEMText":
                        return FromId((int) CLIPFORMAT.CF_OEMTEXT);
                    case "Palette":
                        return FromId((int) CLIPFORMAT.CF_PALETTE);
                    case "PenData":
                        return FromId((int) CLIPFORMAT.CF_PENDATA);
                    case "RiffAudio":
                        return FromId((int) CLIPFORMAT.CF_RIFF);
                    case "WaveAudio":
                        return FromId((int) CLIPFORMAT.CF_WAVE);
                    case "FileDrop":
                        return FromId((int) CLIPFORMAT.CF_HDROP);
                    case "Locale":
                        return FromId((int) CLIPFORMAT.CF_LOCALE);
                }
                return FromNativeName(name);
            }

            public FORMATETC GetFormatEtc(int index = -1, DVASPECT aspect = DVASPECT.DVASPECT_CONTENT)
                => DataObjectUtils.GetFormatEtc(Id, index, aspect);

            public override string ToString()
            {
                return NativeName + (ConstantName != string.Empty ? $"(#{ConstantName})" : "");
            }
        }
    }
