using System;

namespace Illarion.Client.Common
{
    /// <summary>
    /// Static game class allowing easy access to major game components
    /// </summary>
    public static class Game 
    {
        private static bool _initialized;

        private static IFileSystem _fileSystem;
        private static ILogger _logger;
        private static IConfig _config;

        public static void Initialize(IFileSystem fileSystem, ILogger logger, IConfig config)
        {
            if (_initialized) throw new InvalidOperationException("Game already initialized!");

            _fileSystem = fileSystem;
            _logger = logger;
            _config = config;

            _initialized = true;
        }

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