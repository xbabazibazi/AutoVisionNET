using System;
using EVOX.Data;
using SettingsManager.ScreenCapture;

namespace SettingsManager.GeneralSettingss;

/// <summary>
/// Telegram bot credentials for remote notifications (inventory full, disconnected from the
/// service, etc.) - the operator doesn't have to be watching the PC to find out something needs
/// attention. Create a bot via @BotFather to get a token, then message it once and read the chat
/// id back from https://api.telegram.org/bot{token}/getUpdates.
/// </summary>
public class Telegram : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[3] { "IsActive", "BotToken", "ChatId" };

	public bool IsActive
	{
		get
		{
			return GetSetting<bool>("IsActive");
		}
		set
		{
			SetSetting("IsActive", value);
		}
	}

	public string BotToken
	{
		get
		{
			return GetSetting<string>("BotToken");
		}
		set
		{
			SetSetting("BotToken", value);
		}
	}

	public string ChatId
	{
		get
		{
			return GetSetting<string>("ChatId");
		}
		set
		{
			SetSetting("ChatId", value);
		}
	}

	public Telegram(DbManager dbManager)
		: base(dbManager, "Telegram")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		return key switch
		{
			"IsActive" => false,
			"BotToken" => string.Empty,
			"ChatId" => string.Empty,
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key),
		};
	}
}
