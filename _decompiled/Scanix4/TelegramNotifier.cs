using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SettingsManager;
using SimpleLogger;

namespace Scanix4;

/// <summary>
/// Best-effort remote notifications so the operator finds out about something (inventory full,
/// disconnected from the service, ...) without having to be watching the PC. A notification
/// failure (bad token, no internet, Telegram down) must never take down the caller - every path
/// here swallows its own exceptions and just logs them.
/// </summary>
public static class TelegramNotifier
{
	private static readonly HttpClient Http = new HttpClient
	{
		Timeout = TimeSpan.FromSeconds(10.0)
	};

	public static async Task SendAsync(string message)
	{
		try
		{
			SettingsManager.GeneralSettingss.Telegram settings = Settings.Instance.GeneralSettings.Telegram;
			if (!settings.IsActive || string.IsNullOrWhiteSpace(settings.BotToken) || string.IsNullOrWhiteSpace(settings.ChatId))
			{
				return;
			}
			string url = $"https://api.telegram.org/bot{settings.BotToken}/sendMessage";
			string nickname = SnapNetClient.AppClient.Nickname;
			string prefixedMessage = string.IsNullOrEmpty(nickname) ? message : $"[{nickname}] {message}";
			using FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "chat_id", settings.ChatId },
				{ "text", prefixedMessage }
			});
			using HttpResponseMessage response = await Http.PostAsync(url, content);
			if (!response.IsSuccessStatusCode)
			{
				string body = await response.Content.ReadAsStringAsync();
				Logger.Instance.LogWarning($"Telegram bildirimi gönderilemedi ({(int)response.StatusCode}): {body}");
			}
		}
		catch (Exception ex)
		{
			Logger.Instance.LogWarning("Telegram bildirimi gönderilirken hata: " + ex.Message);
		}
	}
}
