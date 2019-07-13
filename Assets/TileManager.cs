using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager
{
    private Tile[] tiles;

    public TileManager()
    {
        Tile[] tiles = Resources.LoadAll<Tile>("tiles");

        

        Debug.Log($"{tiles[0].name}");
    }

    public Tile GetTileById(int id) 
    {
        return null;
    }
}
