namespace Illarion.Client.Common
{
    public static class Constants 
    {
        public static class UserData
        {
            public const string TilesetPath = "";
            public const string VersionPath = "";
            public const string MapPath = "";
            public const string ServerMapPath = "";
        }

        public static class Update 
        {
            public const string ServerAddress = "";
            public const string MapVersionEndpoint = "";
            public const string MapDataEndpoint = "";
            
            public const string TileTablePath = "";
            public const string TileFileName = "";
            public const string OverlayTablePath = "";
            public const string OverlayFileName = "";
            public const int TileNameColumn = 0;
            public const int TileIdColumn = 0;
            public const int OverlayNameColumn = 0;
            public const int OverlayIdColumn = 0;
        }

        public static class Tile
        {
            public const int OverlayFactor = 1000;
            
            public const int ShapeIdMask = 0;
            public const int OverlayIdMask = 0;
            public const int BaseIdMask = 0;
        }

        public static class Map
        {
            public const int Chunksize = 20;
        }
    }
}