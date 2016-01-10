

namespace DownloadKnight.Rutorrent
{
    public class RutorrentSettings
    {
        public RutorrentSettings()
        {
            Host = "localhost";
            Port = 8080;
            UrlBase = "xmlrpc";
            TvCategory = "tv-sonarr";
            OlderTvPriority = (int)RutorrentPriority.Normal;
            RecentTvPriority = (int)RutorrentPriority.Normal;
        }
        
        public string Host { get; set; }

        public int Port { get; set; }

        public string UrlBase { get; set; }

        public bool UseSsl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string TvCategory { get; set; }

        public string TvDirectory { get; set; }

        public int RecentTvPriority { get; set; }

        public int OlderTvPriority { get; set; }
    }
}