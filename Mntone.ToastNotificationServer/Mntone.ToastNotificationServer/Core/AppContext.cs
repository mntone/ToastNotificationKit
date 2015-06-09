using Mntone.ToastNotificationServer.Data;
using Mntone.ToastNotificationServer.Frameworks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Mntone.ToastNotificationServer.Core
{
	public static class AppContext
	{
		public const string APP_IDENTIFER = "CHARM_NOTIFICATION";

		private static ToastNotifier _notifier = null;
		private static ChatServer _server = null;
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
				_server = new ChatServer();
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
			if (!string.IsNullOrEmpty(message.SkinData))
			{
				var targetSize = (int)(1.8F /* TODO: guess */ * 90);
				var filedir = Path.Combine(Path.GetTempPath(), "Mntone.ToastNotificationServer");
				var filepath = Path.Combine(filedir, string.Format("{0}_{1}.png", message.PlayerName, targetSize));
				if (!Directory.Exists(filedir))
				{
					Directory.CreateDirectory(filedir);
				}
				if (!File.Exists(filepath))
				{
					var data = Convert.FromBase64String(message.SkinData);
					using (var ms = new MemoryStream(data))
					{
						var decoder = PngBitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
						var bitmap = ImageHelper.CropAndResize(decoder.Frames[0], new Rect(8, 8, 8, 8), new Size(targetSize, targetSize));
						var encoder = new PngBitmapEncoder();
						encoder.Frames.Add(BitmapFrame.Create(bitmap));

						using (var stream = File.Create(filepath))
						{
							encoder.Save(stream);
						}
					}
				}

				var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);
				var textNodes = toastXml.GetElementsByTagName("text");
				textNodes[0].AppendChild(toastXml.CreateTextNode(string.Format("{0} さんが発言しました", message.PlayerName)));
				textNodes[1].AppendChild(toastXml.CreateTextNode(message.Text));
				var toastImage = (XmlElement)toastXml.GetElementsByTagName("image")[0];
				toastImage.SetAttribute("src", string.Format("file:///{0}", filepath));
				toastImage.SetAttribute("alt", "Skin Icon");
				_notifier.Show(new ToastNotification(toastXml));
			}
			else
			{
				var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
				var textNodes = toastXml.GetElementsByTagName("text");
				textNodes[0].AppendChild(toastXml.CreateTextNode(string.Format("{0} さんが発言しました", message.PlayerName)));
				textNodes[1].AppendChild(toastXml.CreateTextNode(message.Text));
				_notifier.Show(new ToastNotification(toastXml));
			}
		}
	}
}
