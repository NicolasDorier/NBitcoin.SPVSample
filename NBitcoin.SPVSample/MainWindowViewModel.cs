using NBitcoin.Protocol;
using NBitcoin.Protocol.Behaviors;
using NBitcoin.SPV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBitcoin.SPVSample
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public MainWindowViewModel()
        {
            foreach (var wallet in WalletViewModel.Load(App.AppDir))
            {
                Wallets.Add(wallet);
                wallet.Update();
            }
            SelectedWallet = Wallets.FirstOrDefault();

            StartConnecting();
        }

        private async void StartConnecting()
        {
            await Task.Factory.StartNew(() =>
            {
                var parameters = new NodeConnectionParameters();
                parameters.TemplateBehaviors.Add(new AddressManagerBehavior(GetAddressManager())); //So we find nodes faster
                parameters.TemplateBehaviors.Add(new ChainBehavior(GetChain())); //So we don't have to load the chain each time we start
                parameters.TemplateBehaviors.Add(new TrackerBehavior(GetTracker())); //Tracker knows which scriptPubKey and outpoints to track, it monitors all your wallets at the same
                if (!_Disposed)
                {
                    _Group = new NodesGroup(App.Network, parameters, new NodeRequirement()
                    {
                        RequiredServices = NodeServices.Network //Needed for SPV
                    });
                    _Group.MaximumNodeConnection = 4;
                    _Group.Connect();
                    _ConnectionParameters = parameters;
                }
            });

            PeriodicSave();
            PeriodicUiUpdate();
            PeriodicKick();

            foreach(var wallet in Wallets)
            {
                wallet.Wallet.Configure(_Group);
                wallet.Wallet.Connect();
            }
        }


        private async void PeriodicKick()
        {
            while (!_Disposed)
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
                _Group.Purge("For privacy concerns, will renew bloom filters on fresh nodes");
            }
        }

        private async void PeriodicUiUpdate()
        {
            while (!_Disposed)
            {
                await Task.Delay(1000);
                CurrentHeight = GetChain().Height;
                ConnectedNodes = _Group.ConnectedNodes.Count;
                if (SelectedWallet != null)
                    SelectedWallet.Update();
            }
        }

        NodeConnectionParameters _ConnectionParameters;
        private async void PeriodicSave()
        {
            while (!_Disposed)
            {
                await Task.Delay(100000);
                SaveAsync();
            }
        }

        bool _Disposed;
        private void SaveAsync()
        {
            var wallets = Wallets.ToArray();
            var unused = Task.Factory.StartNew(() =>
            {
                lock (App.Saving)
                {
                    GetAddressManager().SavePeerFile(AddrmanFile(), App.Network);
                    using (var fs = File.Open(ChainFile(), FileMode.Create))
                    {
                        GetChain().WriteTo(fs);
                    }
                    using (var fs = File.Open(TrackerFile(), FileMode.Create))
                    {
                        GetTracker().Save(fs);
                    }

                    foreach (var wallet in wallets)
                        wallet.Save();
                }
            });
        }

        private ConcurrentChain GetChain()
        {
            if (_ConnectionParameters != null)
            {
                return _ConnectionParameters.TemplateBehaviors.Find<ChainBehavior>().Chain;
            }
            var chain = new ConcurrentChain(App.Network);
            try
            {
                lock (App.Saving)
                {
                    chain.Load(File.ReadAllBytes(ChainFile()));
                }
            }
            catch
            {
            }
            return chain;
        }

        private Tracker GetTracker()
        {
            if (_ConnectionParameters != null)
            {
                return _ConnectionParameters.TemplateBehaviors.Find<TrackerBehavior>().Tracker;
            }
            try
            {
                lock (App.Saving)
                {
                    using (var fs = File.OpenRead(TrackerFile()))
                    {
                        return Tracker.Load(fs);
                    }
                }
            }
            catch
            {
            }
            return new Tracker();
        }

        private string TrackerFile()
        {
            return Path.Combine(App.AppDir, "tracker.dat");
        }

        private static string ChainFile()
        {
            return Path.Combine(App.AppDir, "chain.dat");
        }
        internal NodesGroup _Group;

        private AddressManager GetAddressManager()
        {
            if (_ConnectionParameters != null)
            {
                return _ConnectionParameters.TemplateBehaviors.Find<AddressManagerBehavior>().AddressManager;
            }
            try
            {
                lock (App.Saving)
                {
                    return AddressManager.LoadPeerFile(AddrmanFile());
                }
            }
            catch
            {
                return new AddressManager();
            }
        }

        private static string AddrmanFile()
        {
            return Path.Combine(App.AppDir, "addrman.dat");
        }

        private WalletViewModel _SelectedWallet;
        public WalletViewModel SelectedWallet
        {
            get
            {
                return _SelectedWallet;
            }
            set
            {
                if (value != _SelectedWallet)
                {
                    _SelectedWallet = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedWallet"));
                }
            }
        }

        private readonly ObservableCollection<WalletViewModel> _Wallets = new ObservableCollection<WalletViewModel>();
        public ObservableCollection<WalletViewModel> Wallets
        {
            get
            {
                return _Wallets;
            }
        }

        internal async void CreateWallet(WalletCreationViewModel walletCreationViewModel)
        {
            WalletCreation creation = walletCreationViewModel.CreateWalletCreation();
            Message = "Creating wallet...";
            Wallet wallet = await CreateWallet(creation);
            Message = "Wallet created";
            var walletVm = new WalletViewModel(wallet, walletCreationViewModel);
            Wallets.Add(walletVm);
            if (SelectedWallet == null)
                SelectedWallet = walletVm;
            walletVm.Save();
            if(_ConnectionParameters != null)
            {
                wallet.Configure(_ConnectionParameters);
                wallet.Connect();
            }
        }

        private string _Message;
        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Message"));
                }
            }
        }

        private Task<Wallet> CreateWallet(WalletCreation creation)
        {
            return Task.Factory.StartNew(() => new Wallet(creation));
        }


        private int _ConnectedNodes;
        public int ConnectedNodes
        {
            get
            {
                return _ConnectedNodes;
            }
            set
            {
                if (value != _ConnectedNodes)
                {
                    _ConnectedNodes = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ConnectedNodes"));
                }
            }
        }

        private int _CurrentHeight;
        public int CurrentHeight
        {
            get
            {
                return _CurrentHeight;
            }
            set
            {
                if (value != _CurrentHeight)
                {
                    _CurrentHeight = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentHeight"));
                }
            }
        }

        internal ConnectedNodesViewModel CreateConnectedNodesViewModel()
        {
            return new ConnectedNodesViewModel(_Group);
        }

        public void Dispose()
        {
            _Disposed = true;
            SaveAsync();
            if (_Group != null)
                _Group.Disconnect();

        }
    }
}
