using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DataFormatLib;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.ObjectExtensions;

namespace ClipboardPeek
{
    public class ViewModel
    {
        private Model model;

        public ReactiveProperty<IEnumerable<FormatItem>> Formats { get; private set; }
        public ReactiveCommand LoadFromClipboardCommand { get; }
        public ReactiveProperty<string> Text { get; private set; }

        public ReactiveProperty<FormatItem> SelectedFormat { get; private set; }

        public ReactiveProperty<IEnumerable<KeyValuePair<string, string>>> BasicInfo { get; set; }

        public ReactiveProperty<Encoding> SelectedEncoding { get; set; }
        public ReactiveProperty<object> Files { get; private set; }
        public ReactiveProperty<ImageSource> Image { get; private set; }

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
                    //{"Size", s.Binary?.Length.ToString()},
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
                char[] str = new char[5000];
                int count = b.Read(str, 0, 5000);
                if (count <= 0) return "";
                return new string(str, 0, count - 1) + (count >= 5000 ? "\n\r(省略されました)" : "");
            }
        }
    

        public ViewModel()
        {

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

        }

    }
}
