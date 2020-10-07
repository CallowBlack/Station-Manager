using StationManager.DataStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StationManager.Conventers
{
    class IDToNameIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.Equals(typeof(int)))
            {       
                if (MainWindow.nameTablesIDs.ContainsKey(parameter.ToString()))
                {
                    var nameTableIDs = (BiDictionary<int, int>)MainWindow.nameTablesIDs[parameter.ToString()];
                    int nameIndex;
                    if (nameTableIDs.TryGetByFirst((int)value, out nameIndex))
                        return nameIndex;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.Equals(typeof(int)))
            {
                if (MainWindow.nameTablesIDs.ContainsKey(parameter.ToString()))
                {
                    var nameTableIDs = (BiDictionary<int, int>)MainWindow.nameTablesIDs[parameter.ToString()];
                    int id;
                    if (nameTableIDs.TryGetBySecond((int)value, out id))
                        return id;
                }
            }
            return null;
        }
    }
}
