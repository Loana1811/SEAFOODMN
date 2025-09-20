using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ASM_GROUP3.View
{
    public partial class OrderManagement : Window
    {
        public OrderManagement()
        {
            InitializeComponent();
        }

        // Chặn ký tự không phải số hoặc dấu chấm
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép số (0-9) và dấu chấm (.)
            Regex regex = new Regex("[^0-9.]");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Không cho nhập phím Space
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
