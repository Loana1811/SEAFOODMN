using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SupplierManagement.xaml
    /// </summary>
    public partial class SupplierManagement : Window
    {
        public SupplierManagement()
        {
            InitializeComponent();
            DataContext = new SupplierViewModel(); // ViewModel của bạn
        }
    }
}
