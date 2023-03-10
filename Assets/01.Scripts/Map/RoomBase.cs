using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomBase : MonoBehaviour
{
    [SerializeField]
    protected Define.MapTypeFlag mapTypeFlag;

    protected Define.RoomTypeFlag roomTypeFlag;
    protected bool isClear = false;

    protected abstract void SetRoomTypeFlag();
    protected abstract bool IsClear();
    
}
