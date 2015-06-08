using Mntone.ToastNotificationServer.Data;
using Mntone.ToastNotificationServer.Frameworks;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Mntone.ToastNotificationServer.Core
{
	public static class AppContext
	{
		public const string APP_IDENTIFER = "CHARM_NOTIFICATION";

		private static ToastNotifier _notifier = null;
		private static ChatSocketServer _server = null;
		private static bool _enabled = false;

		static AppContext()
		{
			ShellExtensions.TryCreateShortcut("Mntone.CharmNotification.lnk", APP_IDENTIFER);
			_notifier = ToastNotificationManager.CreateToastNotifier(APP_IDENTIFER);
		}

		public static void Start()
		{
			_enabled = true;

			var server = Interlocked.CompareExchange(ref _server, null, null);
			if (server == null)
			{
				_server = new ChatSocketServer();
				_server.ClientConnected += (sender, e) =>
				{
					var ctx = e.Context;
					ctx.MessageReceived += (sender2, e2) => NotifyToast(e2.Message);
				};
				_server.ServerClosed += (sender, e) =>
				{
					if (_enabled)
					{
						Start();
					}
				};
                _server.Start();
			}
		}

		public static Task Shutdown()
		{
			var task = _server.ShutdownAsync();
			task.ContinueWith(prevTask =>
			{
				_server = null;
				_enabled = false;
			});
			return task;
		}

		private static void NotifyToast(ChatMessage message)
		{
			var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
			var textNodes = toastXml.GetElementsByTagName("text");
			textNodes[0].AppendChild(toastXml.CreateTextNode(string.Format("{0} さんが発言しました", message.PlayerName)));
			textNodes[1].AppendChild(toastXml.CreateTextNode(message.Text));
			_notifier.Show(new ToastNotification(toastXml));
		}
	}
}
