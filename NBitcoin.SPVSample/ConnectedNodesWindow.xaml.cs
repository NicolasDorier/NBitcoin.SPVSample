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
    /// Interaction logic for ConnectedNodesWindow.xaml
    /// </summary>
    public partial class ConnectedNodesWindow : Window
    {
        public ConnectedNodesWindow()
        {
            InitializeComponent();
        }

        public ConnectedNodesViewModel ViewModel
        {
            get
            {
                return root.DataContext as ConnectedNodesViewModel;
            }
            set
            {
                root.DataContext = value;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosed(e);
        }
    }
}
