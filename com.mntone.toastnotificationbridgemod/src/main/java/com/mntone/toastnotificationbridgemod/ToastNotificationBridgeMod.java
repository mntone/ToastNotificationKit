package com.mntone.toastnotificationbridgemod;

import com.mntone.toastnotificationbridgemod.data.ChatMessage;

import net.minecraft.client.resources.IResource;
import net.minecraft.util.IChatComponent;
import net.minecraft.util.ResourceLocation;
import net.minecraftforge.client.event.ClientChatReceivedEvent;
import net.minecraftforge.common.MinecraftForge;
import net.minecraftforge.fml.common.Mod;
import net.minecraftforge.fml.common.Mod.EventHandler;
import net.minecraftforge.fml.common.event.FMLInitializationEvent;
import net.minecraftforge.fml.common.event.FMLModDisabledEvent;
import net.minecraftforge.fml.common.eventhandler.SubscribeEvent;
import net.minecraftforge.fml.common.gameevent.PlayerEvent;

import org.apache.commons.codec.binary.Base64;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;

@Mod(
	modid = ToastNotificationBridgeMod.ID,
	name = ToastNotificationBridgeMod.NAME,
	version = ToastNotificationBridgeMod.VERSION,
	canBeDeactivated = true)
public final class ToastNotificationBridgeMod
{
	public static final String ID = "mod_ToastNotificationBridge";
	public static final String NAME = "Toast Notification Bridge Mod";
	public static final String VERSION = "0.8";

	private ChatClient _client = null;

	@EventHandler
	public void init(final FMLInitializationEvent event)
	{
		MinecraftForge.EVENT_BUS.register(this);
		this._client = new ChatClient();
	}

	@EventHandler
	public void disable(final FMLModDisabledEvent event)
	{
		this._client.close();
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
			final ChatMessage chatMessage = new ChatMessage("SYSTEM", "/" + messageText);
			this._client.send(chatMessage);

			return;
		}

		final String playerName = messageText.substring(1, messageText.indexOf('>'));
		final String messageBody = messageText.substring(messageText.indexOf('>') + 2);
		final ResourceLocation skinLocation = MinecraftHelper.getPlayerSkin(playerName);

		byte[] skinData = null;
		try
		{
			final IResource skinResource = MinecraftHelper.getResource(skinLocation);
			try (final InputStream stream = skinResource.getInputStream())
			{
				final ByteArrayOutputStream bout = new ByteArrayOutputStream();
				byte[] buffer = new byte[4096];
				while (true)
				{
					int length = stream.read(buffer);
					if (length < 0)
					{
						break;
					}
					bout.write(buffer, 0, length);
				}
				skinData = bout.toByteArray();
			}
		}
		catch (IOException e)
		{
			e.printStackTrace();
		}

		final ChatMessage chatMessage = new ChatMessage(playerName, messageBody, Base64.encodeBase64String(skinData));
		this._client.send(chatMessage);
	}

	@SubscribeEvent
	public void onPlayerLoggedInEvent(final PlayerEvent.PlayerLoggedInEvent event)
	{
		event.player.getName();
	}
}
