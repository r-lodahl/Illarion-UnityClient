namespace Illarion.Client.Common
{
    public static class Constants 
    {
        public static class UserData
        {
            // Internal Path
            public const string TilesetPath = "tiles/";

            public const string VersionPath = "/map.version";
            public const string MapPath = "/map/";
            public const string ServerMapPath = "/Illarion-Map/";
        }

        public static class Update 
        {
            public const string ServerAddress = "https://c107-243.cloud.gwdg.de/";
            public const string MapVersionEndpoint = "api/map/version";
            public const string MapDataEndpoint = "api/map/zipball";
            
            // Internal Path: must be .txt .html .htm .xml .bytes .json .csv .yaml .fnt
            public const string TileTablePath = "tables/Tiles";
            public const string OverlayTablePath = "tables/Overlays";

            public const string TileFileName = "/tileMapping.bin";
            public const string OverlayFileName = "/overlayMapping.bin";

            public const int TileNameColumn = 3;
            public const int TileIdColumn = 9;
            public const int OverlayNameColumn = 3;
            public const int OverlayIdColumn = 2;
        }

        public static class Tile
        {
            public const int OverlayFactor = 1000;
            
            public const int ShapeIdMask = 0xFC00;
            public const int OverlayIdMask = 0x03E0;
            public const int BaseIdMask = 0x001F;
        }

        public static class Scene
        {
            public const int Map = 0;
        }

        public static class Map
        {
            public const int Chunksize = 20;
            public const int VisibleLayers = 10;
            public const int LayerDrawingFactor = 2;
            public const int OverlayDrawingAdd = 1;
        }
    }
}