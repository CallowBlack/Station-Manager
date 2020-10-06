using StationManager.Data;
using StationManager.Data.TableElements;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StationManager.Conventers
{
    class EventListToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "";
            foreach (var eventObject in (IEnumerable<object>)value)
            {
                bool newline = true;
                if (eventObject.GetType().IsAssignableFrom(typeof(EnumRow))) {
                    var enumRow = (EnumRow)eventObject;
                    string valueName = "";
                    if (MainWindow.nameTables.ContainsKey(parameter.ToString()))
                    {
                        var nameDict = (Dictionary<int, string>)MainWindow.nameTables[parameter.ToString()];
                        if (!nameDict.TryGetValue(enumRow.ElementID, out valueName))
                            continue;
                        
                    }
                    else if (parameter.ToString() == "Ranges")
                    {
                        var converter = new IDToRange();
                        valueName = (string)converter.Convert(enumRow.ElementID, typeof(string), null, culture);
                        newline = false;
                        if (valueName == null)
                            continue;
                    }
                    result += valueName;
                }
                else
                    result += eventObject.ToString();
                result += ";" + (newline ? "\n" : "");
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    
    }
}
