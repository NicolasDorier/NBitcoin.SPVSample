using NBitcoin.SPV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBitcoin.SPVSample
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        private Wallet _Wallet;
        public Wallet Wallet
        {
            get
			{
                return _Wallet;
            }
        }

        public WalletViewModel()
        {

        }
        public WalletViewModel(SPV.Wallet wallet, WalletCreationViewModel creation)
        {
            this._Wallet = wallet;
            Name = creation.Name;
            PrivateKeys = creation.Keys.Where(k => k.PrivateKey != null).Select(k => k.PrivateKey).ToArray();
        }
        public string Name
        {
            get;
            set;
        }

        public string WalletDir
        {
            get
            {
                return Path.Combine(App.AppDir, Name);
            }
        }

        private BitcoinAddress _CurrentAddress;
        public BitcoinAddress CurrentAddress
        {
            get
            {
                return _CurrentAddress;
            }
            set
            {
                if (value != _CurrentAddress)
                {
                    _CurrentAddress = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentAddress"));
                }
            }
        }

        internal void Save()
        {
            if (!Directory.Exists(WalletDir))
                Directory.CreateDirectory(WalletDir);
            using (var fs = File.Open(WalletFile(), FileMode.Create))
            {
                Wallet.Save(fs);
            }
            File.WriteAllText(PrivateKeyFile(), string.Join(",", PrivateKeys.AsEnumerable()));
        }

        private string WalletFile()
        {
            return Path.Combine(WalletDir, "Wallet.dat");
        }

        private string PrivateKeyFile()
        {
            return Path.Combine(WalletDir, "PrivateKeys");
        }

        public BitcoinExtKey[] PrivateKeys
        {
            get;
            set;
        }

        internal static WalletViewModel[] Load(string directory)
        {
            List<WalletViewModel> wallets = new List<WalletViewModel>();
            foreach (var child in new DirectoryInfo(directory).GetDirectories())
            {
                WalletViewModel vm = new WalletViewModel();
                vm.Name = child.Name;

                try
                {
                    vm.PrivateKeys =
                        File.ReadAllText(vm.PrivateKeyFile())
                        .Split(',')
                        .Select(c => new BitcoinExtKey(c, App.Network))
                        .ToArray();
                    using (var fs = File.Open(vm.WalletFile(), FileMode.Open))
                    {
                        vm._Wallet = Wallet.Load(fs);
                    }
                    wallets.Add(vm);
                }
                catch (IOException)
                {
                }
            }
            return wallets.ToArray();
        }

        private List<TransactionViewModel> _Transactions;
        public List<TransactionViewModel> Transactions
        {
            get
            {
                return _Transactions;
            }
            set
            {
                if (value != _Transactions)
                {
                    _Transactions = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Transactions"));
                }
            }
        }
        public void Update()
        {
            if (_Wallet != null)
            {
                CurrentAddress = _Wallet
                                    .GetKnownScripts(true)
                                    .Where(s => s.Value.Indexes[0] == 0) //On public branch
                                    .OrderByDescending(s => s.Value.Indexes[1]) //We generate HD on the path : 0/N, the highest is the latest scriptPubKey
                                    .Select(s => s.Key.GetDestinationAddress(App.Network))
                                    .FirstOrDefault();
                if (_Wallet.State != WalletState.Created)
                    Transactions = _Wallet.GetTransactions().Select(t => new TransactionViewModel(t)).ToList();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class TransactionViewModel
    {
        WalletTransaction _Transaction;
        public TransactionViewModel(WalletTransaction transaction)
        {
            _Transaction = transaction;
        }

        public string TransactionId
        {
            get
            {
                return _Transaction.Transaction.GetHash().ToString();
            }
        }
        public string Balance
        {
            get
            {
                return _Transaction.Balance.ToUnit(MoneyUnit.BTC).ToString();
            }
        }

        public string BlockId
        {
            get
            {
                return _Transaction.BlockInformation == null ? null : _Transaction.BlockInformation.Header.GetHash().ToString();
            }
        }

        public string Confirmations
        {
            get
            {
                return (_Transaction.BlockInformation == null ? 0 : _Transaction.BlockInformation.Confirmations).ToString();
            }
        }
    }
}
