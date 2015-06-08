using Mntone.ToastNotificationServer.Core;
using System.Windows;

namespace Mntone.ToastNotificationServer
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			AppContext.Start();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			AppContext.Shutdown();
		}
	}
}
