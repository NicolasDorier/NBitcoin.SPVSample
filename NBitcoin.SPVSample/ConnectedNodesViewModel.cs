using NBitcoin.Protocol;
using NBitcoin.Protocol.Behaviors;
using NBitcoin.SPV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBitcoin.SPVSample
{
    public class ConnectedNodeViewModel : INotifyPropertyChanged
    {
        internal Node _Node;
        public ConnectedNodeViewModel(Node node)
        {
            _Node = node;
            ConnectedAt = _Node.ConnectedAt.LocalDateTime;
        }

        public string Name
        {
            get
            {
                return _Node.RemoteSocketAddress + ":" + _Node.RemoteSocketPort;
            }
        }

        private string _Speed;
        public string Speed
        {
            get
            {
                return _Speed;
            }
            set
            {
                if(value != _Speed)
                {
                    _Speed = value;
                    if(PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Speed"));
                }
            }
        }


        private int _CurrentProgress;
        public int CurrentProgress
        {
            get
            {
                return _CurrentProgress;
            }
            set
            {
                if(value != _CurrentProgress)
                {
                    _CurrentProgress = value;
                    if(PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentProgress"));
                }
            }
        }

        public VersionPayload Version
        {
            get
            {
                return _Node.PeerVersion;
            }
        }

        PerformanceSnapshot _Snap;
        public void UpdateSpeed()
        {
            var snap = _Node.Counter.Snapshot();
            if(_Snap != null)
            {
                Speed = (snap - _Snap).ToString();
            }
            _Snap = snap;
            var behavior = _Node.Behaviors.Find<PingPongBehavior>();
            if(behavior != null)
            {
                Latency = (int)behavior.Latency.TotalMilliseconds;
                behavior.Probe();
            }

            LastSeen = _Node.LastSeen.LocalDateTime;

            var tracker = _Node.Behaviors.Find<TrackerBehavior>();
            var chain = _Node.Behaviors.Find<ChainBehavior>();
            if(tracker.CurrentProgress != null)
                CurrentProgress = chain.Chain.FindFork(tracker.CurrentProgress).Height;
        }

        private DateTime _LastSeen;
        public DateTime LastSeen
        {
            get
            {
                return _LastSeen;
            }
            set
            {
                if(value != _LastSeen)
                {
                    _LastSeen = value;
                    if(PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("LastSeen"));
                }
            }
        }

        private int _Latency;
        public int Latency
        {
            get
            {
                return _Latency;
            }
            set
            {
                if(value != _Latency)
                {
                    _Latency = value;
                    if(PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Latency"));
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public DateTime ConnectedAt
        {
            get;
            set;
        }
    }
    public class ConnectedNodesViewModel : INotifyPropertyChanged
    {
        private Protocol.NodesGroup _Group;

        public ConnectedNodesViewModel(NodesGroup group)
        {
            this._Group = group;
            StartRefresh();
        }


        private async void StartRefresh()
        {
            while(!_Stop)
            {
                List<ConnectedNodeViewModel> included = new List<ConnectedNodeViewModel>();
                foreach(var node in _Group.ConnectedNodes)
                {
                    var vm = Find(node);
                    if(vm == null)
                    {
                        vm = new ConnectedNodeViewModel(node);
                        Nodes.Add(vm);
                    }
                    included.Add(vm);
                }
                foreach(var vm in Nodes.ToList())
                {
                    if(!included.Contains(vm))
                        Nodes.Remove(vm);
                }
                foreach(var vm in Nodes)
                {
                    vm.UpdateSpeed();
                }
                await Task.Delay(1000);
            }
        }



        private readonly ObservableCollection<ConnectedNodeViewModel> _Nodes = new ObservableCollection<ConnectedNodeViewModel>();
        public ObservableCollection<ConnectedNodeViewModel> Nodes
        {
            get
            {
                return _Nodes;
            }
        }

        ConnectedNodeViewModel Find(Node node)
        {
            return _Nodes.FirstOrDefault(n => n._Node == node);
        }

        private ConnectedNodeViewModel _SelectedNode;
        public ConnectedNodeViewModel SelectedNode
        {
            get
            {
                return _SelectedNode;
            }
            set
            {
                if(value != _SelectedNode)
                {
                    _SelectedNode = value;
                    if(PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedNode"));
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        bool _Stop;
        public void Dispose()
        {
            _Stop = true;
        }
    }
}
