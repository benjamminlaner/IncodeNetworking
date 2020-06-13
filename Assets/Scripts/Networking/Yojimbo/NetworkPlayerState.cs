using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO (BEN): Replace with struct
public class NetworkPlayerState
{
    public ulong Id;
    public ulong MoveState;
    public ulong State;
    public Vector3 Pos;
    public float Scale;
    public bool IsOwner;
}
