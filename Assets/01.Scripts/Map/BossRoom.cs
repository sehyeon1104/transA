using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : RoomBase
{

    protected override void SetRoomTypeFlag()
    {
        roomTypeFlag = Define.RoomTypeFlag.BossRoom;
    }
    protected override bool IsClear()
    {
        // TODO : ���� Ŭ���� �Ǿ����� üũ
        return isClear;
    }
}