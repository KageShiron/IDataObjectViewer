using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DataFormatLib;

namespace IDataObjectViewer
{
    class FileDescriptorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var o = value as FileDescriptor;
            if (o == null) return null;
            return new Dictionary<string, string>()
            {
                {"FileName", o.FileName},
                {"ClsID", o.Clsid.ToString()},
                {"Size", o.Size.ToString()},
                {"Point", o.Point.ToString()},
                {"FileAttributes", o.FileAttributes.ToString()},
                {"CreationTime", o.CreationTime.ToString()},
                {"LastAccessTime", o.CreationTime.ToString()},
                {"WriteTime", o.WriteTime.ToString()},
                {"FileSize", o.FileSize.ToString()}
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
