using System.Windows;
using ASM_GROUP3.View;


namespace ASM_GROUP3.View
{
    public partial class AdminHome : Window
    {
        public AdminHome()
        {
            InitializeComponent();
        }


        private void Seafood_Click(object sender, RoutedEventArgs e)
        {
            var seafoodWindow = new MainWindow();
            seafoodWindow.ShowDialog();
        }

        private void BtnDonHang_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderManagement();
            orderWindow.ShowDialog();
        }

        private void ImportOrder_Click(object sender, RoutedEventArgs e)
        {
            var importWindow = new ImportOrderManagement();
            importWindow.ShowDialog();
        }

        private void Supplier_Click(object sender, RoutedEventArgs e)
        {
            var supplierWindow = new SupplierManagement();
            supplierWindow.ShowDialog();
        }

        private void Statistic_Click(object sender, RoutedEventArgs e)
        {
            var statisticWindow = new StatisticsView();
            statisticWindow.ShowDialog();
        }

        private void User_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đang phát triển", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}