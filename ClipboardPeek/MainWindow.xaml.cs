using System;
using System.Collections.Generic;
using System.Linq;
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
using Fluent;

namespace ClipboardPeek
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
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
            HexGrid.Children.Add(host);

            DataContext = new ViewModel(hex);
        }
    }
}
