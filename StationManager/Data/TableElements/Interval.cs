using StationManager.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StationManager.Data.TableElements
{
    public class Interval<T> : ITableElement
    {
        public int id { get; set; }
        public int StationID { get; set; }

        private T _start = default;
        public T start {
            get => _start;
            set
            {
                var oldValue = _start;
                _start = value;
                OnPropertyChanged(value, oldValue);
            }
        }

        private T _end = default;
        public T end { 
            get => _end;
            set {
                var oldValue = _end;
                _end = value;
                OnPropertyChanged(value.Equals(default(T)) ? null : (object)value, oldValue.Equals(default(T)) ? null : (object)oldValue);
            }
        }

        public bool IsEnd
        {
            get => !end.Equals(default);
        }

        public string NumberInterval
        {
            get => this.ToString();
            set
            {
                var stringNumbers = value.ToString().Split('-');
                decimal startTemp, endTemp;
                dynamic dnStart = default(T);
                dynamic dnEnd = default(T);
                if (stringNumbers.Length > 0)
                {
                    if (decimal.TryParse(stringNumbers[0], System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out startTemp))
                    {

                        if (typeof(T).IsAssignableFrom(typeof(int)))
                        {
                            dnStart = (dynamic)(int)startTemp;
                        }
                        else if (typeof(T).IsAssignableFrom(typeof(double)))
                        {
                            dnStart = (dynamic)(double)startTemp;
                        }
                        if (stringNumbers.Length == 2)
                        {
                            if (decimal.TryParse(stringNumbers[1], System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out endTemp))
                            {
                                if (typeof(T).IsAssignableFrom(typeof(int)))
                                {
                                    dnEnd = (dynamic)(int)endTemp;
                                }
                                else if (typeof(T).IsAssignableFrom(typeof(double)))
                                {
                                    dnEnd = (dynamic)(double)endTemp;
                                }
                            }
                        }
                    }
                }
                start = dnStart;
                end = dnEnd;
            }
        }

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public void OnPropertyChanged(object value, object oldValue, [CallerMemberName] string propertyName = "")
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs { Name = propertyName, Value = value, OldValue = oldValue });
        }


        public override string ToString()
        {
            
            if (end.Equals(default(T)))
                return start.ToString().Replace(',','.');

            return (start.ToString() + "-" + end.ToString()).Replace(',', '.'); 
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
            return new Dictionary<string, object>() { { "StationID", StationID }, { "start", start }, { "end", !end.Equals(default) ? (object)end : null } };
        }
    }
}
