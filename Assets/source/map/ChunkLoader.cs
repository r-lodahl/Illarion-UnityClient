using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;
using Illarion.Client.Common;

namespace Illarion.Client.Map
{
	public class ChunkLoader
	{
        public event EventHandler<Chunk> ChunkLoaded;
        public event EventHandler<Chunk> ChunkUnloading;

        protected virtual void OnChunkLoaded(Chunk chunk)
        {
            EventHandler<Chunk> handler = ChunkLoaded;
            handler?.Invoke(this, chunk);
        }

        protected virtual void OnChunkUnloading(Chunk chunk)
        {
            EventHandler<Chunk> handler = ChunkUnloading;
            handler?.Invoke(this, chunk);
        }

        // Position-Tracking
		private int x, y;
		private int chunkX, chunkY;

        // Loaded Chunks
		private Chunk[] activeChunks;

        // Chunk-Reader
		private BinaryFormatter binaryFormatter;

		public ChunkLoader(int x, int y, IMovementSupplier movementSupplier) 
		{
			movementSupplier.MovementDone += OnMovementDone;

			this.x = x;
			this.y = y;

			chunkX = x / Constants.Map.Chunksize;
			chunkY = y / Constants.Map.Chunksize;

			activeChunks = new Chunk[9];

			binaryFormatter = new BinaryFormatter();

			ReloadChunks(new int[]{0,1,2,3,4,5,6,7,8});
		}

		private void OnMovementDone(object e, Vector2i movement)
		{
			x += movement.x;
			y += movement.y;

			HashSet<int> reloadChunks = new HashSet<int>();

			if (movement.x == -1 && x%Constants.Map.Chunksize == 0) 
			{
				chunkX--;

				for (int i = 1; i < 9; i+=3)
				{
					activeChunks[i+1] = activeChunks[i];
					activeChunks[i] = activeChunks[i-1];
				}

				reloadChunks.Add(0);
				reloadChunks.Add(3);
				reloadChunks.Add(6);
			}
			else if (movement.x == 1 && x%Constants.Map.Chunksize == 0) 
			{
				chunkX++;
				
				for (int i = 1; i < 9; i+=3)
				{
					activeChunks[i-1] = activeChunks[i];
					activeChunks[i] = activeChunks[i+1];
				}

				reloadChunks.Add(2);
				reloadChunks.Add(5);
				reloadChunks.Add(8);
			}

			if (movement.y == -1 && y%Constants.Map.Chunksize == 0)
			{
				chunkY--;
				
				for (int i = 3; i < 6; i++)
				{
					activeChunks[i+3] = activeChunks[i];
					activeChunks[i] = activeChunks[i-3];
				}

				reloadChunks.Add(0);
				reloadChunks.Add(1);
				reloadChunks.Add(2);
			}
			else if (movement.y == 1 && y%Constants.Map.Chunksize == 0)
			{
				chunkY++;
				
				for (int i = 3; i < 6; i++)
				{
					activeChunks[i-3] = activeChunks[i];
					activeChunks[i] = activeChunks[i+3];
				}

				reloadChunks.Add(6);
				reloadChunks.Add(7);
				reloadChunks.Add(8);
			}

			if (reloadChunks.Count == 0) return;

			ReloadChunks(reloadChunks);
		}

		private void ReloadChunks(IEnumerable<int> chunkList) 
		{
			foreach (int chunkId in chunkList)
			{
                OnChunkUnloading(activeChunks[chunkId]);
				activeChunks[chunkId] = LoadChunk(chunkId);
                OnChunkLoaded(activeChunks[chunkId]);
			}
		}
		
		private Chunk LoadChunk(int chunkId) 
		{
			string chunkPath = String.Concat(
					Game.FileSystem.UserDirectory,
					"/map/chunk_",
					chunkX + (chunkId % 3 - 1),
					"_",
					chunkY + (chunkId / 3 - 1),
					".bin"); 

			FileInfo mapFile = new FileInfo(chunkPath);

			if (!mapFile.Exists) {
				Game.Logger.Error($"does not exits {chunkPath}");
				return null;
			}

			object rawChunk;
			using(var file = mapFile.OpenRead())
			{
				rawChunk = binaryFormatter.Deserialize(file);
				file.Flush();
			}

			Chunk chunk = rawChunk as Chunk;
			if (chunk != null) return chunk;

			Game.Logger.Error($"Malformed chunk at x: {chunkX + (chunkId % 3 - 1)} and y: {chunkY + (chunkId / 3 - 1)}!");
			return null;
		}
	}
}
