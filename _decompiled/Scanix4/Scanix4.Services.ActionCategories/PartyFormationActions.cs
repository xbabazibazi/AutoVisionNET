using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using InputManager;
using Scanix4.Core;
using Scanix4.Models;
using Scanix4.Services.BaseClasses;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace Scanix4.Services.ActionCategories;

[SupportedOSPlatform("windows10.0.10240.0")]
public class PartyFormationActions(InputUtils inputUtils, Logger logger) : BaseActions(inputUtils, logger)
{
	private const string RequestPartyMenuItemTaskId = "SendPartyInviteMenuItem";

	private const string RequestPartyMenuItemDefaultPath = "Images/RequestPartyMenuItem.jpg";

	private const int MaxCycles = 6;

	private const int CycleDelayMs = 4000;

	private readonly OcrTextScanner _ocrScanner = new OcrTextScanner(logger);

	private readonly ScreenCaptureSettings _settings = Settings.Instance.ScreenCapture;

	public async Task FormPartyAsync(string[] memberNames, CancellationToken cancellationToken = default)
	{
		Rectangle infoArea = _settings.RectanglesSettings.Info.GetRectangle();
		if (infoArea.Width <= 0 || infoArea.Height <= 0)
		{
			Logger.LogWarning("Parti kurma: 'Info' tarama alanı ayarlanmamış. Ayarlar > Info bölgesinden seçin.");
			return;
		}
		Rectangle partyArea = _settings.RectanglesSettings.Party.GetRectangle();
		bool canCheckMembership = partyArea.Width > 0 && partyArea.Height > 0;
		if (!canCheckMembership)
		{
			Logger.LogWarning("Parti kurma: 'Party' tarama alanı ayarlanmamış, kimin partiye girdiği denetlenemeyecek — her üyeye tek seferlik davet gönderilecek.");
		}

		HashSet<string> pending = new HashSet<string>(memberNames, StringComparer.OrdinalIgnoreCase);

		try
		{
			for (int cycle = 0; cycle < MaxCycles && pending.Count > 0; cycle++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (canCheckMembership)
				{
					HashSet<string> currentMembers = await ReadPartyMembersAsync(partyArea);
					foreach (string joined in pending.Where(currentMembers.Contains).ToList())
					{
						pending.Remove(joined);
						Logger.LogInformation($"Parti kurma: '{joined}' partiye katıldı, artık denetlenmiyor.");
					}
					if (pending.Count == 0)
					{
						break;
					}
				}

				foreach (string memberName in pending.ToList())
				{
					cancellationToken.ThrowIfCancellationRequested();
					try
					{
						await InviteOneMemberAsync(infoArea, memberName);
					}
					catch (Exception ex)
					{
						Logger.LogError($"Parti kurma: '{memberName}' davet edilirken hata: {ex.Message}", ex);
					}
					await Task.Delay(500, cancellationToken);
				}

				if (!canCheckMembership)
				{
					break;
				}
				if (pending.Count > 0)
				{
					await Task.Delay(CycleDelayMs, cancellationToken);
				}
			}
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("Parti kurma: kullanıcı tarafından iptal edildi. Denetlenmeyen üyeler: " + string.Join(", ", pending));
			return;
		}

		if (pending.Count > 0)
		{
			Logger.LogWarning("Parti kurma: partiye girmeyen üyeler (zaman aşımı, denetime devam edilmiyor): " + string.Join(", ", pending));
		}
	}

	private async Task<HashSet<string>> ReadPartyMembersAsync(Rectangle partyArea)
	{
		List<(string Text, Rectangle Bounds)> words = await _ocrScanner.ReadTextAsync(partyArea);
		return new HashSet<string>(words.Select(w => w.Text.Trim()).Where(t => t.Length > 0), StringComparer.OrdinalIgnoreCase);
	}

	private async Task InviteOneMemberAsync(Rectangle infoArea, string memberName)
	{
		Point? namePoint = await _ocrScanner.FindTextCenterAsync(infoArea, memberName);
		if (namePoint == null)
		{
			Logger.LogWarning($"Parti kurma: '{memberName}' Info listesinde bulunamadı.");
			return;
		}
		MoveAndRightClick(namePoint.Value);
		Point? menuPoint = FindRequestPartyMenuItem(namePoint.Value);
		if (menuPoint == null)
		{
			Logger.LogWarning($"Parti kurma: '{memberName}' için sağ tık menüsünde 'Request Party' bulunamadı.");
			return;
		}
		MoveAndLeftClick(menuPoint.Value);
		Logger.LogInformation($"Parti kurma: '{memberName}' davet edildi.");
	}

	private Point? FindRequestPartyMenuItem(Point rightClickOrigin)
	{
		string templatePath = TemplateResolver.Resolve(RequestPartyMenuItemTaskId, RequestPartyMenuItemDefaultPath);
		Rectangle searchArea = new Rectangle(Math.Max(0, rightClickOrigin.X - 40), Math.Max(0, rightClickOrigin.Y - 20), 300, 260);
		using ImageSearchService searchService = new ImageSearchService(new SearchConfig
		{
			TemplatePath = templatePath,
			SearchArea = searchArea,
			Threshold = 0.8,
			UseColor = true,
			Mode = MatchMode.SingleMatch
		}, Logger);
		return searchService.Search() as Point?;
	}
}
