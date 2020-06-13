using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerState : MonoBehaviour
{
    public ulong Id;
    public ulong MoveState;
    public ulong State;
    public Vector3 Pos;
    public float Scale;
    public bool IsOwner;
}
