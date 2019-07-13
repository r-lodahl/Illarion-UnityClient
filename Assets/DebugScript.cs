using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TileManager loader = new TileManager();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(1f / Time.deltaTime);
    }
}
