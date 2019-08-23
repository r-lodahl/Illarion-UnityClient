using System;

namespace Illarion.Client.Map
{
    [Serializable]
    public struct MapObject
    {
        public string Description {get;set;}
        public string Name {get;set;}
        public int BaseId {get;set;}
    }
}