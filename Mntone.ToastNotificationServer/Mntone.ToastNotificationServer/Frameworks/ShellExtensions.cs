using System;
using System.Diagnostics;
using System.IO;

namespace Mntone.ToastNotificationServer.Frameworks
{
	public static class ShellExtensions
	{
		public static bool TryCreateShortcut(string filename, string appID)
		{
			var exePath = Process.GetCurrentProcess().MainModule.FileName;
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", filename);

			if (!File.Exists(path))
			{
				using (var shortcut = new ShellLink())
				{
					shortcut.TargetPath = exePath;
					shortcut.Arguments = string.Empty;
					shortcut.AppUserModelID = appID;
					shortcut.Save(path);
				}
				return true;
			}
			return true;
		}
	}
}