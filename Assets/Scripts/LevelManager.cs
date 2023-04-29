using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelShell
    {
        public int MirrorAdder;
        public int SplitterAdder;
        public List<ColorableObject> Objectives;
    }

    public LevelShell[] Levels;
    public PlayerController player;
    public bool UseUI;

    int m_currLevelIndex = 0;
    float m_timer = 0.0f;

    List<Vector3> MirrorPos;
    List<Quaternion> MirrorRot;

    List<Vector3> SplitterPos;
    List<Quaternion> SplitterRot;

    int LastMirrors = 0;
    int LastSplitters = 0;
    bool wasHolding = false;
    int HoldingPickable = -1;
    bool HoldingMirror = false;

    GameObject MirrorPrefab;
    GameObject SplitterPrefab;

    void MakeCheckpoint()
    {
        LastMirrors = player.MirrorsLeft;
        LastSplitters = player.SplittersLeft;
        wasHolding = player.m_pickedState;
        HoldingPickable = -1;
        HoldingMirror = false;

        MirrorPos.Clear();
        MirrorRot.Clear();
        int index = 0;
        foreach (var mirror in GameManager.instance.Mirrors)
        {
            MirrorPos.Add(mirror.transform.position);
            MirrorRot.Add(mirror.transform.rotation);
            if (mirror == player.m_pickableObject)
            {
                HoldingPickable = index;
                HoldingMirror = true;
            }
            index++;
        }

        SplitterPos.Clear();
        SplitterRot.Clear();
        index = 0;
        foreach (var splitter in GameManager.instance.Splitter)
        {
            SplitterPos.Add(splitter.transform.position);
            SplitterRot.Add(splitter.transform.rotation);
            if (splitter == player.m_pickableObject)
            {
                HoldingPickable = index;
                HoldingMirror = false;
            }
            index++;
        }
    }

    public void Reset()
    {
        foreach (var mirror in GameManager.instance.Mirrors)
        {
            Destroy(mirror);
        }
        foreach (var splitter in GameManager.instance.Splitter)
        {
            Destroy(splitter);
        }


        GameManager.instance.Mirrors.Clear();
        GameManager.instance.Splitter.Clear();

        for (int i = 0; i < MirrorPos.Count; i++)
        {
            GameManager.instance.Mirrors.Add(Instantiate(MirrorPrefab, MirrorPos[i], MirrorRot[i]));
        }
        for (int i = 0; i < SplitterPos.Count; i++)
        {
            GameManager.instance.Splitter.Add(Instantiate(SplitterPrefab, SplitterPos[i], SplitterRot[i]));
        }

        player.MirrorsLeft = LastMirrors;
        player.SplittersLeft = LastSplitters;

        player.PickUpIndicatorAnimator.SetBool("ShowIndicator", false);

        if (wasHolding)
        {
            player.m_pickableObject = HoldingMirror? GameManager.instance.Mirrors[HoldingPickable]: GameManager.instance.Splitter[HoldingPickable];
            player.PickFromReset();
        }
    }

    public void SetupLevel()
    {
        MirrorPos = new List<Vector3>();
        MirrorRot = new List<Quaternion>();

        SplitterPos = new List<Vector3>();
        SplitterRot = new List<Quaternion>();

        MirrorPrefab = player.MirrorPrefab;
        SplitterPrefab = player.SplitterPrefab;

        player.MirrorsLeft += Levels[m_currLevelIndex].MirrorAdder;
        player.SplittersLeft += Levels[m_currLevelIndex].SplitterAdder;
        if(UseUI)
            UpdateUI();

        MakeCheckpoint();
    }

    public void CheckObjectiveStates()
    {
        if (FinishedGame())
            return;
        
        bool objectivesCompleted = true;
        foreach(ColorableObject co in Levels[m_currLevelIndex].Objectives)
        {
            if (co.IsInColor == false)
            {
                objectivesCompleted = false;
                break;
            }
        }

        if(objectivesCompleted)
        {
            m_timer += Time.deltaTime;
            if(m_timer >= 0.5f)
            {
                m_currLevelIndex++;
                if (!FinishedGame())
                {
                    player.MirrorsLeft += Levels[m_currLevelIndex].MirrorAdder;
                    player.SplittersLeft += Levels[m_currLevelIndex].SplitterAdder;
                    MakeCheckpoint();
                    if (UseUI)
                    {
                        UpdateUI();
                        GameManager.instance.UIManagerObject.DisplaySuccess();
                    }
                } else if(UseUI)
                {
                    ClearObjectivesUI();
                    GameManager.instance.UIManagerObject.DisplayGameFinished();
                }
                m_timer = 0.0f;
            }
        }
        else
        {
            m_timer = 0.0f;
        }
    }

    public bool FinishedGame()
    {
        return m_currLevelIndex == Levels.Length;
    }

    private void UpdateUI()
    {
        GameManager.instance.UIManagerObject.DisplayNewObjectives(Levels[m_currLevelIndex].Objectives);
    }

    private void ClearObjectivesUI()
    {
        GameManager.instance.UIManagerObject.DisplayNewObjectives(new List<ColorableObject>());
    }

}
