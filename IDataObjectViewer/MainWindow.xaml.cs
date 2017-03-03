using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Be.Windows.Forms;
using DataFormatLib;
using DataFormatLibWPF;
using IDataObjectViewer;
using Microsoft.Practices.Prism.Mvvm;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using MessageBox = System.Windows.Forms.MessageBox;

namespace IDataObjectViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog logger =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [DllImport("ole32.dll")]
        static extern int OleInitialize(IntPtr pvReserved);

        public MainWindow()
        {
            OleInitialize(IntPtr.Zero);
            InitializeComponent();
            WindowsFormsHost host = new WindowsFormsHost();
            HexBox hex = new HexBox();
            hex.ColumnInfoVisible = true;
            hex.LineInfoVisible = true;
            hex.StringViewVisible = true;
            hex.UseFixedBytesPerLine = true;
            hex.VScrollBarVisible = true;
            hex.ReadOnly = true;
            host.Child = hex;
            this.hexGrid.Children.Add(host);
            DataContext = new FormatViewModel(hex);

        }



        private void Listbox_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var x = new ComDataObjectWpf(e.Data as System.Runtime.InteropServices.ComTypes.IDataObject);
                (DataContext as FormatViewModel).Formats = DataObjectFormatModel.FromDataObjectFormats(x);
                
            }
            catch(Exception ex)
            {
                throw;
            }


        }


        [DllImport("ole32.dll")]
        static extern int OleGetClipboard(out System.Runtime.InteropServices.ComTypes.IDataObject ppDataObj);

        private void Clipboard_Click(object sender, RoutedEventArgs e)
        {
/*
            throw new ApplicationException("");
            var d = System.Windows.Forms.Clipboard.GetDataObject();
            var n = d.GetType()
                .GetField("innerData", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)
                .GetValue(d);
            var m = n.GetType()
                .GetField("innerData", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)
                .GetValue(n);
            var x = new DataObjectManager( m as System.Runtime.InteropServices.ComTypes.IDataObject);*/

            IDataObject data;
            OleGetClipboard(out data);
            var x = new ComDataObjectWpf(data);

            (DataContext as FormatViewModel).Formats = DataObjectFormatModel.FromDataObjectFormats(x);
        }
    }

    
    public class DecHexConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            var o = value as DataObjectFormatModel;
            
            return string.Format("0x{0:X4} ({0})", (short)o.DataObjectFormat.FormatId);
        }

        public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DataObjectFormatModel
    {
        public DataObjectFormat DataObjectFormat { get; }
        public Stream Binary { get; }
        public object Files { get; }

        public DataObjectFormatModel( DataObjectFormat f , Stream binary = null , object files = null )
        {
            this.DataObjectFormat = f;
            this.Binary = binary;
            this.Files = files;
        }

        public static IEnumerable<DataObjectFormatModel> FromDataObjectFormats( ComDataObjectWpf obj)
        {
            var fmts = obj.GetFormats();
            var models = new List<DataObjectFormatModel>(fmts.Length);
            foreach (var f in fmts)
            {
                try
                {
                    if (f.FormatName == "FileContents")
                    {
                        models.Add(new DataObjectFormatModel(f));
                    }
                    else
                    {
                        var stream = obj.GetStream(f.FormatId);
                        if (f.FormatId == (int) CLIPFORMAT.CF_HDROP)
                        {
                            models.Add(new DataObjectFormatModel(f, stream, obj.GetFiles()));
                        }
                        else if (f.FormatName == "FileGroupDescriptorW")
                        {
                            models.Add(new DataObjectFormatModel(f, stream,
                                FileDescriptor.FromFileGroupDescriptor(stream)));
                            stream.Position = 0;
                        }
                        else
                        {
                            models.Add(new DataObjectFormatModel(f, stream));
                        }
                    }
                }catch
                {
                    models.Add(new DataObjectFormatModel(f));
                }
            }
            return models;
        }
    }

    public class FormatViewModel : BindableBase
    {
        private HexBox _hex;
        public FormatViewModel(HexBox hex)
        {
            _hex = hex;
        }
        public IEnumerable<DataObjectFormatModel> Formats
        {
            get { return _Formats; }
            set { SetProperty(ref _Formats, value); }
        }

        IEnumerable<DataObjectFormatModel> _Formats;

        public object Files => this.SelectedFormat?.Files;

        private Encoding[] mainEncodings = new Encoding[]
        {
            Encoding.ASCII,
            Encoding.UTF8,
            Encoding.Unicode,
            Encoding.GetEncoding(932),
            Encoding.GetEncoding(51932),
        };

        public IEnumerable<KeyValuePair<string, string>> Locale
        {
            get
            {
                if( SelectedFormat == null || SelectedFormat.DataObjectFormat.FormatName != DataFormats.Locale) return null;
                SelectedFormat.Binary.Seek(0, SeekOrigin.Begin);
                var c = new CultureInfo(new BinaryReader(SelectedFormat.Binary).ReadInt32());
                return new Dictionary<string, string>()
                {
                    {"LCID", c.LCID.ToString()},
                    {"Display Name", c.DisplayName},
                    {"CodePage", c.TextInfo.OEMCodePage.ToString()}
                };
            }
        }


        public IEnumerable<KeyValuePair<string, string>> BasicInfo
        {
            get
            {
                try
                {
                    var s = SelectedFormat;
                    return new Dictionary<string, string>()
                    {
                        {"ID(Dec)", s.DataObjectFormat.FormatId.ToString()},
                        {"ID(Hex)", ((short) s.DataObjectFormat.FormatId).ToString("X4")},
                        {"Name", s.DataObjectFormat.FormatName},
                        {"Tymed", s.DataObjectFormat.Tymed.ToString()},
                        {"IsPtdNull", s.DataObjectFormat.IsPtdNull.ToString()},
                        {"DvAspect", s.DataObjectFormat.DvAspect.ToString()},
                        {"Canonical", s.DataObjectFormat.Canonical.ToString()},
                        {"Size", s.Binary?.Length.ToString()},
                        {"WellKnown", s.DataObjectFormat.WellKnownName},
                    };
                }
                catch (Exception ex)
                {
                    return null;
                }
                /*return new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("ID(Dec)", s.FormatId.ToString()),
                    new KeyValuePair<string, string>("ID(Hex)", ((short)s.FormatId).ToString("X4")),
                    new KeyValuePair<string, string>("Name", s.FormatName),
                    new KeyValuePair<string, string>("Tymed", s.Tymed.ToString()),
                    new KeyValuePair<string, string>("IsPtdNull", s.IsPtdNull.ToString()),
                    new KeyValuePair<string, string>("DvAspect", s.DvAspect.ToString()),
                    new KeyValuePair<string, string>("Canonical", s.Canonical.ToString()),
                    new KeyValuePair<string, string>("Size", s.Binary.Length.ToString()),
                    new KeyValuePair<string, string>("WellKnown", s.WellKnown),
                };*/
            }
        }

        public object KnownData => "";//selectedFormat.KnownData;



        private DataObjectFormatModel selectedFormat;

        public DataObjectFormatModel SelectedFormat
        {
            get { return selectedFormat; }
            set
            {
                SetProperty(ref selectedFormat, value);
                OnPropertyChanged("Text");
                OnPropertyChanged(nameof(BasicInfo));
                OnPropertyChanged("Files");
                OnPropertyChanged(nameof(Locale));

                if (this.selectedFormat.Binary != null)
                {
                    var mem = new MemoryStream((int) this.selectedFormat.Binary.Length);
                    this.selectedFormat.Binary.Seek(0, SeekOrigin.Begin);
                    this.selectedFormat.Binary.CopyTo(mem);
                    _hex.ByteProvider = new DynamicFileByteProvider(mem);
                }
            }
        }


        public IEnumerable<Encoding> Encodings
        {
            get
            {
                return ShowAllEncodings ? Encoding.GetEncodings().Select(x => x.GetEncoding()) : mainEncodings;
            }
        }

        public bool ShowAllEncodings
        {
            get { return _MainEncodingOnly; }
            set
            {
                SetProperty(ref _MainEncodingOnly, value);
                OnPropertyChanged(nameof(Encodings));
            }
        }

        bool _MainEncodingOnly = false;


        private Encoding selectedEncoding = Encoding.Unicode;

        public Encoding SelectedEncoding
        {
            get { return selectedEncoding; }
            set
            {
                SetProperty(ref selectedEncoding, value);
                OnPropertyChanged("Text");
            }
        }

        public string Text
        {
            get
            {
                if (SelectedFormat?.Binary != null)
                {
                    SelectedFormat.Binary.Seek(0, SeekOrigin.Begin);
                    using (
                        var b = new StreamReader(SelectedFormat.Binary, SelectedEncoding, true, 1024,
                            true))
                    {
                        char[] str = new char[5000];
                        int count = b.Read(str, 0, 5000);
                        return new string(str, 0, count - 1) + (count >= 5000 ? "\n\r(省略されました)" : "");
                    }
                }
                else
                    return "";
            }
        }




    }

    enum AnalyzeTab
    {
        BasicInfo,
        String,

    }

}