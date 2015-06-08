package com.mntone.toastnotificationbridgemod;

import net.minecraft.util.IChatComponent;
import net.minecraftforge.client.event.ClientChatReceivedEvent;
import net.minecraftforge.common.MinecraftForge;
import net.minecraftforge.fml.common.Mod;
import net.minecraftforge.fml.common.Mod.EventHandler;
import net.minecraftforge.fml.common.event.FMLInitializationEvent;
import net.minecraftforge.fml.common.event.FMLModDisabledEvent;
import net.minecraftforge.fml.common.eventhandler.SubscribeEvent;

@Mod(
	modid = ToastNotificationBridgeMod.ID,
	name = ToastNotificationBridgeMod.NAME,
	version = ToastNotificationBridgeMod.VERSION,
	canBeDeactivated = true)
public class ToastNotificationBridgeMod
{
	public static final String ID = "mod_ToastNotificationBridge";
	public static final String NAME = "Toast Notification Bridge Mod";
	public static final String VERSION = "0.8";

	private ChatSocketClient _client = null;

	@EventHandler
	public void init(final FMLInitializationEvent event)
	{
		MinecraftForge.EVENT_BUS.register(new ForgeListener(this));
		this._client = new ChatSocketClient();
	}

	@EventHandler
	public void disable(final FMLModDisabledEvent event)
	{
		this._client.close();
	}

	public static class ForgeListener
	{
		private ToastNotificationBridgeMod _that;

		public ForgeListener(ToastNotificationBridgeMod that)
		{
			this._that = that;
		}

		@SubscribeEvent
		public void onClientChatReceivedEvent(final ClientChatReceivedEvent event)
		{
			final IChatComponent message = event.message;
			if (message == null)
			{
				return;
			}

			final String messageText = event.message.getUnformattedText();
			if (!messageText.contains(">"))
			{
				return;
			}

			final String playerName = messageText.substring(1, messageText.indexOf('>'));
			final String messageBody = messageText.substring(messageText.indexOf('>') + 2);
			final String jsonText = "{\"player_name\":\"" + playerName + "\",\"text\":\"" + messageBody + "\"}";
			this._that._client.send(jsonText);
		}
	}
}
