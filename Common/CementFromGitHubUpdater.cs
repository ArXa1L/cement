using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Common
{
    public class CementFromGitHubUpdater : ICementUpdater
    {
        private readonly ILogger log;
        private string downloadUri;

        public CementFromGitHubUpdater(ILogger log)
        {
            this.log = log;
        }

        public string GetNewCommitHash()
        {
            try
            {
                var webClient = new WebClientWithTimeout();
                webClient.Headers.Add("User-Agent", "Anything");
                var json = webClient.DownloadString("https://api.github.com/repos/skbkontur/cement/releases/latest");
                var release = JsonConvert.DeserializeObject<GitHubRelease>(json);
                if (release.Assets.Count != 1)
                    throw new CementException("Failed to parse json:\n" + json);
                downloadUri = release.Assets[0].BrowserDownloadUrl;

                var parts = downloadUri.Split('/', '.');
                return parts[parts.Length - 2];
            }
            catch (WebException ex)
            {
                log.LogError("Fail self-update ", ex);
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse) ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound) // HTTP 404
                    {
                        ConsoleWriter.WriteError("Failed to look for updates on github. Server responsed 404");
                        return null;
                    }
                }
                ConsoleWriter.WriteError("Failed to look for updates on github: " + ex.Message);
                return null;
            }
        }

        public byte[] GetNewCementZip()
        {
            var client = new WebClientWithTimeout();
            var zipContent = client.DownloadData(downloadUri);
            return zipContent;
        }

        public string GetName()
        {
            return "GitHub";
        }
    }

    public class GitHubRelease
    {
        public List<GitHubAsset> Assets;
    }

    public class GitHubAsset
    {
        [JsonProperty(PropertyName = "browser_download_url")] public string BrowserDownloadUrl;
    }
}