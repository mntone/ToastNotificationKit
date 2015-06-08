using System;

namespace Mntone.ToastNotificationServer.Core
{
	public sealed class ClosedEventArgs : EventArgs
	{
		public static readonly new ClosedEventArgs Empty = new ClosedEventArgs();
	}
}