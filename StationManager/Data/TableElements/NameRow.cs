using StationManager.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StationManager.Data.TableElements
{
    public class NameRow : ITableElement
    {
        public int id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                var oldValue = _name;
                _name = value;
                OnPropertyChanged(value, oldValue);
            }
        }

        public int StationID { get => 0; set { } }

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public void OnPropertyChanged(object value, object oldValue, [CallerMemberName] string propertyName = "")
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs { Name = propertyName, Value = value, OldValue = oldValue });
        }

        public void SetAllProperties(dynamic properties)
        {
            Utils.SetAllProperties(this, properties);
        }

        public bool HasIncrementKey()
        {
            return true;
        }

        public PropertyInfo GetIncrementKey()
        {
            return GetType().GetProperty("id");
        }

        public Dictionary<string, object> GetPrimalKeys()
        {
            return new Dictionary<string, object>() { { "id", id } };
        }

        public Dictionary<string, object> GetAllProperties()
        {
            return new Dictionary<string, object>() { { "Name", Name } };
        }
    }
}
