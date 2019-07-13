using System;
using Illarion.Client.Common;

namespace Illarion.Client
{
    public static class Game 
    {
        private static bool _initialized;

        private static IFileSystem _fileSystem;
        private static ILogger _logger;
        private static IConfig _config;

        public static IFileSystem FileSystem
        {
            get
            {
                if (!_initialized) throw new InvalidOperationException("Game not yet initialized!");
                return _fileSystem;
            }
        }

        public static ILogger Logger
        {
            get
            {
                if (!_initialized) throw new InvalidOperationException("Game not yet initialized!");
                return _logger;
            }
        }

        public static IConfig Config
        {
            get
            {
                if (!_initialized) throw new InvalidOperationException("Game not yet initialized!");
                return _config;
            }
        }

    }
}