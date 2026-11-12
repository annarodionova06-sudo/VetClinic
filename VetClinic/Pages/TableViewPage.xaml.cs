using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class TableViewPage : Page
    {
        private readonly string _tableName;

        public TableViewPage(string tableName, string title)
        {
            InitializeComponent();
            _tableName = tableName;
            tblTitle.Text = title;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var context = new VetClinicEntities()) // замените на имя вашего контекста
                {
                    var query = context.Database.SqlQuery<object>($"SELECT * FROM [{_tableName}]");
                    // DataGrid не умеет показывать dynamic, используем DataTable
                    var dt = new System.Data.DataTable();
                    using (var cmd = context.Database.Connection.CreateCommand())
                    {
                        context.Database.Connection.Open();
                        cmd.CommandText = $"SELECT * FROM [{_tableName}]";
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                    dgData.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}