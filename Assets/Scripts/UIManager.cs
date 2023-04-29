using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Objectives")]
    public GameObject ObjectivesHolder;
    public GameObject ObjectiveCardPrefab;
    public float DistanceBetweenTwoCards = 140f;

    [Header("Inventory Text")]
    public Text MirrorCountText;
    public Text SplitterCountText;
    public Animator MirrorCross;
    public Animator SplitterCross;

    [Header("UI Animations")]
    public Animator MainHUD;
    public Animator SuccessHUD;
    public Animator GameFinishedHUD;
    public Animator PauseHUD;

    List<ObjectiveUI> Objectives = new List<ObjectiveUI>();

    public void UpdateTicks()
    {
        foreach(ObjectiveUI objective in Objectives)
        {
            if(objective.ObjectiveReference.IsInColor != objective.ObjectiveReference.WasInColor)
            {
                objective.setTick(objective.ObjectiveReference.IsInColor);
            }
        }
    }

    public void DisplayNewObjectives(List<ColorableObject> objectives)
    {
        StopAllCoroutines();
        StartCoroutine(LoadNewObjectives(objectives));
    }

    IEnumerator LoadNewObjectives(List<ColorableObject> objectives)
    {
        foreach(ObjectiveUI obj in Objectives)
        {
            obj.PopOut();
            Destroy(obj.gameObject, 0.1f);
        }
        Objectives.Clear();

        float totalSize = DistanceBetweenTwoCards * (objectives.Count-1);

        float XPos = -totalSize/2;
        foreach(ColorableObject obj in objectives)
        {
            GameObject objCard = Instantiate(ObjectiveCardPrefab, ObjectivesHolder.transform);
            objCard.GetComponent<RectTransform>().localPosition = new Vector3(XPos, 0, 0);
            ObjectiveUI objCardUI = objCard.GetComponent<ObjectiveUI>();
            objCardUI.ObjectiveReference = obj;
            objCardUI.setIcon(obj.IconImage);
            objCardUI.PopIn();
            Objectives.Add(objCardUI);
            if(obj.IsInColor)
                objCardUI.setTick(true);
            XPos += DistanceBetweenTwoCards;
            yield return new WaitForSeconds(0.25f);
        }

        yield return null;
    }

    public void UpdatePlayerInventory(int MirrorCount, int SplitterCount)
    {
        MirrorCountText.text = MirrorCount > 0 ? MirrorCount.ToString() : "NaN";
        SplitterCountText.text = SplitterCount > 0 ? SplitterCount.ToString() : "NaN";
    }

    public void TriggerMirrorCross()
    {
        SoundManager.instance.PlayError(Vector3.zero);
        MirrorCross.SetTrigger("Pop");
    }
    public void TriggerSplitterCross()
    {
        SoundManager.instance.PlayError(Vector3.zero);
        SplitterCross.SetTrigger("Pop");
    }

    public void DisplaySuccess()
    {
        SoundManager.instance.PlayGameWin();
        GameManager.instance.GamePaused = true;
        MainHUD.SetBool("OnlyObjective", true);
        GameFinishedHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(false);
        SuccessHUD.gameObject.SetActive(true);
        SuccessHUD.Play("Success",0);
    }

    public void DisplayGameFinished()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SoundManager.instance.PlayGameWin();
        GameManager.instance.GamePaused = true;
        MainHUD.SetBool("AllClear", true);
        SuccessHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(false);
        GameFinishedHUD.gameObject.SetActive(true);
        GameFinishedHUD.Play("GameFinished", 0);
    }

    public void ShowPause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SoundManager.instance.PlayGameWin();
        GameManager.instance.GamePaused = true;
        MainHUD.SetBool("AllClear", true);
        SuccessHUD.gameObject.SetActive(false);
        GameFinishedHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(true);
        PauseHUD.Play("Pause", 0);
    }

    public void RemovePause()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SoundManager.instance.PlayGameWin();
        GameManager.instance.GamePaused = false;
        SuccessHUD.gameObject.SetActive(false);
        GameFinishedHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(true);
        PauseHUD.Play("Pause_Out", 0);
    }

    public void ResetCheckpoint()
    {
        GameManager.instance.resetCheckPoint();
        RemovePause();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
