using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkedGameManager : MonoBehaviour
{
    public static NetworkedGameManager instance;

    private void Awake()
    {
        instance = this;
    }

    public LightManager LightManagerGameObject;

    public NetworkedPlayerController playerObject;
    public CameraController cameraControllerObject;

    public NetworkedUIManager UIManagerObject;
    
    public bool GamePaused = true;

    public GameObject ColorableObjectsParent;

    List<ColorableObject> m_colorableObjects;
    [HideInInspector] public List<BillboardFollow> m_billboardTextures;

    bool Loaded = false;

    public float timeToPlay = 300.0f;

    public int readyPlayers = 0;
    bool LogicLoaded = false;

    public void Start()
    {
        UIManagerObject.Objectives[0] = new List<ObjectiveUI>();
        UIManagerObject.Objectives[1] = new List<ObjectiveUI>();

        m_colorableObjects = ColorableObjectsParent.GetComponentsInChildren<ColorableObject>().ToList<ColorableObject>();
        m_billboardTextures = ColorableObjectsParent.GetComponentsInChildren<BillboardFollow>().ToList<BillboardFollow>();
    }

    private void Update()
    {
        if(!Loaded)
        {
            if(playerObject != null)
            {
                Loaded = true;

                cameraControllerObject.target = playerObject.gameObject;
            }
        }
        else if(!GamePaused)
        {
            if(Input.GetKeyDown(KeyCode.P))
            {
                UIManagerObject.DisplayPause();
            }

            playerObject.UpdatePlayer();
            cameraControllerObject.UpdateCamera();

            float pitch = cameraControllerObject.transform.rotation.eulerAngles.y;
            foreach (BillboardFollow billboard in m_billboardTextures)
                if(billboard != null)
                    billboard.gameObject.transform.rotation = Quaternion.Euler(new Vector3(billboard.RotationOffset.x, pitch + 90 + billboard.RotationOffset.y, billboard.RotationOffset.z));
            foreach (ColorableObject co in m_colorableObjects)
            {
                co.WasInColor = co.IsInColor;
                co.IsInColor = false;
            }

            LightManagerGameObject.LightUpdate();

            foreach (ColorableObject co in m_colorableObjects)
            {
                co.UpdateColors();
            }
        } else
        {
            playerObject.PausePlayer();
        }
        
        if(readyPlayers == 2 && !LogicLoaded)
        {
            LogicLoaded = true;
            NetworkedLevelManager.instance.LoadObjectives();
            UIManagerObject.MainHUD.SetBool("AllClear", false);
        }
        if(LogicLoaded)
        {
            timeToPlay -= Time.deltaTime;
            if(timeToPlay <= 0)
            {
                NetworkedLevelManager.instance.CheckEndGame();
                return;
            }
            NetworkedLevelManager.instance.CheckObjectiveStates();
            UIManagerObject.UpdateTicks();
        }

        UIManagerObject.DisplayTimer(timeToPlay);
    }

    public void DisplaySuccess(int id)
    {
        if (id == 1 && PhotonNetwork.IsMasterClient)
            UIManagerObject.DisplaySuccess();
        else if(id == 2 && !PhotonNetwork.IsMasterClient)
            UIManagerObject.DisplaySuccess();
    }
}
