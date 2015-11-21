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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NBitcoin.SPVSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            root.DataContext = new MainWindowViewModel();
        }
        
        public MainWindowViewModel ViewModel
        {
            get
            {
                return root.DataContext as MainWindowViewModel;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddWalletWindow win = new AddWalletWindow();
            win.ViewModel.Name = "Wallet" + ViewModel.Wallets.Count;
            var result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ViewModel.CreateWallet(win.ViewModel);
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel._Group != null)
            {
                ConnectedNodesWindow win = new ConnectedNodesWindow();
                win.ViewModel = ViewModel.CreateConnectedNodesViewModel();
                win.Show();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ViewModel.Dispose();
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedWallet != null && ViewModel.SelectedWallet.CurrentAddress != null)
            {
                Clipboard.SetText(ViewModel.SelectedWallet.CurrentAddress.ToString());
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedWallet != null)
            {
                ViewModel.SelectedWallet.Wallet.GetNextScriptPubKey();
                ViewModel.SelectedWallet.Update();
            }
        }
    }
}
