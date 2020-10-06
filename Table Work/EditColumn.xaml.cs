
using System;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Windows.Controls;
using StationManager.Data;
using StationManager.Data.TableElements;

namespace StationManager
{
    /// <summary>
    /// Логика взаимодействия для EditColumn.xaml
    /// </summary>
    public partial class EditColumn : UserControl
    {
        private string _headerText;
        public string HeaderText { get => _headerText; set { _headerText = HeaderTextBlock.Text = value; } }
        private string _tableName;
        public string TableName { get => _tableName;
            set {
                _tableName = value;
                LoadContext(value);
            } 
        }

        public EditColumn()
        {
            InitializeComponent();
            
            if (!MainWindow.sqlData.IsConnected)
            {
                this.IsEnabled = false;
            }
        }

        private void LoadContext(string tableName)
        {
            if (ElementDataGrid.DataContext != null && ElementDataGrid.DataContext.GetType().IsAssignableFrom(typeof(NamedObservableCollection<NameRow>)))
                MainWindow.sqlData.UnbindNameList((NamedObservableCollection<NameRow>)ElementDataGrid.DataContext);
            var nameCollection = MainWindow.sqlData.GetNameList(tableName);
            ElementDataGrid.ItemsSource = nameCollection;
        }

    }
}
