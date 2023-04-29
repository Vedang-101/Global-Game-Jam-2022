using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitGame : MonoBehaviour
{
    public void InitializeGame()
    {
        GameManager.instance.UIManagerObject.MainHUD.SetBool("AllClear", false);
        GameManager.instance.Pause_unPause();
    }
}
