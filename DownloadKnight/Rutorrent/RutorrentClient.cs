using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CookComputing.XmlRpc;

namespace DownloadKnight.Rutorrent
{
    public interface IRtorrentClient
    {
        string GetVersion();
        List<RutorrentTorrent> GetTorrents();

        void AddTorrentFromUrl(string torrentUrl);
        void AddTorrentFromFile(string fileName, byte[] fileContent);
        void RemoveTorrent(string hash);
        void SetTorrentPriority(string hash, RutorrentPriority priority);
        void SetTorrentLabel(string hash, string label);
        void SetTorrentDownloadDirectory(string hash, string directory);
        bool HasHashTorrent(string hash);
        void StartTorrent(string hash);

        string[] GetDownloadList();
    }

    public interface IRutorrent : IXmlRpcProxy
    {
        [XmlRpcMethod("d.multicall2")]
        object[] TorrentMulticall(params string[] parameters);

        [XmlRpcMethod("load.normal")]
        int LoadUrl(string target, string data);

        [XmlRpcMethod("load.raw")]
        int LoadBinary(string target, byte[] data);

        [XmlRpcMethod("d.erase")]
        int Remove(string hash);

        [XmlRpcMethod("d.custom1.set")]
        string SetLabel(string hash, string label);

        [XmlRpcMethod("d.priority.set")]
        int SetPriority(string hash, long priority);

        [XmlRpcMethod("d.directory.set")]
        int SetDirectory(string hash, string directory);

        [XmlRpcMethod("d.name")]
        string GetName(string hash);

        [XmlRpcMethod("system.client_version")]
        string GetVersion();

        [XmlRpcMethod("system.multicall")]
        object[] SystemMulticall(object[] parameters);
        
        [XmlRpcMethod("download_list")]
        string[] GetDownloadList();

        [XmlRpcMethod("view_list")]
        string[] GetViewList();
    }

    public class RutorrentClient : IRtorrentClient
    {
        private RutorrentSettings Settings { get; set; }

        private IRutorrent Client { get; set; }

        public RutorrentClient(RutorrentSettings settings)
        {
            Settings = settings;
            Client = BuildClient();
        }

        public string GetVersion()
        {
            var version = Client.GetVersion();

            return version;
        }

        public List<RutorrentTorrent> GetTorrents()
        {
            var ret = Client.TorrentMulticall("", "",
                "d.name=", // string
                "d.hash=", // string
                "d.base_path=", // string
                "d.custom1=", // string (label)
                "d.size_bytes=", // long
                "d.left_bytes=", // long
                "d.down.rate=", // long (in bytes / s)
                "d.ratio=", // long
                "d.is_open=", // long
                "d.is_active=", // long
                "d.complete=", // long
                "d.timestamp.finished=");

            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            var items = new List<RutorrentTorrent>();
            foreach (object[] torrent in ret)
            {
                var finishedUnixTime = (long)torrent[11];

                var item = new RutorrentTorrent
                {
                    Name = (string)torrent[0],
                    Hash = (string)torrent[1],
                    Path = (string)torrent[2],
                    Label = (string)torrent[3],
                    TotalSize = (long)torrent[4],
                    RemainingSize = (long)torrent[5],
                    DownRate = (long)torrent[6],
                    Ratio = (long)torrent[7],
                    IsOpen = Convert.ToBoolean((long)torrent[8]),
                    IsActive = Convert.ToBoolean((long)torrent[9]),
                    IsFinished = Convert.ToBoolean((long)torrent[10]),
                    Finished = finishedUnixTime > 0L ? (DateTime?)epoch.AddSeconds(finishedUnixTime).ToLocalTime() : null
                };

                items.Add(item);
            }

            return items;
        }

        public void AddTorrentFromUrl(string torrentUrl)
        {
            var response = Client.LoadUrl("", torrentUrl);
            if (response != 0)
                throw new Exception($"Could not add torrent: {torrentUrl}.");
        }

        public void AddTorrentFromFile(string fileName, Byte[] fileContent)
        {
            var response = Client.LoadBinary("", fileContent);
            if (response != 0)
                throw new Exception($"Could not add torrent: {fileName}.");
        }

        public void RemoveTorrent(string hash)
        {
            var response = Client.Remove(hash);
            if (response != 0)
                throw new Exception($"Could not remove torrent: {hash}.");
        }

        public void SetTorrentPriority(string hash, RutorrentPriority priority)
        {
            var response = Client.SetPriority(hash, (long)priority);
            if (response != 0)
                throw new Exception($"Could not set priority on torrent: {hash}.");
        }

        public void SetTorrentLabel(string hash, string label)
        {
            var setLabel = Client.SetLabel(hash, label);
            if (setLabel != label)
                throw new Exception($"Could set label on torrent: {hash}.");
        }

        public void SetTorrentDownloadDirectory(string hash, string directory)
        {
            var response = Client.SetDirectory(hash, directory);
            if (response != 0)
                throw new Exception($"Could not set directory for torrent: {hash}.");
        }

        public bool HasHashTorrent(string hash)
        {
            try
            {
                var name = Client.GetName(hash);
                if (string.IsNullOrWhiteSpace(name)) return false;
                bool metaTorrent = name == (hash + ".meta");
                return !metaTorrent;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void StartTorrent(string hash)
        {
            var multicallResponse = Client.SystemMulticall(new[]
            {
                new { methodName = "d.open", @params = new[] { hash } },
                new { methodName = "d.start", @params = new[] { hash } },
            }
            ).SelectMany(c => ((IEnumerable<int>)c));

            if (multicallResponse.Any(r => r != 0))
                throw new Exception($"Could not start torrent: {hash}.");
        }

        private IRutorrent BuildClient()
        {
            var client = XmlRpcProxyGen.Create<IRutorrent>();

            client.Url = string.Format(@"{0}://{1}:{2}/{3}",
                Settings.UseSsl ? "https" : "http",
                Settings.Host,
                Settings.Port,
                Settings.UrlBase);

            client.EnableCompression = true;

            if (!string.IsNullOrWhiteSpace(Settings.Username))
                client.Credentials = new NetworkCredential(Settings.Username, Settings.Password);

            return client;
        }

        public string[] GetDownloadList()
        {
            return Client.GetDownloadList();
        }

        public string[] GetViewList()
        {
            return Client.GetViewList();
        }
    }
}
