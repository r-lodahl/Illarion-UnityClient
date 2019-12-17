using System;
using UnityEngine;
using Illarion.Client.Common;

namespace Illarion.Client.Unity.Scene.Ingame
{
    /// <summary>
    /// Player stub to allow movement over the map
    /// </summary>
    public class Player : MonoBehaviour, IMovementSupplier
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (Input.GetKeyDown(KeyCode.W)) 
                {
                    transform.Translate(-0.5f, 0.25f, 0f);
                    OnMovementDone(0, -1);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    transform.Translate(-0.5f, -0.25f, 0f);
                    OnMovementDone(-1, 0);
                }
                else
                {
                    transform.Translate(-1f, 0f, 0f);
                    OnMovementDone(-1, -1);
                }
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                if (Input.GetKeyDown(KeyCode.W)) 
                {
                    transform.Translate(0.5f, 0.25f, 0f);
                    OnMovementDone(1, 0);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    transform.Translate(0.5f, -0.25f, 0f);
                    OnMovementDone(0, 1);
                }
                else
                {
                    transform.Translate(1f, 0f, 0f);
                    OnMovementDone(1, 1);
                }
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                transform.Translate(0f, 0.5f, 0f);
                OnMovementDone(1, -1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                transform.Translate(0f, -0.5f, 0f);
                OnMovementDone(-1, 1);
            }
        }
    }
}