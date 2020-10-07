using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StationManager.Conventers
{
    class IDToName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = new IDToNameIndex();
            string v;
            if (MainWindow.nameTables.ContainsKey(parameter) && ((Dictionary<int, string>)MainWindow.nameTables[parameter]).TryGetValue((int)value, out v))
            {
                return v;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
