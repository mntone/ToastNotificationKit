package com.mntone.toastnotificationbridgemod;

import net.minecraft.client.Minecraft;
import net.minecraft.client.network.NetworkPlayerInfo;
import net.minecraft.client.resources.IResource;
import net.minecraft.util.ResourceLocation;

import java.io.IOException;

public final class MinecraftHelper
{
	public static Minecraft getMinecraft()
	{
		return Minecraft.getMinecraft();
	}

	/*public static NetworkPlayerInfo[] getNetworkPlayers()
	{
		final Minecraft mc = getMinecraft();
		final Collection players = mc.getNetHandler().func_175106_d();
		final NetworkPlayerInfo[] playersGeneric = players.stream().map(p -> (NetworkPlayerInfo)p).toArray(NetworkPlayerInfo[]::new);
		return playersGeneric;
	}*/

	public static NetworkPlayerInfo getNetworkPlayer(final String playerID)
	{
		final Minecraft mc = getMinecraft();
		return mc.getNetHandler().func_175104_a(playerID);
	}

	public static ResourceLocation getPlayerSkin(final String playerID)
	{
		final NetworkPlayerInfo player = getNetworkPlayer(playerID);
		return player.getLocationSkin();
	}

	public static IResource getResource(final ResourceLocation location) throws IOException
	{
		return getMinecraft().getResourceManager().getResource(location);
	}
}