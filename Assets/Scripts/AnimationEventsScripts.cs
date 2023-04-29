using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationEventsScripts : MonoBehaviour
{
    public void ResumeMainHUD()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.UIManagerObject.MainHUD.SetBool("AllClear", false);
            GameManager.instance.UIManagerObject.MainHUD.SetBool("OnlyObjective", false);
            GameManager.instance.GamePaused = false;
        }
        else if(NetworkedGameManager.instance != null)
        {
            NetworkedGameManager.instance.UIManagerObject.MainHUD.SetBool("AllClear", false);
            NetworkedGameManager.instance.UIManagerObject.MainHUD.SetBool("OnlyObjective", false);
            NetworkedGameManager.instance.GamePaused = false;
        }
    }

    public void ResetSuccess()
    {
        this.gameObject.SetActive(false);
    }

    public void InitializeMuliplayerGame()
    {
        NetworkedLevelManager.instance.StartGame();
    }
}
