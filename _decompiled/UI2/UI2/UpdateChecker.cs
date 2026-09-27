using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UI2;

public class UpdateInfo
{
	public Version Version { get; set; }

	public string TagName { get; set; }

	public string DownloadUrl { get; set; }

	public string ReleaseNotes { get; set; }

	public string HtmlUrl { get; set; }
}

// NOTE: UI2 and SnapNetUI are published together under the same GitHub release
// (same repo, same tag), but they are versioned independently (UI2 on a 1.3.x
// track, SnapNetUI on a 1.1.x track). Comparing UI2's own version against the
// shared release TAG is wrong - a SnapNetUI-only release bumps the tag without
// UI2 ever changing, which made UI2 see "update available" forever (the tag
// kept climbing while UI2's embedded version stayed frozen), reinstall the
// same unchanged files every launch, and never catch up. Instead, UI2's real
// version is published as the tiny "evox-console-version.txt" asset, and only THAT is
// compared against CurrentVersion.
public static class UpdateChecker
{
	private const string RepoOwner = "xbabazibazi";

	private const string RepoName = "AutoVisionNET";

	private const string VersionMarkerAssetName = "evox-console-version.txt";

#if NET48
	private const string UpdateAssetName = "evox-console-update-win7.zip";
#else
	private const string UpdateAssetName = "update.zip";
#endif

#if NET48
	// Windows 7's default schannel config doesn't negotiate TLS 1.2 unless the
	// process opts in explicitly, and GitHub's API rejects anything older -
	// without this, HttpClient fails with a generic "İstek gönderilirken bir
	// hata oluştu" that gives no hint it's a TLS version mismatch.
	static UpdateChecker()
	{
		System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
	}
#endif

	public static Version CurrentVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

	public static async Task<UpdateInfo> CheckForUpdateAsync()
	{
		using HttpClient client = new HttpClient();
		client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AutoVisionNET-UpdateChecker", "1.0"));
		client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
		client.Timeout = TimeSpan.FromSeconds(15.0);

		string url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
		HttpResponseMessage response = await client.GetAsync(url);
		if (!response.IsSuccessStatusCode)
		{
			return null;
		}

		string json = await response.Content.ReadAsStringAsync();
		JObject release = JObject.Parse(json);
		string tagName = (string)release["tag_name"];
		if (string.IsNullOrWhiteSpace(tagName))
		{
			return null;
		}

		JToken assets = release["assets"];
		string versionMarkerUrl = assets?
			.FirstOrDefault(a => string.Equals((string)a["name"], VersionMarkerAssetName, StringComparison.OrdinalIgnoreCase))?
			["browser_download_url"]?.ToString();
		if (string.IsNullOrWhiteSpace(versionMarkerUrl))
		{
			return null;
		}

		string versionText = (await client.GetStringAsync(versionMarkerUrl)).Trim();
		if (!Version.TryParse(versionText, out Version parsedVersion))
		{
			return null;
		}

		Version remoteVersion = new Version(
			Math.Max(parsedVersion.Major, 0),
			Math.Max(parsedVersion.Minor, 0),
			Math.Max(parsedVersion.Build, 0),
			Math.Max(parsedVersion.Revision, 0));

		if (remoteVersion <= CurrentVersion)
		{
			return null;
		}

		string downloadUrl = assets?
			.FirstOrDefault(a => string.Equals((string)a["name"], UpdateAssetName, StringComparison.OrdinalIgnoreCase))?
			["browser_download_url"]?.ToString();

		return new UpdateInfo
		{
			Version = remoteVersion,
			TagName = tagName,
			DownloadUrl = downloadUrl,
			ReleaseNotes = (string)release["body"] ?? string.Empty,
			HtmlUrl = (string)release["html_url"]
		};
	}

	public static async Task<string> DownloadUpdateAsync(string downloadUrl)
	{
		using HttpClient client = new HttpClient();
		client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AutoVisionNET-UpdateChecker", "1.0"));
		client.Timeout = TimeSpan.FromMinutes(5.0);

		string tempZip = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"AutoVisionNET_update_{Guid.NewGuid()}.zip");
		using HttpResponseMessage response = await client.GetAsync(downloadUrl);
		response.EnsureSuccessStatusCode();
		using System.IO.FileStream fs = new System.IO.FileStream(tempZip, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
		await response.Content.CopyToAsync(fs);
		return tempZip;
	}
}
