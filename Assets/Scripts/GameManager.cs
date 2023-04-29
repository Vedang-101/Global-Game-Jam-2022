using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
    }

    public LightManager LightManagerGameObject;
    public CameraController CameraControllerObject;

    public List<PlayerController> PlayerControllerObjects;
    public List<LevelManager> LevelManagers;
    public UIManager UIManagerObject;

    public GameObject ColorableObjectsParent;

    List<ColorableObject> m_colorableObjects;
    List<BillboardFollow> m_billboardTextures;

    [Header("Game States")]
    public bool GamePaused = false;

    [HideInInspector] public List<GameObject> Mirrors;
    [HideInInspector] public List<GameObject> Splitter;

    void Start()
    {
        Mirrors = new List<GameObject>();
        Splitter = new List<GameObject>();

        if (!GamePaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        m_colorableObjects = ColorableObjectsParent.GetComponentsInChildren<ColorableObject>().ToList<ColorableObject>();
        m_billboardTextures = ColorableObjectsParent.GetComponentsInChildren<BillboardFollow>().ToList<BillboardFollow>();

        for (int i = 0; i < PlayerControllerObjects.Count; i++)
        {
            LevelManagers[i].player = PlayerControllerObjects[i];
            LevelManagers[i].UseUI = true;
            LevelManagers[i].SetupLevel();
        }
    }

    void Update()
    {
        if(!GamePaused)
        {
            foreach (PlayerController player in PlayerControllerObjects)
                player.UpdatePlayer();
            CameraControllerObject.UpdateCamera();

            float pitch = CameraControllerObject.transform.rotation.eulerAngles.y;
            foreach (BillboardFollow billboard in m_billboardTextures)
                billboard.gameObject.transform.rotation = Quaternion.Euler(new Vector3(billboard.RotationOffset.x, pitch + 90 + billboard.RotationOffset.y, billboard.RotationOffset.z));

            if (Input.GetKeyDown(KeyCode.P))
            {
                UIManagerObject.ShowPause();
            }
        }
        else
        {
            foreach (PlayerController player in PlayerControllerObjects)
                player.PausePlayer();
        }

        foreach(ColorableObject co in m_colorableObjects)
        {
            co.WasInColor = co.IsInColor;
            co.IsInColor = false;
        }

        LightManagerGameObject.LightUpdate();

        foreach (ColorableObject co in m_colorableObjects)
        {
            co.UpdateColors();
        }

        foreach(LevelManager levelManager in LevelManagers)
        {
            levelManager.CheckObjectiveStates();
            UIManagerObject.UpdateTicks();
        }
    }

    public void Pause_unPause()
    {
        GamePaused = !GamePaused;
        
        if (GamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void resetCheckPoint()
    {
        foreach (LevelManager levelManager in LevelManagers)
        {
            levelManager.Reset();
            UIManagerObject.UpdateTicks();
        }
    }
}
