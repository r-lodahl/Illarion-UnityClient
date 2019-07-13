using System;
using System.Collections.Generic;
using Illarion.Client.Common;

namespace Illarion.Client.Map
{
	[Serializable]
	public class Chunk
	{
		public int[][] Map {get;private set;}
		public int[] Layers {get;private set;}
		public int[] Origin {get;private set;}
		public Dictionary<Vector3i, MapObject[]> Items {get;private set;}
		public Dictionary<Vector3i, Vector3i> Warps {get;private set;}

		public Chunk(int[][] map, int[] layers, int[] origin, Dictionary<Vector3i, MapObject[]> items, Dictionary<Vector3i, Vector3i> warps)
		{
			Map = map;
			Layers = layers;
			Origin = origin;
			Items = items;
			Warps = warps;
		}
	}
}