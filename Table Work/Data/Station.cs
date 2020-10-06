using StationManager.Data.Interfaces;
using StationManager.Data.TableElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StationManager.Data
{
    public class Station : INotifyPropertyChanged
    {
        public int id { get; set; }

        private string _name = null;
        public string Name {
            get => _name;
            set
            {
                var args = new CollectionItemChangedEventArgs()
                {
                    PropertyName = "Name",
                    PropertyValue = value,
                    Item = null,
                    SenderCollection = null
                };
                CollectionItemChanged?.Invoke(this, args);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                _name = value;
            }
        }

        private string _note = null;
        public string Note {
            get => _note;
            set
            {
                var args = new CollectionItemChangedEventArgs()
                {
                    PropertyName = "Note",
                    PropertyValue = value,
                    Item = null,
                    SenderCollection = null
                };
                CollectionItemChanged?.Invoke(this, args);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Note"));
                _note = value;
            }
        }

        private byte[] _image = null;
        public byte[] Image {
            get => _image; 
            set
            {
                var args = new CollectionItemChangedEventArgs() {
                    PropertyName = "Image", 
                    PropertyValue = value,                                      
                    Item = null, 
                    SenderCollection = null 
                };
                CollectionItemChanged?.Invoke(this, args);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Image"));
                _image = value;
            }
        }

        public NamedObservableCollection<EnumRow> Affiliations { get; set; }
        public NamedObservableCollection<EnumRow> Types { get; set; }
        public NamedObservableCollection<EnumRow> Roles { get; set; }

        public NamedObservableCollection<EnumRow> Ranges { get; set; }

        public NamedObservableCollection<Interval<int>> CarrierFrequencies { get; set; }
        public NamedObservableCollection<Interval<double>> ImpulseDurations { get; set; }
        public NamedObservableCollection<Interval<int>> ImpulseRepeatFrequencies { get; set; }
        public NamedObservableCollection<Interval<double>> Periods { get; set; }

        public event EventHandler<CollectionChangedEventArgs>        CollectionItemAdded;
        public event EventHandler<CollectionChangedEventArgs>        CollectionItemRemoved;
        public event EventHandler<CollectionItemChangedEventArgs> CollectionItemChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public Station()
        {
            Affiliations = new NamedObservableCollection<EnumRow>("AffiliationsOfStation"); 
            Affiliations.CollectionChanged += OnCollectionChanged;
            Affiliations.ItemValueChanged += OnPropertyChanged;

            Types = new NamedObservableCollection<EnumRow>("TypesOfStation");
            Types.CollectionChanged += OnCollectionChanged;
            Types.ItemValueChanged += OnPropertyChanged;

            Roles = new NamedObservableCollection<EnumRow>("RolesOfStation");
            Roles.CollectionChanged += OnCollectionChanged;
            Roles.ItemValueChanged += OnPropertyChanged;

            Ranges = new NamedObservableCollection<EnumRow>("RangesOfStation");
            Ranges.CollectionChanged += OnCollectionChanged;
            Ranges.ItemValueChanged += OnPropertyChanged;

            CarrierFrequencies = new NamedObservableCollection<Interval<int>>("CarrierFrequenciesOfStation");
            CarrierFrequencies.CollectionChanged += OnCollectionChanged;
            CarrierFrequencies.ItemValueChanged += OnPropertyChanged;

            ImpulseDurations = new NamedObservableCollection<Interval<double>>("ImpulseDurationsOfStation");
            ImpulseDurations.CollectionChanged += OnCollectionChanged;
            ImpulseDurations.ItemValueChanged += OnPropertyChanged;

            ImpulseRepeatFrequencies = new NamedObservableCollection<Interval<int>>("ImpulseRepeatFrequenciesOfStation");
            ImpulseRepeatFrequencies.CollectionChanged += OnCollectionChanged;
            ImpulseRepeatFrequencies.ItemValueChanged += OnPropertyChanged;

            Periods = new NamedObservableCollection<Interval<double>>("PeriodsOfStation");
            Periods.CollectionChanged += OnCollectionChanged;
            Periods.ItemValueChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            var newArgs = new CollectionItemChangedEventArgs
            {
                Item = e.Item,
                PropertyName = e.PropertyName,
                PropertyValue = e.PropertyValue,
                PropertyOldValue = e.PropertyOldValue,
                SenderCollection = sender
            };
            CollectionItemChanged?.Invoke(this, newArgs);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(GetPropertyNameByCollectionName(((dynamic)sender).Name)));
        }

        private String GetPropertyNameByCollectionName(string collectionName)
        {
            int ind;
            if ((ind = collectionName.IndexOf("OfStation")) != 0){
                return collectionName.Substring(0, ind);
            }
            return collectionName;
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var newElement = (ITableElement)e.NewItems[0];
                newElement.StationID = id;

                var newArgs = new CollectionChangedEventArgs
                {
                    SenderCollection = sender,
                    Item = e.NewItems[0]
                };
                CollectionItemAdded?.Invoke(this, newArgs);
                
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                var newArgs = new CollectionChangedEventArgs
                {
                    SenderCollection = sender,
                    Item = e.OldItems[0]
                };
                CollectionItemRemoved?.Invoke(this, newArgs);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(GetPropertyNameByCollectionName(((dynamic)sender).Name)));
        }

        public void SetID(int value)
        {
            id = value;
            Affiliations.SetItemsStationID(value);
            Types.SetItemsStationID(value);
            Roles.SetItemsStationID(value);
            Ranges.SetItemsStationID(value);
            CarrierFrequencies.SetItemsStationID(value);
            ImpulseDurations.SetItemsStationID(value);
            ImpulseRepeatFrequencies.SetItemsStationID(value);
            Periods.SetItemsStationID(value);
        }
    }

    public class CollectionChangedEventArgs
    {
        public object Item;
        public object SenderCollection;
    }

    public class CollectionItemChangedEventArgs
    {
        public string PropertyName;
        public object PropertyValue;
        public object PropertyOldValue;

        public object Item;
        public object SenderCollection;
    }

}
