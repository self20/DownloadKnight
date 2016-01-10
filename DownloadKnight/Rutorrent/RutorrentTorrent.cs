using System;

namespace DownloadKnight.Rutorrent
{
    public class RutorrentTorrent
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Path { get; set; }
        public string Label { get; set; }
        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public long DownRate { get; set; }
        public long Ratio { get; set; }
        public bool IsFinished { get; set; }
        public bool IsOpen { get; set; }
        public bool IsActive { get; set; }

        public DateTime? Finished { get; set; }

        public TimeSpan TimeRemaining
        {
            get
            {
                if (DownRate > 0)
                {
                    var secondsLeft = RemainingSize / DownRate;
                    return TimeSpan.FromSeconds(secondsLeft);
                }
                else
                    return TimeSpan.Zero;
            }
        }

        public decimal PercentComplete
        {
            get
            {
                if (RemainingSize > 0)
                {
                    return 1 - ((decimal)RemainingSize / TotalSize);
                }
                else
                    return 1m;
            }
        }

        public string Status
        {
            get
            {
                if (IsFinished)
                    return "Completed";
                else if (IsActive)
                    return "Downloading";
                else if (!IsActive)
                    return "Paused";
                return "Unknown";
            }
        }

        public string FriendlyTotalSize
        {
            get { return GetFriendlyFileSize(TotalSize); }
        }


        public static string GetFriendlyFileSize(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }
}
