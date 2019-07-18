using System;
using Illarion.Client.Common;

namespace Illarion.Client.Unity.Scene.Ingame
{
    public class Player : IMovementSupplier
    {
        public event EventHandler<Vector2i> MovementDone;
        public event EventHandler<int> LayerChanged;

        protected virtual void OnMovementDone(int x, int y)
        {
            EventHandler<Vector2i> handler = MovementDone;
            handler?.Invoke(this, new Vector2i(x, y));
        }

        protected virtual void OnLayerChanged(int z)
        {
            EventHandler<int> handler = LayerChanged;
            handler?.Invoke(this, z);
        }
    }
}