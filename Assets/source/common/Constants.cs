namespace Illarion.Client.Common
{
    public static class Constants 
    {
        public static class UserData
        {
            public const string TilesetPath = "tiles/";
            public const string ItemsetPath = "items/items";

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
            public const string TileFileName = "/tileMapping.bin";
            public const int TileNameColumn = 3;
            public const int TileIdColumn = 9;

            public const string OverlayTablePath = "tables/Overlays";
            public const string OverlayFileName = "/overlayMapping.bin";
            public const int OverlayNameColumn = 3;
            public const int OverlayIdColumn = 2;
            
            public const string ItemFileName = "/itemMapping.bin";
            public const string ItemBaseFileName = "/itemBase.bin";
            public const string ItemTablePath = "tables/Items";
            public const string ItemOffsetPath = "items/offsets";

            public const int ItemIdColumn = 2;
            public const int ItemNameColumn = 3;
            public const int ItemFrameCountColumn = 4;
            public const int ItemModeColumn = 5;
            public const int ItemOffsetXColumn = 6;
            public const int ItemOffsetYColumn = 7;
            public const int ItemAnimationSpeedColumn = 8;
            public const int ItemScalingColumn = 15;
            public const int ItemSurfaceLevelColumn = 20;
            public const int ItemLightEmitColumn = 21;
            public const int ItemColorModRedColumn = 23;
            public const int ItemColorModGreenColumn = 24;
            public const int ItemColorModBlueColumn = 25;
            public const int ItemColorModAlphaColumn = 26;
        }

        public static class Tile
        {
            public const int OverlayFactor = 1000;
            
            public const int ShapeIdMask = 0xFC00;
            public const int OverlayIdMask = 0x03E0;
            public const int BaseIdMask = 0x001F;

            public const int SizeX = 76;
        }

        public static class Scene
        {
            public const int Map = 1;
        }

        public static class Map
        {
            public const int Chunksize = 20;
            public const int VisibleLayers = 10;
            
            public const int LayerDrawingFactor = 4;

            public const int OverlayDrawingAdd = 2;
            public const int OverlayCellMinus = 1;
        }

        public enum ItemMode
        {
            Simple = 0,
            Animated = 1,
            Variance = 2
        }
    }
}