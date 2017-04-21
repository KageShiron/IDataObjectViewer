using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using Be.Windows.Forms;
using DataFormatLib;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.ObjectExtensions;

namespace ClipboardPeek
{
    public class ViewModel : INotifyDataErrorInfo
    {
        private Model model;
        private HexBox hex;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ReactiveProperty<IEnumerable<FormatItem>> Formats { get; private set; }
        public ReactiveCommand LoadFromClipboardCommand { get; }
        public ReactiveProperty<string> Text { get; private set; }
        public ReactiveProperty<string> Rtf { get; private set; }

        public ReactiveProperty<FormatItem> SelectedFormat { get; private set; }

        public ReactiveProperty<IEnumerable<KeyValuePair<string, string>>> BasicInfo { get; set; }

        public ReactiveProperty<Encoding> SelectedEncoding { get; set; }
        public ReactiveProperty<object> Files { get; private set; }
        public ReactiveProperty<ImageSource> Image { get; private set; }
        public ReactiveProperty<System.Windows.Ink.StrokeCollection> InkStrokes { get; private set; }
        public ReactiveProperty<Visibility> ImageVisibility { get; private set; }
        public ReactiveProperty<Visibility> InkCanvasVisibility { get; private set; }

        public IEnumerable<Encoding> Encodings { get; set; } = new Encoding[]
        {
            Encoding.ASCII,
            Encoding.UTF8,
            Encoding.Unicode,
            Encoding.BigEndianUnicode,
            Encoding.UTF32,
            Encoding.GetEncoding(932),
            Encoding.GetEncoding("euc-jp")
        };

        public bool HasErrors
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private static IEnumerable<KeyValuePair<string,string>> CreateBasicInfo(FormatItem formatItem)
        {
            try
            {
                var s = formatItem.Format;
                var dic = new Dictionary<string, string>()
                {
                    {"ID(Dec)", s.FormatId.Id.ToString()},
                    {"ID(Hex)", ((short) s.FormatId.Id).ToString("X4")},
                    {"Name", s.FormatId.NativeName},
                    {".NET Name", s.FormatId.DotNetName},
                    {"Tymed", s.Tymed.ToString()},
                    {"Ptd", "0x"+s.PtdNull.ToString("X"+(IntPtr.Size*2))},
                    {"DvAspect", s.DvAspect.ToString()},
                    {"Canonical", s.Canonical.ToString()},
                    {"Size", formatItem.Stream.Length.ToString("#,0")},
                    {"WellKnown", s.FormatId.ConstantName},
                };

                if (formatItem.Format.FormatId.Id == DataFormatIdentifies.CF_LOCALE.Id)
                {
                    formatItem.Stream.Seek(0, SeekOrigin.Begin);
                    using (var br = new BinaryReader(formatItem.Stream, Encoding.ASCII, true))
                    {
                        var c = new CultureInfo(br.ReadInt32());
                        dic.Add("-------","■CF_LOCALEの個別情報■");
                        dic.Add("LCID", c.LCID.ToString());
                        dic.Add("Display Name", c.DisplayName);
                        dic.Add("CodePage", c.TextInfo.OEMCodePage.ToString());
                    }

                }
                return dic;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string GetString(FormatItem item, Encoding enc)
        {
            if (item != null && item.Error != null)
            {
                return item.Error.ToString();
            }
            if (item?.Stream == null) return "";
            item.Stream.Seek(0, SeekOrigin.Begin);
            using (var b = new StreamReader(item.Stream, enc, true, 1024, true))
            {
                /*char[] str = new char[5000];
                int count = b.Read(str, 0, 5000);
                if (count <= 0) return "";*/
                
                return b.ReadToEnd();//new string(str, 0, count - 1) + (count >= 5000 ? "\n\r(省略されました)" : "");
            }
        }

        private static StrokeCollection GetStrokeCollection( FormatItem x )
        {
            if (x?.Format?.FormatId?.NativeName == "Ink Serialized Format")
            {
                x.Stream.Seek(0, SeekOrigin.Begin);
                var y = new StrokeCollection(x.Stream);
                return y;
            }
            return null;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            throw new NotImplementedException();
        }

        public ViewModel( HexBox hex )
        {
            this.hex = hex;
            model = new Model();
            this.Formats = model.ObserveProperty(x => x.DataObjectFormats).ToReactiveProperty();

            SelectedFormat = new ReactiveProperty<FormatItem>();

            LoadFromClipboardCommand = new ReactiveCommand();
            LoadFromClipboardCommand.Subscribe( _ => model.LoadFromClipboard() );

            BasicInfo = SelectedFormat.Select(CreateBasicInfo).ToReactiveProperty();
            SelectedEncoding = new ReactiveProperty<Encoding>( Encoding.Unicode );
            Text = SelectedFormat.CombineLatest(SelectedEncoding, GetString).ToReactiveProperty();

            Files = SelectedFormat.Select(x => x?.Excetra).ToReactiveProperty();
            Image = SelectedFormat.Select(x => x?.Excetra as ImageSource).ToReactiveProperty();
            InkStrokes = SelectedFormat.Select(GetStrokeCollection).ToReactiveProperty();

            ImageVisibility = Image.Select(x => x == null ? Visibility.Collapsed : Visibility.Visible).ToReactiveProperty();
            InkCanvasVisibility =　InkStrokes.Select(x => x == null ? Visibility.Collapsed : Visibility.Visible)
                    .ToReactiveProperty();
            Rtf = SelectedFormat.Select(x => ((x?.Format?.FormatId?.NativeName == "Rich Text Format") ? GetString(x, Encoding.UTF8) : "")).ToReactiveProperty();

            SelectedFormat.PropertyChanged += (s, e) =>
            {
                if (SelectedFormat?.Value?.Stream == null)
                {
                    hex.ByteProvider = new DynamicFileByteProvider(Stream.Null);
                    return;
                }
                var mem = new MemoryStream((int) SelectedFormat.Value.Stream.Length);
                this.SelectedFormat.Value.Stream.Seek(0, SeekOrigin.Begin);
                this.SelectedFormat.Value.Stream.CopyTo(mem);
                hex.ByteProvider = new DynamicFileByteProvider(mem);
            };
        }

    }
}
