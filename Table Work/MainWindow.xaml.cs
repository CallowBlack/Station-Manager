using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StationManager.Properties;
using StationManager.Data;
using System.Diagnostics;
using StationManager.Components;
using Dapper;
using StationManager.DataStructures;
using StationManager.Conventers;
using System.Net;
using System.Collections.ObjectModel;

namespace StationManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Hashtables help to faster search

        public static Hashtable nameTablesIDs = new Hashtable();

        public static Hashtable nameTables = new Hashtable();

        // Sqlite manager

        public static SQLiteData sqlData = null;

        // Delegates

        public delegate void updateConnectionStatusCallback(bool isConnected);

        private delegate void FillGridCallback(DataTable dt);

        // Search threads variables

        private static Mutex searchMutex = new Mutex();

        private static bool AllOccurencesQueue = true;

        private static Dictionary<string, IEnumerable<string>> conditionsQueue = null;

        private static Thread searchThread;

        private string PathToDataBase;

        public bool Additional = false;

        //

        private bool IsConnected
        {
            get => mainGrid.IsEnabled;
            set
            {
                if (value)
                {
                    if (IsConnected)
                        IsConnected = false;

                    ReconnectPanel.Visibility = Visibility.Collapsed;
                    ConnectionStatus.Text = "Переподключение";
                    ConnectionStatus.Foreground = new SolidColorBrush(Colors.Orange);


                    Thread th = new Thread(() =>
                    {
                        sqlData = new SQLiteData(PathToDataBase);
                        sqlData.connection.Open();
                       
                        Dispatcher.Invoke(new updateConnectionStatusCallback(this.UpdateConnectionStatus), new object[] { sqlData.IsConnected });

                    });
                    th.Start();
                }
                else
                {
                    UpdateConnectionStatus(false);
                }
            }
        }

        public MainWindow(bool additional, string path) {
            this.Additional = additional;
            this.PathToDataBase = path;
            
            InitializeComponent();

            if (nameTablesIDs.Count == 0)
            {
                nameTablesIDs.Add("Affiliations", new BiDictionary<int, int>());
                nameTablesIDs.Add("Roles", new BiDictionary<int, int>());
                nameTablesIDs.Add("Types", new BiDictionary<int, int>());
            }

            if (nameTables.Count == 0)
            {
                nameTables.Add("Affiliations", null);
                nameTables.Add("Roles", null);
                nameTables.Add("Types", null);
            }

            IsConnected = true;

            searchThread = new Thread(SearchThreadFunc);
            searchThread.Start();

            Closing += (s, e) => searchThread.Abort();
        }

        public MainWindow() : this(false, "stations.db") { }

        private void UpdateConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                mainGrid.IsEnabled = true;

                ConnectionStatus.Text = "Подключено";
                ConnectionStatus.Foreground = new SolidColorBrush(Colors.DarkGreen);

                UpdateAllNameTables();
                UpdateMainDataGrid();
            }
            else
            {
                mainGrid.IsEnabled = false;
                ReconnectPanel.Visibility = Visibility.Visible;
                ConnectionStatus.Text = "Отключено";
                ConnectionStatus.Foreground = new SolidColorBrush(Colors.DarkRed);

                if (sqlData != null && sqlData.IsConnected)
                {
                    sqlData.Close();
                }
            }
        }

        public void UpdateAllNameTables()
        {
            foreach (var name in nameTablesIDs.Keys)
            {
                var nameTable = sqlData.GetNameDictionary(name.ToString());
                nameTables[name] = nameTable;

                var nameTableIDs = (BiDictionary<int, int>)nameTablesIDs[name];
                if (nameTableIDs.Count > 0)
                    nameTableIDs.Clear();

                var idList = nameTable.Keys.AsList();
                var nameList = nameTable.Values.AsList();
                for (int i = 0; i < nameTable.Count; i++)
                    nameTableIDs.Add(idList[i], i + 1);

                nameList.Insert(0, "Пусто");

                Binding bind = new Binding();
                bind.Source = nameList;
                bind.Mode = BindingMode.OneWay;

                var nameComboBox = (ComboBox)FindName(name.ToString());
                nameComboBox.SetBinding(ComboBox.ItemsSourceProperty, bind);
                nameComboBox.SelectedIndex = 0;
            }
        }

        private void UpdateMainDataGrid()
        {
            searchMutex.WaitOne();

            BindingOperations.ClearBinding(DBGrid, DataGrid.ItemsSourceProperty);

            conditionsQueue = GetConditions();
            AllOccurencesQueue = ConditionOperator.SelectedIndex == 0;

            searchMutex.ReleaseMutex();
        }

        private void SearchThreadFunc()
        {
            while (true)
            {
                Thread.Sleep(20);

                searchMutex.WaitOne();

                if (conditionsQueue != null && sqlData != null)
                {
                    var conditions = conditionsQueue;
                    conditionsQueue = null;
                    searchMutex.ReleaseMutex();
                    ObservableCollection<Station> result;

                    if (conditions.Count == 0)
                        result = sqlData.GetAllStations();
                    else
                        result = sqlData.GetStation(conditions, AllOccurencesQueue);

                    searchMutex.WaitOne();
                    if (conditionsQueue == null)
                        Dispatcher.BeginInvoke(new UpdateMainGridDelegate(UpdateMainGridBinding), new object[] { result });
                    else
                        Console.WriteLine("Запрос отменён!");
                    searchMutex.ReleaseMutex();
                }
                else
                    searchMutex.ReleaseMutex();
            }
        }

        private delegate void UpdateMainGridDelegate(ObservableCollection<Station> stations); 

        private void UpdateMainGridBinding(ObservableCollection<Station> stations)
        {
            Binding b = new Binding();
            if (!Additional)
                sqlData.BindStations(stations);
            b.Source = stations;
            b.Mode = BindingMode.OneWay;
            b.IsAsync = true;

            DBGrid.SetBinding(DataGrid.ItemsSourceProperty, b);
        }


        private Dictionary<string, IEnumerable<string>> GetConditions()
        {
            Dictionary<string, IEnumerable<string>> conditions = new Dictionary<string, IEnumerable<string>>();
            var stationConditions = new List<string>();
            if (NameSearchActive.IsChecked.Value)
                stationConditions.Add(TextQuery("Name", NameField, NameField_Equal, NameField_Register));

            if (NoteSearchActive.IsChecked.Value)
                stationConditions.Add(TextQuery("Note", NoteField, NoteField_Equal, NoteField_Register));

            if (stationConditions.Count > 0)
                conditions.Add("Stations", stationConditions);

            if (AffiliationSearchActive.IsChecked.Value && Affiliations.SelectedIndex > 0)
                conditions.Add("Affiliations", new string[] { ComboQuery("AffiliationsOfStation", "AffiliationsOfStation.ElementID", Affiliations) });

            if (TypeSearchActive.IsChecked.Value && Types.SelectedIndex > 0)
                conditions.Add("Types", new string[] { ComboQuery("TypesOfStation", "TypesOfStation.ElementID", Types) });

            if (RoleSearchActive.IsChecked.Value && Roles.SelectedIndex > 0)
                conditions.Add("Roles", new string[] { ComboQuery("RolesOfStation", "RolesOfStation.ElementID", Roles) });

            if (RangesSearchActive.IsChecked.Value && frequencyRanges.SelectedIndex > 0)
                conditions.Add("Ranges", new string[] { $"SELECT StationID FROM RangesOfStation WHERE RangesOfStation.ElementID={frequencyRanges.SelectedIndex} GROUP BY StationID" });

            var carrierQuery = CarrierField.GetSQLQuery("CarrierFrequenciesOfStation");
            if (CarrierSearchActive.IsChecked.Value && !string.IsNullOrEmpty(carrierQuery))
                conditions.Add("CarrierFrequencies", new string[] { carrierQuery });

            var impulseQuery = FrequencyImpulseField.GetSQLQuery("ImpulseRepeatFrequenciesOfStation");
            if (FrequencySearchActive.IsChecked.Value && !string.IsNullOrEmpty(impulseQuery))
                conditions.Add("ImpulseRepeatFrequencies", new string[] { impulseQuery });

            var durationQuery = DurationField.GetSQLQuery("ImpulseDurationsOfStation");
            if (DurationSearchActive.IsChecked.Value && !string.IsNullOrEmpty(durationQuery))
                conditions.Add("ImpulseDurations", new string[] { durationQuery });

            var periodQuery = PeriodField.GetSQLQuery("PeriodsOfStation");
            if (PeriodSearchActive.IsChecked.Value && !string.IsNullOrEmpty(periodQuery))
                conditions.Add("Periods", new string[] { periodQuery });

            return conditions;
        }

        private string TextQuery(string name, HintedTextBox textBox, CheckBox equal, CheckBox caseSensitive)
        {
            string result;
            string text = textBox.Text.Replace("'", "''");
            if (equal.IsChecked.Value)
            {
                if (caseSensitive.IsChecked.Value)
                    result = $"{name}='{text}'";
                else
                    result = $"UPPER({name})=UPPER('{text}')";
            }
            else
            {
                if (caseSensitive.IsChecked.Value)
                    result = $"{name} LIKE '%{text}%'";
                else
                    result = $"{name} LIKE '%{text}%' COLLATE nocase";
            }
            return result;
        }

        private string ComboQuery(string tablename, string name, ComboBox comboBox)
        {
            var idDict = (BiDictionary<int, int>)nameTablesIDs[comboBox.Name];
            int id;
            if (idDict.TryGetBySecond(comboBox.SelectedIndex, out id))
                return $"SELECT StationID FROM {tablename} WHERE {name}={id} GROUP BY StationID";
            return null;
        }

        // Callbacks

        private void OnReconnectClick(object sender, RoutedEventArgs e)
        {
            IsConnected = true;
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            UpdateMainDataGrid();
        }

        private void OnSearchChanged(object sender, EventArgs e)
        {
            if (OnlineMode != null && OnlineMode.IsChecked.Value)
            {
                UpdateMainDataGrid();
            }
        }

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            DataGridWidth.Width = new GridLength(0, GridUnitType.Star);
            EditingWidth.Width = new GridLength(1, GridUnitType.Star);
            EditingPanel.DataContext = new Station();
            AcceptAddButton.Visibility = Visibility.Visible;
            RowImage.Source = null;
            saveFileButton.Visibility = Visibility.Collapsed;
            SearchButton.IsEnabled = false;
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            int countRowSelected = DBGrid.SelectedCells.Count / DBGrid.Columns.Count;
            if (countRowSelected == 1)
            {
                DataGridWidth.Width = new GridLength(0, GridUnitType.Star);
                EditingWidth.Width = new GridLength(1, GridUnitType.Star);
                var station = (Station)DBGrid.SelectedItem;
                EditingPanel.DataContext = station;
                sqlData.UpdateStationImage(station);
                RowImage.Source = station.Image != null ? Utils.LoadImage(station.Image) : null;
                AcceptAddButton.Visibility = Visibility.Collapsed;
                SearchButton.IsEnabled = false;
            }

        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            CloseEditRowPage();
        }

        private void CloseEditRowPage()
        {
            //EditingPanel.DataContext = null;
            DataGridWidth.Width = new GridLength(1, GridUnitType.Star);
            EditingWidth.Width = new GridLength(0, GridUnitType.Star);
            RowImage.Source = null;
            SearchButton.IsEnabled = true;
        }

        private void OnAcceptAddClick(object sender, RoutedEventArgs e)
        {
            sqlData.AddStation((Station)EditingPanel.DataContext);
            UpdateMainDataGrid();
            CloseEditRowPage();
        }

        private void OnEditColumnsClick(object sender, RoutedEventArgs e)
        {
            var selfpf = new MainWindow();
            selfpf.ShowDialog();
            var columnsWindow = new EditColumnWindow();
            columnsWindow.ShowDialog();
            UpdateAllNameTables();
            UpdateMainDataGrid();
        }

        private void OnChooseFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (openFileDialog.ShowDialog() == true)
            {
                var station = (Station)EditingPanel.DataContext;
                station.Image = File.ReadAllBytes(openFileDialog.FileName);
                saveFileButton.Visibility = Visibility.Visible;
                RowImage.Source = Utils.LoadImage(station.Image);
            }
        }

        private void OnSaveFileClick(object sender, RoutedEventArgs e)
        {
            if (RowImage.Source != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = ".png";
                saveFileDialog.FileName = "image.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, ((Station)EditingPanel.DataContext).Image);
                }
            }

        }

        private void OnImageDeleteClick(object sender, RoutedEventArgs e)
        {
            ((Station)EditingPanel.DataContext).Image = null;
            saveFileButton.Visibility = Visibility.Collapsed;
        }

        
    }

}
