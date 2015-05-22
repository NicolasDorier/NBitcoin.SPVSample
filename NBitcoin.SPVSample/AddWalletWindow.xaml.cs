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

namespace NBitcoin.SPVSample
{
    /// <summary>
    /// Interaction logic for AddWalletWindow.xaml
    /// </summary>
    public partial class AddWalletWindow : Window
    {
        public AddWalletWindow()
        {
            InitializeComponent();
            root.DataContext = new WalletCreationViewModel();
        }

        public WalletCreationViewModel ViewModel
        {
            get
            {
                return root.DataContext as WalletCreationViewModel;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsValid)
                this.DialogResult = true;
        }
    }
}
