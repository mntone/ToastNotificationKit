using System.Runtime.Serialization;

namespace Mntone.ToastNotificationServer.Data
{
	[DataContract]
	public sealed class ChatMessage
	{
		public ChatMessage(string playerName, string text, string skinData)
		{
			this.PlayerName = playerName;
			this.Text = text;
			this.SkinData = skinData;
		}

		[DataMember(Name = "player_name", IsRequired = true)]
		public string PlayerName { get; private set; }

		[DataMember(Name = "text", IsRequired = true)]
		public string Text { get; private set; }

		[DataMember(Name = "skin_data")]
		public string SkinData { get; private set; }
	}
}