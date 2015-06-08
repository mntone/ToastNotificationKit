using System.Runtime.Serialization;

namespace Mntone.ToastNotificationServer.Data
{
	[DataContract]
	public sealed class ChatMessage
	{
		public ChatMessage(string playerName, string text)
		{
			this.PlayerName = playerName;
			this.Text = text;
		}

		[DataMember(Name = "player_name")]
		public string PlayerName { get; private set; }

		[DataMember(Name = "text")]
		public string Text { get; private set; }
	}
}