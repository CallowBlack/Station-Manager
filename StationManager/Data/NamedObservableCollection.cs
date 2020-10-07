using StationManager.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;

namespace StationManager.Data
{
    public class NamedObservableCollection<T> : ObservableCollection<T> where T : ITableElement
    {

        public string Name { get; private set; }

        public event EventHandler<ItemPropertyChangedEventArgs> ItemValueChanged;

        public void SetItemsStationID(int id)
        {
            foreach(ITableElement element in this)
                element.StationID = id;
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
    
                    var notify = (ITableElement)item;
                    notify.ValueChanged += OnValueChanged;
                }
            }
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            ItemValueChanged?.Invoke(this, new ItemPropertyChangedEventArgs(e.Name, e.Value, e.OldValue, sender));
        }

        public NamedObservableCollection(string name) : base()
        {
            Name = name;
            base.CollectionChanged += OnCollectionChanged;
        }

        public NamedObservableCollection(string name, IEnumerable<T> collection) : base(collection)
        {
            Name = name;
        }

    }

    public class ItemPropertyChangedEventArgs
    {
        public string PropertyName { get; private set; }
        public object Item { get; private set; }
        public object PropertyValue { get; private set; }
        public object PropertyOldValue { get; private set; }

        public ItemPropertyChangedEventArgs(string propertyName, object propertyValue, object propertyOldValue, object item){
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            PropertyOldValue = propertyOldValue;
            Item = item;
        }

    }
}
