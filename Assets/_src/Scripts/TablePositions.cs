using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TablePositions : MonoBehaviour
{
    public Transform clientPlayerTransform;
    public Transform remotePlayerTransform;
    void Start()
    {
        clientPlayerTransform = GameObject.FindWithTag("Client Player").transform;
        remotePlayerTransform = GameObject.FindWithTag("Remote Player").transform;
    }
}
