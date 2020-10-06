
using Dapper;
using Microsoft.Data.Sqlite;
using StationManager.Data;
using StationManager.Data.Interfaces;
using StationManager.Data.TableElements;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace StationManager.Data
{

    public class SQLiteData
    {
        public bool IsConnected { get => (connection != null && connection.State == System.Data.ConnectionState.Open); }

        private static Mutex queryMutex = new Mutex();

        public IDbConnection connection;

        private static SqliteCommand[] startupCommands = new SqliteCommand[] {
            new SqliteCommand("create table if not exists [Stations]        (\"id\" INTEGER NOT NULL UNIQUE,\"Name\" TEXT, \"Note\" TEXT, \"Image\" BLOB, PRIMARY KEY(\"id\" AUTOINCREMENT));"),

            new SqliteCommand("create table if not exists [Affiliations]    (\"id\" INTEGER NOT NULL UNIQUE,\"Name\" TEXT, PRIMARY KEY(\"id\" AUTOINCREMENT));"),
            new SqliteCommand("create table if not exists [Types]           (\"id\" INTEGER NOT NULL UNIQUE,\"Name\" TEXT, PRIMARY KEY(\"id\" AUTOINCREMENT));"),
            new SqliteCommand("create table if not exists [Roles]           (\"id\" INTEGER NOT NULL UNIQUE,\"Name\" TEXT, PRIMARY KEY(\"id\" AUTOINCREMENT));"),

            new SqliteCommand("create table if not exists [AffiliationsOfStation]   (\"StationID\" INTEGER NOT NULL, \"ElementID\"  INTEGER NOT NULL, PRIMARY KEY(\"ElementID\", \"StationID\"), FOREIGN KEY(\"ElementID\") REFERENCES \"Affiliations\"(\"id\") ON DELETE CASCADE, FOREIGN KEY(\"StationID\") REFERENCES \"Stations\"(\"id\") ON DELETE CASCADE);"),
            new SqliteCommand("create table if not exists [TypesOfStation]          (\"StationID\" INTEGER NOT NULL, \"ElementID\"  INTEGER NOT NULL, PRIMARY KEY(\"ElementID\", \"StationID\"), FOREIGN KEY(\"ElementID\") REFERENCES \"Types\"(\"id\")        ON DELETE CASCADE, FOREIGN KEY(\"StationID\") REFERENCES \"Stations\"(\"id\") ON DELETE CASCADE);"),
            new SqliteCommand("create table if not exists [RolesOfStation]          (\"StationID\" INTEGER NOT NULL, \"ElementID\"  INTEGER NOT NULL, PRIMARY KEY(\"ElementID\", \"StationID\"), FOREIGN KEY(\"ElementID\") REFERENCES \"Roles\"(\"id\")        ON DELETE CASCADE, FOREIGN KEY(\"StationID\") REFERENCES \"Stations\"(\"id\") ON DELETE CASCADE);"),

            new SqliteCommand("create table if not exists [RangesOfStation]         (\"StationID\" INTEGER NOT NULL, \"ElementID\"  INTEGER NOT NULL, PRIMARY KEY(\"ElementID\", \"StationID\"), FOREIGN KEY(\"StationID\") REFERENCES \"Stations\"(\"id\")     ON DELETE CASCADE);"),

            new SqliteCommand("create table if not exists [CarrierFrequenciesOfStation]         (\"id\" INTEGER NOT NULL UNIQUE, \"StationID\" INTEGER NOT NULL, \"start\" INTEGER NOT NULL, \"end\" INTEGER, FOREIGN KEY(\"StationID\")    REFERENCES \"Stations\"(\"id\")     ON DELETE CASCADE, PRIMARY KEY(\"id\" AUTOINCREMENT));"),
            new SqliteCommand("create table if not exists [ImpulseDurationsOfStation]           (\"id\" INTEGER NOT NULL UNIQUE, \"StationID\" INTEGER NOT NULL, \"start\" REAL NOT NULL,    \"end\" REAL,    FOREIGN KEY(\"StationID\")    REFERENCES \"Stations\"(\"id\")     ON DELETE CASCADE, PRIMARY KEY(\"id\" AUTOINCREMENT));"),
            new SqliteCommand("create table if not exists [ImpulseRepeatFrequenciesOfStation]   (\"id\" INTEGER NOT NULL UNIQUE, \"StationID\" INTEGER NOT NULL, \"start\" INTEGER NOT NULL, \"end\" INTEGER, FOREIGN KEY(\"StationID\")    REFERENCES \"Stations\"(\"id\")     ON DELETE CASCADE, PRIMARY KEY(\"id\" AUTOINCREMENT));"),
            new SqliteCommand("create table if not exists [PeriodsOfStation]                    (\"id\" INTEGER NOT NULL UNIQUE, \"StationID\" INTEGER NOT NULL, \"start\" REAL NOT NULL,    \"end\" REAL,    FOREIGN KEY(\"StationID\")    REFERENCES \"Stations\"(\"id\")     ON DELETE CASCADE, PRIMARY KEY(\"id\" AUTOINCREMENT));")
        };

        private static Dictionary<string, string> queriesStrings = new Dictionary<string, string>();

        public SQLiteData(string filepath)
        {

            if (queriesStrings.Count == 0)
            {
                queriesStrings.Add("Affiliations", "SELECT {0} FROM [AffiliationsOfStation]");
                queriesStrings.Add("Types", "SELECT {0} FROM [TypesOfStation]");
                queriesStrings.Add("Roles", "SELECT {0} FROM [RolesOfStation]");
                queriesStrings.Add("Ranges", "SELECT {0} FROM [RangesOfStation]");
                queriesStrings.Add("CarrierFrequencies", "SELECT {0} FROM [CarrierFrequenciesOfStation]");
                queriesStrings.Add("ImpulseDurations", "SELECT {0} FROM [ImpulseDurationsOfStation]");
                queriesStrings.Add("ImpulseRepeatFrequencies", "SELECT {0} FROM [ImpulseRepeatFrequenciesOfStation]");
                queriesStrings.Add("Periods", "SELECT {0} FROM [PeriodsOfStation]");
                //queriesStrings.Add("Stations",                  "SELECT id, name, note FROM [Stations]");
            }

            using (var connection = new SqliteConnection($"Data Source={@filepath}"))
            {
                try
                {
                    connection.Open();
                    foreach (SqliteCommand command in startupCommands)
                    {
                        command.Connection = (connection);
                        command.ExecuteNonQuery();
                    }
                    this.connection = connection;
                    connection.Open();
                }
                catch (SqliteException e)
                {
                    Console.WriteLine("SQLite open exception: " + e.Message);
                }
            }
        }

        public ObservableCollection<Station> GetAllStations()
        {
            queryMutex.WaitOne();
            var stationDictionary = connection.Query<Station>("SELECT id, name, note FROM [Stations]").ToDictionary(x => x.id);
            foreach (var query in queriesStrings)
            {
                var propertyInfo = typeof(Station).GetProperty(query.Key);
                var result = connection.Query(propertyInfo.PropertyType.GetGenericArguments()[0], string.Format(query.Value, "*"));
                foreach (ITableElement element in result)
                {
                    var propertyCollection = (System.Collections.IList)propertyInfo.GetValue(stationDictionary[element.StationID]);
                    propertyCollection.Add(element);
                }
            }
            queryMutex.ReleaseMutex();

            var collection = new ObservableCollection<Station>(stationDictionary.Values);
            collection.CollectionChanged += OnStationCollectionChanged;
            return collection;
        }

        private static string[] equals = new string[10] { "Name", "Note", "AffiliationsOfStations", "CarrierFrequenciesOfStation", "ImpulseDurationsOfStation", "ImpulseRepeatFrequenciesOfStation", "PeriodsOfStation", "RangesOfStation", "RolesOfStations", "TypesOfStation"};

        public void GetNotEqual(bool[] conditions) {
            if (conditions.Length < 10)
                return;
            
        }

        public void CopyStations(List<int> stations) { 
            
        }

        public ObservableCollection<Station> GetStation(Dictionary<string, IEnumerable<string>> conditions, bool allOccurences = true)
        {
            if (!IsConnected)
                throw new SqliteException("Sqlite database is not connected.", 1);

            string select_string = "SELECT id, name, note FROM [Stations]";
            IEnumerable<string> value;

            bool first = true;
            if (conditions.TryGetValue("Stations", out value)) {
                select_string += " WHERE " + string.Join(allOccurences ? " AND " : " OR " , value);
                conditions.Remove("Stations");
                first = false;
            }
            if (conditions.Count > 0)
            {
                if (first)
                {
                    select_string += " WHERE ";
                }
                else
                {
                    select_string += allOccurences ? " AND " : " OR ";
                    first = true;
                }
                select_string += "id IN (";
                foreach (var condition in conditions)
                {
                    foreach (var val in condition.Value)
                    {
                        if (first)
                            first = false;
                        else
                            select_string += allOccurences ? " INTERSECT " : " UNION ";
                        select_string += val;
                    }
                }
                select_string += ")";
            }

            queryMutex.WaitOne();
            Dictionary<int, Station> stationDictionary = connection.Query<Station>(select_string).ToDictionary(x => x.id);

            var param = new { ids = stationDictionary.Keys.AsList() };

            foreach (var query in queriesStrings)
            {
                var propertyInfo = typeof(Station).GetProperty(query.Key);
                IEnumerable<object> result;
                if (param.ids.Count < 900)
                {
                    result = connection.Query(propertyInfo.PropertyType.GetGenericArguments()[0], string.Format(query.Value, "*") + " WHERE StationID IN @ids", param);
                }
                else
                {
                    result = connection.Query(propertyInfo.PropertyType.GetGenericArguments()[0], string.Format(query.Value, "*"));
                }

                foreach (ITableElement element in result)
                {
                    Station station;
                    if (stationDictionary.TryGetValue(element.StationID, out station))
                    {
                        var propertyCollection = (System.Collections.IList)propertyInfo.GetValue(station);
                        propertyCollection.Add(element);
                    }
                }
            }
            queryMutex.ReleaseMutex();

            var collection = new ObservableCollection<Station>(stationDictionary.Values);
            collection.CollectionChanged += OnStationCollectionChanged;
            return collection;
        }

        private void OnStationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
                DeleteStations(e.OldItems);
        }

        public void BindStations(IEnumerable<Station> stations)
        {
            foreach(var station in stations)
                BindStation(station);
        }

        public void BindStation(Station station)
        {
            station.CollectionItemChanged += OnStationParameterChanging;
            station.CollectionItemRemoved += OnStationParameterDeleted;
            station.CollectionItemAdded += OnStationParameterAdded;
        }

        public void UnbindStations(IEnumerable<Station> stations)
        {
            foreach (var station in stations)
                UnbindStation(station);
        }

        public void UnbindStation(Station station)
        {
            station.CollectionItemChanged -= OnStationParameterChanging;
            station.CollectionItemRemoved -= OnStationParameterDeleted;
            station.CollectionItemAdded -= OnStationParameterAdded;
        }

        public void DeleteStations(System.Collections.IList stations)
        {
            foreach (var station in stations)
                DeleteStation((Station)station);
        }

        public void DeleteStation(Station station)
        {
            Console.WriteLine($"Deleting station with id {station.id}");
            UnbindStation(station);
            connection.Execute("DELETE FROM Stations WHERE id=" + station.id);
        }

        public void AddStation(Station station)
        {
            var propertyList = typeof(Station).GetProperties().AsList();
            var parametersDict = new Dictionary<string, object>();
            var parameterNames = new List<string>();
            Type collectionType = typeof(IEnumerable<ITableElement>);
            foreach (var propertyInfo in propertyList)
            {
                if(propertyInfo.Name == "id")
                {
                    continue;
                }

                var value = propertyInfo.GetValue(station);
                if (!collectionType.IsInstanceOfType(value))
                {
                    parameterNames.Add(propertyInfo.Name);
                    parametersDict.Add("@" + propertyInfo.Name, value);
                }
            }

            queryMutex.WaitOne();

            station.SetID(connection.QueryFirst<int>($"INSERT INTO Stations({string.Join(", ", parameterNames)}) VALUES ({string.Join(", ", parametersDict.Keys)}); SELECT last_insert_rowid();", new DynamicParameters(parametersDict)));
            
            queryMutex.ReleaseMutex();

            foreach (var propertyInfo in propertyList)
            {
                var propertyObject = propertyInfo.GetValue(station);
                if (collectionType.IsInstanceOfType(propertyObject))
                {
                    var elementList = (System.Collections.IList)propertyObject;
                    var count = elementList.Count;
                    for (int i=0; i < count; i++)
                    {
                        if (!SQLAddParameter(elementList[i], propertyInfo.PropertyType.GetProperty("Name").GetValue(elementList).ToString()))
                        {
                            elementList.RemoveAt(i);
                            i--;
                            count--;
                        }
                               
                    }
                }
            }
            BindStation(station);
        }
        
        public void UpdateStationImage(Station station)
        {
            queryMutex.WaitOne();

            station.Image = connection.QueryFirst<byte[]>("SELECT Image from Stations WHERE id=" + station.id);

            queryMutex.ReleaseMutex();
            
        }

        private bool SQLAddParameter(object parameter, string tableName)
        {
            queryMutex.WaitOne();
            try
            {
                var tableElement = parameter as ITableElement;
                var paramDictionary = tableElement.GetAllProperties();
                var sqlParamDictionary = new Dictionary<string, object>();
                foreach(var pair in paramDictionary)
                    sqlParamDictionary.Add("@" + pair.Key, pair.Value);

                connection.Execute($"INSERT INTO {tableName}({string.Join(",", paramDictionary.Keys)}) VALUES ({string.Join(",", sqlParamDictionary.Keys)})", new DynamicParameters(sqlParamDictionary));
                if (tableElement.HasIncrementKey())
                {
                    var property = tableElement.GetIncrementKey();
                    property.SetValue(tableElement, connection.QueryFirst<int>("SELECT last_insert_rowid();"));
                }    
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                queryMutex.ReleaseMutex();
            }
            
        }

        private bool SQLChangeParameter(object parameter, string tableName, string propertyName, object value, object oldValue)
        {
            queryMutex.WaitOne();

            try
            {
                var tableElement = parameter as ITableElement;
                var keysDictionary = tableElement.GetPrimalKeys();
                if (keysDictionary.ContainsKey(propertyName))
                    keysDictionary[propertyName] = oldValue;
                var rowChanged = connection.Execute($"UPDATE {tableName} SET {propertyName}=@value WHERE {string.Join(" AND ", keysDictionary.Keys.Select((key) => $"{key}={keysDictionary[key]}"))}", new { value = value });
                if (rowChanged == 0)
                    if (!SQLAddParameter(parameter, tableName) && !tableElement.HasIncrementKey())
                        return false;

                return true;
            } 
            catch
            {
                return false;
            }
            finally
            {
                queryMutex.ReleaseMutex();
            }
        }

        private bool SQLDeleteParameter(object parameter, string tableName)
        {
            queryMutex.WaitOne();

            try
            {
                var tableElement = parameter as ITableElement;
                var keysDictionary = tableElement.GetPrimalKeys();
                var rowChanged = connection.Execute($"DELETE FROM {tableName} WHERE {string.Join(" AND ", keysDictionary.Keys.Select((key) => $"{key}={keysDictionary[key]}"))}");
                if (rowChanged == 0)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                queryMutex.ReleaseMutex();
            }
        }

        private void OnStationParameterChanging(object sender, CollectionItemChangedEventArgs e)
        {
            var station = (Station)sender;
            if (e.SenderCollection == null)
            {
                queryMutex.WaitOne();
                connection.Execute($"UPDATE Stations SET {e.PropertyName}=@value WHERE id={station.id}", new { value = e.PropertyValue});
                queryMutex.ReleaseMutex();
            }
            else
            {
                string collectionName = (string)e.SenderCollection.GetType().GetProperty("Name").GetValue(e.SenderCollection);
                if (!SQLChangeParameter(e.Item, collectionName, e.PropertyName, e.PropertyValue, e.PropertyOldValue))
                {
                    UnbindStation(station);
                    if (e.PropertyOldValue == null)
                        ((IList)e.SenderCollection).Remove(e.Item);
                    else
                        e.Item.GetType().GetProperty(e.PropertyName).SetValue(e.Item, e.PropertyOldValue);
                    BindStation(station);
                }
            }
        }

        private void OnStationParameterAdded(object sender, CollectionChangedEventArgs eventArgs)
        {
            var item = (dynamic)eventArgs.Item;
            item.StationID = ((Station)sender).id;
            SQLAddParameter(eventArgs.Item, ((dynamic)eventArgs.SenderCollection).Name);
        }

        private void OnStationParameterDeleted(object sender, CollectionChangedEventArgs e)
        {
            SQLDeleteParameter(e.Item, (string)e.SenderCollection.GetType().GetProperty("Name").GetValue(e.SenderCollection));
        }

        public NamedObservableCollection<NameRow> GetNameList(string name)
        {
            if (!IsConnected)
                throw new SqliteException("Sqlite database is not connected.", 1);

            queryMutex.WaitOne();
            var result = connection.Query<NameRow>($"SELECT * FROM {name}");
            queryMutex.ReleaseMutex();

            var observableList = new NamedObservableCollection<NameRow>(name);
            foreach(var tableElement in result)
                observableList.Add(tableElement);

            BindNameList(observableList);

            return observableList;
        }

        public void BindNameList(NamedObservableCollection<NameRow> nameList)
        {
            nameList.CollectionChanged += OnNameCollectionChanged;
            nameList.ItemValueChanged += OnNamePropertyChanged;
        }

        private void OnNamePropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            SQLChangeParameter(e.Item, sender.GetType().GetProperty("Name").GetValue(sender).ToString(), e.PropertyName, e.PropertyValue, e.PropertyOldValue);
        }

        private void OnNameCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {   
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var nameRow = (NameRow)item;
                    SQLAddParameter(nameRow, sender.GetType().GetProperty("Name").GetValue(sender).ToString());
                }
            } 
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach(var item in e.OldItems)
                {
                    var nameRow = (NameRow)item;
                    SQLDeleteParameter(nameRow, sender.GetType().GetProperty("Name").GetValue(sender).ToString());
                }
            }
        }

        public void UnbindNameList(NamedObservableCollection<NameRow> nameList)
        {
            nameList.CollectionChanged  -= OnNameCollectionChanged;
            nameList.ItemValueChanged    -= OnNamePropertyChanged;
        }

        public Dictionary<int, string> GetNameDictionary(string name)
        {
            if (!IsConnected)
                throw new SqliteException("Sqlite database is not connected.", 1);

            queryMutex.WaitOne();
            var result = connection.Query($"SELECT * FROM {name}");
            queryMutex.ReleaseMutex();

            var eventList = new Dictionary<int, string>();
            foreach (var row in result)
                eventList.Add((int)row.id, row.Name);

            return eventList;
        }


        public void Close()
        {
            queryMutex.WaitOne();
            connection.Close();
            queryMutex.ReleaseMutex();
        }
    }
}
