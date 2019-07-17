using System;

namespace Illarion.Client.Common
{
    public interface IMovementSupplier
    {
        event EventHandler<Vector2i> MovementDone;
        event EventHandler<int> LayerChanged;
    }
}