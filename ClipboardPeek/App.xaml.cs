using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ClipboardPeek
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog logger =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void UHE(Exception e)
        {

            string message = string.Format(
                "大変申し訳けありません。システムエラーが発生しました。\n ({0} {1})\n\n{2}",
                e.GetType(), e.Message, e.StackTrace);
            logger.Error("Unhundled error", e);
            MessageBox.Show(message);
            Environment.Exit(1);
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            UHE(e.Exception);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, ex) => UHE(ex.ExceptionObject as Exception);
        }
    }
}
