using StationManager.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StationManager.Data.TableElements
{
    public class EnumRow : ITableElement
    {

        public int StationID 
        {
            get; set; 
        }

        private int _elementID = default;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public int ElementID
        {
            get => _elementID;
            set {
                var oldValue = _elementID;
                _elementID = value;
                OnPropertyChanged(value, oldValue);
            }
        }

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
            return false;
        }

        public PropertyInfo GetIncrementKey()
        {
            return null;
        }

        public Dictionary<string, object> GetPrimalKeys()
        {
            return new Dictionary<string, object> { 
                { "StationID", StationID },
                { "ElementID", ElementID },
            };
        }

        public Dictionary<string, object> GetAllProperties()
        {
            return new Dictionary<string, object> {
                { "StationID", StationID },
                { "ElementID", ElementID },
            };
        }
    }
}
