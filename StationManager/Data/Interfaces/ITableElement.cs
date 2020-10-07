using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StationManager.Data.Interfaces
{
    public interface ITableElement
    {

        int StationID { get; set; }

        event EventHandler<ValueChangedEventArgs> ValueChanged;

        void SetAllProperties(dynamic properties);

        bool HasIncrementKey();

        PropertyInfo GetIncrementKey();

        Dictionary<string, object> GetPrimalKeys();

        Dictionary<string, object> GetAllProperties();
    }

    public class ValueChangedEventArgs
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public object OldValue { get; set; }
    }
}
