package com.mntone.toastnotificationbridgemod.data;

import com.google.gson.annotations.SerializedName;

public final class ChatMessage
{
	@SerializedName("player_name")
	private String _playerName;

	@SerializedName("text")
	private String _text;

	@SerializedName("skin_data")
	private String _skinData;

	public ChatMessage(final String playerName, final String text)
	{
		this._playerName = playerName;
		this._text = text;
		this._skinData = null;
	}

	public ChatMessage(final String playerName, final String text, final String skinData)
	{
		this._playerName = playerName;
		this._text = text;
		this._skinData = skinData;
	}

	public String getPlayerName()
	{
		return this._playerName;
	}

	public void setPlayerName(final String playerName)
	{
		this._playerName = playerName;
	}

	public String getText()
	{
		return this._text;
	}

	public void setText(final String text)
	{
		this._text = text;
	}

	public String getSkinData()
	{
		return this._skinData;
	}

	public void setSkinData(final String skinData)
	{
		this._skinData = skinData;
	}
}