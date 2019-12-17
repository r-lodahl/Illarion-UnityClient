using System;

namespace Illarion.Client.Map
{
    /// <summary>
    /// Represents a single object on the map
    /// </summary>
    [Serializable]
    public struct MapObject
    {
        public string Description {get;set;}
        public string Name {get;set;}
        public int BaseId {get;set;}
    }
}