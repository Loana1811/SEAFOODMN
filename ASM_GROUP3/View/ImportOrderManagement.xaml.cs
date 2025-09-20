using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ASM_GROUP3.ViewModel;

namespace ASM_GROUP3.View
{
    /// <summary>
    /// Interaction logic for ImportOrderManagement.xaml
    /// </summary>
    public partial class ImportOrderManagement : Window
    {
        public ImportOrderManagement()
        {
            InitializeComponent();
            DataContext = new ImportOrderViewModel();
        }
        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]"); // Chỉ cho số và dấu chấm
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

    }
}
