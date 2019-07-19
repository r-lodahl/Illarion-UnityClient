using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Linq;
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
		}

		public void ReloadChunks()
		{
			ReloadChunks(new int[]{0,1,2,3,4,5,6,7,8});
		}

		private void NotifyUnloading(int chunkMoveX, int chunkMoveY)
		{
			HashSet<int> unloadedChunkIds = new HashSet<int>();

			if (chunkMoveX == 1) unloadedChunkIds.UnionWith(new int[] {0, 3, 6});
			else if (chunkMoveX == -1) unloadedChunkIds.UnionWith(new int[] {2, 5, 8});
			
			if (chunkMoveY == 1) unloadedChunkIds.UnionWith(new int[] {0, 1, 2});
			else if (chunkMoveY == -1) unloadedChunkIds.UnionWith(new int[] {6, 7, 8});

			foreach (int chunkId in unloadedChunkIds) OnChunkUnloading(activeChunks[chunkId]);
		}

		private IEnumerable<int> ShiftChunks(int chunkMoveX, int chunkMoveY)
		{
			HashSet<int> reloadChunkIds = new HashSet<int>();
			Chunk[] shiftedChunks = new Chunk[9];

			if (chunkMoveX == 1)
			{
				Array.Copy(activeChunks, 1, shiftedChunks, 0, 2);
				Array.Copy(activeChunks, 4, shiftedChunks, 3, 2);
				Array.Copy(activeChunks, 7, shiftedChunks, 6, 2);

				reloadChunkIds.UnionWith(new int[] {2, 5, 8});
			}
			else if (chunkMoveX == -1)
			{
				Array.Copy(activeChunks, 0, shiftedChunks, 1, 2);
				Array.Copy(activeChunks, 3, shiftedChunks, 4, 2);
				Array.Copy(activeChunks, 6, shiftedChunks, 7, 2);

				reloadChunkIds.UnionWith(new int[] {0, 3, 6});
			}
			else
			{
				shiftedChunks = activeChunks;
			}

			activeChunks = shiftedChunks;

			if (chunkMoveY == 1)
			{
				Array.Copy(activeChunks, 3, shiftedChunks, 0, 6);

				reloadChunkIds.UnionWith(new int[] {6, 7, 8});
			}
			else if (chunkMoveY == -1)
			{
				Array.Copy(activeChunks, 0, shiftedChunks, 3, 6);

				reloadChunkIds.UnionWith(new int[] {0, 1, 2});
			}

			activeChunks = shiftedChunks;

			chunkX += chunkMoveX;
			chunkY += chunkMoveY;

			return reloadChunkIds;
		}

		private void OnMovementDone(object sender, Vector2i movement)
		{
			x += movement.x;
			y += movement.y;

			int chunkMoveX = 0;
			int chunkMoveY = 0;

			if (x == -1) { chunkMoveX = -1; x = 19; }
			else if (x == 20) { chunkMoveX = 1; x = 0; }

			if (y == -1) { chunkMoveY = -1; y = 19; }
			else if (y == 20) { chunkMoveY = 1; y = 0; }

			if (chunkMoveX == 0 && chunkMoveY == 0) return;

			NotifyUnloading(chunkMoveX, chunkMoveY);

			ReloadChunks(ShiftChunks(chunkMoveX, chunkMoveY));
		}

		private void ReloadChunks(IEnumerable<int> chunkList) 
		{
			foreach (int chunkId in chunkList)
			{
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
