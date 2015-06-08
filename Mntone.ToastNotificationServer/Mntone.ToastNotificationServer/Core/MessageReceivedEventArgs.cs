using Mntone.ToastNotificationServer.Data;
using Mntone.ToastNotificationServer.Frameworks;
using System;

namespace Mntone.ToastNotificationServer.Core
{
	public sealed class MessageReceivedEventArgs : EventArgs
	{
		public MessageReceivedEventArgs(string jsonText)
		{
			this.Message = JsonSerializerExtensions.Load<ChatMessage>(jsonText);
		}

		public ChatMessage Message { get; set; }
	}
}