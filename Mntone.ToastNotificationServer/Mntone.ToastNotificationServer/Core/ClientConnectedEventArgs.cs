using System;

namespace Mntone.ToastNotificationServer.Core
{
	public class ClientConnectedEventArgs : EventArgs
	{
		public ClientConnectedEventArgs(ChatClientContext context)
		{
			this.Context = context;
		}

		public ChatClientContext Context { get; }
	}
}