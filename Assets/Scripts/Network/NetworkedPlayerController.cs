using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class NetworkedPlayerController : MonoBehaviour
{
    public float MovementSpeed = 1.0f;
    public float RotationLag = 7.5f;
    public float MovementLag = 50f;

    public Transform PickupPoint;

    public Animator PlayerAnimator;
    public Animator PickUpIndicatorAnimator;

    public BillboardFollow PlayerNameTag;
    public Collider TriggerCollider;

    Rigidbody m_rigidbody;
    Camera m_mainCamera;

    [HideInInspector] public GameObject m_pickableObject = null;
    [HideInInspector] public bool m_pickedState = false;

    [HideInInspector]public PhotonView PV;

    public int MirrorsLeft;
    public int SplittersLeft;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        PickUpIndicatorAnimator.transform.parent = null;
    }

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_mainCamera = Camera.main;
        string nickName = PV.Owner.NickName.Substring(0, PV.Owner.NickName.Length - 6);
        PlayerNameTag.setPlayerTagText(nickName);
        NetworkedGameManager.instance.m_billboardTextures.Add(PlayerNameTag);
        NetworkedGameManager.instance.m_billboardTextures.Add(PickUpIndicatorAnimator.GetComponentInChildren<BillboardFollow>());
        if (!PV.IsMine)
            RemoveAllPhysics();
    }

    public void RemoveAllPhysics()
    {
        m_rigidbody.isKinematic = true;
        TriggerCollider.enabled = false;
    }

    public void UpdatePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 forward = m_mainCamera.transform.forward;
        Vector3 right = m_mainCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = forward * moveZ + right * moveX;
        m_rigidbody.velocity = Vector3.Lerp(m_rigidbody.velocity, desiredMoveDirection.normalized * MovementSpeed, Time.deltaTime * MovementLag);
        PlayerAnimator.SetFloat("Movement", m_rigidbody.velocity.normalized.magnitude);

        Vector3 cameraDirection = transform.position - m_mainCamera.transform.position;
        cameraDirection.y = 0;
        Quaternion finalRotation = Quaternion.LookRotation(cameraDirection);

        if (desiredMoveDirection.magnitude > 0)
            finalRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, Time.deltaTime * RotationLag);

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if(m_pickedState == true)
            {
                PV.RPC("UpdateMirrors", RpcTarget.All, PV.Owner.NickName, false, 0, transform.position, transform.rotation.eulerAngles);
            } else if(m_pickableObject != null)
            {
                int index = -1;
                if (m_pickableObject.tag == "Mirror")
                {
                    index = InstantiateManager.instance.MirrorList.IndexOf(m_pickableObject);
                    if (index != -1)
                        PV.RPC("UpdateMirrors", RpcTarget.All, PV.Owner.NickName, true, index, transform.position, transform.rotation.eulerAngles);
                }
                else
                {
                    index = InstantiateManager.instance.SplitterList.IndexOf(m_pickableObject);
                    if (index != -1)
                        PV.RPC("UpdateSplitters", RpcTarget.All, PV.Owner.NickName, true, index, transform.position, transform.rotation.eulerAngles);
                }
                
            } else
            {
                if(MirrorsLeft > 0)
                {
                    MirrorsLeft--;
                    PV.RPC("UpdateMirrors", RpcTarget.All, PV.Owner.NickName, true, -1, transform.position, transform.rotation.eulerAngles);
                }
                else
                {
                    //NetworkedGameManager.instance.UIManagerObject.TriggerMirrorCross();
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (SplittersLeft > 0)
            {
                SplittersLeft--;
                PV.RPC("UpdateSplitters", RpcTarget.All, PV.Owner.NickName, true, -1, transform.position, transform.rotation.eulerAngles);
            }
            else
            {
                NetworkedGameManager.instance.UIManagerObject.TriggerSplitterCross();
            }
        }

        NetworkedGameManager.instance.UIManagerObject.UpdatePlayerInventory(MirrorsLeft, SplittersLeft);
    }

    public void PausePlayer()
    {
        m_rigidbody.velocity = Vector3.zero;
        PlayerAnimator.SetFloat("Movement", m_rigidbody.velocity.normalized.magnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.tag == "Mirror" || other.tag == "Splitter") && !m_pickedState)
        {
            if (other.transform.parent.GetComponentInChildren<ColorableObject>() != null) return;
            if (InstantiateManager.instance.notPickables.Contains(other.transform.parent.gameObject)) return;

            m_pickableObject = other.transform.parent.gameObject;
            PickUpIndicatorAnimator.transform.position = m_pickableObject.transform.position;
            PickUpIndicatorAnimator.SetBool("ShowIndicator", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.tag == "Mirror" || other.tag == "Splitter") && !m_pickedState && m_pickableObject == other.transform.parent.gameObject)
        {
            if (other.transform.parent.GetComponentInChildren<ColorableObject>() != null) return;
            
            PickUpIndicatorAnimator.SetBool("ShowIndicator", false);
            m_pickableObject = null;
        }
    }

    [PunRPC]
    void UpdateMirrors(string CommandSender, bool wantToPickUp, int mirrorIndex, Vector3 position, Vector3 rotation)
    {
        if (CommandSender == PV.Owner.NickName)
        {
            transform.position = position;
            transform.rotation = Quaternion.Euler(rotation);
            SoundManager.instance.PlayPickUp(transform.position);
            InstantiateManager.instance.UpdateMirrors(this, CommandSender, wantToPickUp, mirrorIndex);
        }
    }

    [PunRPC]
    void UpdateSplitters(string CommandSender, bool wantToPickUp, int splitterIndex, Vector3 position, Vector3 rotation)
    {
        if (CommandSender == PV.Owner.NickName)
        {
            transform.position = position;
            transform.rotation = Quaternion.Euler(rotation);
            SoundManager.instance.PlayPickUp(transform.position);
            InstantiateManager.instance.UpdateSplitters(this, CommandSender, splitterIndex);
        }
    }
}
