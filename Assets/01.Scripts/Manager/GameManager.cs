using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{

    public void GameQuit()
    {
        Application.Quit();
    }

}