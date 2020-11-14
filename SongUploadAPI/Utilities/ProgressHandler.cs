using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using ShellProgressBar;

namespace SongUploadAPI.Utilities
{
    public class UploadProgressHandlerBuilder
    {
        private readonly long _fileSize;
        private readonly Progress<long> _progress;
        private double _progressPercentage;
        private readonly int _progressBarWidth;
        private int _progressBarChunks;
        private readonly Stopwatch _stopwatch;
        private readonly int _pgBarMargin = Console.WindowWidth / 6;

        public UploadProgressHandlerBuilder(long fileSize, Stopwatch stopwatch)
        {
            _fileSize = fileSize;
            _stopwatch = stopwatch;
            _progress = new Progress<long>();
            _progress.ProgressChanged += UploadProgressChanged;
            _progressPercentage = 0.0;
            _progressBarWidth = Console.WindowWidth / 2;
        }

        public IProgress<long> Build()
        {
            return _progress;
        }

        private void UploadProgressChanged(object sender, long bytesUploaded)
        {
            _progressPercentage = CalculateUploadPercentage(bytesUploaded);
            
            // -2 b/c need to account for the '[' and ']' characters delimiting the progress bar
            _progressBarChunks = (int) ((_progressBarWidth - 2) * _progressPercentage);

            DisplayPercentage();
            DisplayProgressBar();
            DisplayTimeAndUpSpeed(bytesUploaded);

        }

        private double CalculateUploadPercentage(double bytesUploaded) => bytesUploaded / ((double) _fileSize);

        private void DisplayPercentage()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            var formattedPercentage = $"{_progressPercentage * 100:0.#}";
            Console.Write($"{formattedPercentage}%");
        }
        private void DisplayProgressBar()
        {
            Console.SetCursorPosition(_pgBarMargin, Console.CursorTop);
            Console.Write("[");
            Console.Write(new string('-', _progressBarChunks));
            Console.SetCursorPosition(_pgBarMargin + _progressBarWidth - 1, Console.CursorTop);
            Console.Write("]");
        }

        private void DisplayTimeAndUpSpeed(long bytesUploaded)
        {
            var elapsedTime = _stopwatch.Elapsed;
            var uploadSpeed = ByteSize.FromBytes(bytesUploaded).MegaBytes / elapsedTime.Seconds;
            var formattedTime = $"{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";
            var formattedUpSpeed = $"({uploadSpeed:F1} MB/s)";

            var toDisplay = $"{formattedUpSpeed} {formattedTime}";
            var rightOffset = toDisplay.Length;

            Console.SetCursorPosition(Console.WindowWidth - rightOffset, Console.CursorTop);
            Console.Write(toDisplay);
        }

    }
}
