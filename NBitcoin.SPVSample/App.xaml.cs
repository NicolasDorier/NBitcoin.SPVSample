using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NBitcoin.SPVSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Network Network
        {
            get
            {
                return Network.Main;
            }
        }

        public static string AppDir
        {
            get
            {
                return Directory.GetParent(typeof(App).Assembly.Location).FullName;
            }
        }

        public static object Saving = new object();
        protected override void OnExit(ExitEventArgs e)
        {
            lock (Saving)
            {
                base.OnExit(e);
            }
        }
    }
}
