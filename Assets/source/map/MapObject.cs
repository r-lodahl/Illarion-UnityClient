using System;

namespace Illarion.Client.Map
{
    [Serializable]
    public class MapObject
    {
        public string Description {get;set;}
        public string Name {get;set;}
        public int ObjectId {get;set;}
    }
}