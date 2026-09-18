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

public static class UpdateChecker
{
	private const string RepoOwner = "xbabazibazi";

	private const string RepoName = "AutoVisionNET";

#if NET48
	private const string UpdateAssetName = "update-win7.zip";
#else
	private const string UpdateAssetName = "update.zip";
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

		string versionText = tagName.TrimStart('v', 'V');
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

		string downloadUrl = release["assets"]?
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
