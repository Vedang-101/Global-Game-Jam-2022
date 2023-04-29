using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float MovementSpeed = 1.0f;
    public float RotationLag = 7.5f;
    public float MovementLag = 50f;

    public Transform PickupPoint;

    public Animator PlayerAnimator;

    [Header("Juice")]
    public ParticleSystem DustFX;

    public Animator PickUpIndicatorAnimator;


    [Header("Level Logic")]
    public GameObject MirrorPrefab;
    public GameObject SplitterPrefab;
    public int MirrorsLeft;
    public int SplittersLeft;


    Rigidbody m_rigidbody;
    Camera m_mainCamera;

    [HideInInspector] public GameObject m_pickableObject = null;
    [HideInInspector] public bool m_pickedState = false;

    private void Awake()
    {
        GameManager.instance.CameraControllerObject.target = this.gameObject;
        GameManager.instance.PlayerControllerObjects.Add(this);
    }

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_mainCamera = Camera.main;
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

        if (Input.GetMouseButtonDown(0))
        {
            if (m_pickedState == true)
            {
                SoundManager.instance.PlayPickUp(transform.position);
                m_pickableObject.transform.SetParent(null);
                m_pickableObject.transform.position = m_pickableObject.transform.position - new Vector3(0, m_pickableObject.transform.position.y, 0);
                m_pickableObject.transform.rotation = Quaternion.Euler(new Vector3(0, m_pickableObject.transform.rotation.eulerAngles.y, 0));
                m_pickableObject.transform.GetChild(0).gameObject.SetActive(true);
                DustFX.gameObject.transform.position = m_pickableObject.transform.position;
                DustFX.Play();

                PickUpIndicatorAnimator.transform.localPosition = m_pickableObject.transform.position;
                PickUpIndicatorAnimator.SetBool("ShowIndicator", true);


                m_pickableObject = null;

                m_pickedState = false;
                PlayerAnimator.SetBool("PickedState", m_pickedState);
            }
            else if (m_pickableObject != null)
            {
                SoundManager.instance.PlayPickUp(transform.position);
                m_pickableObject.transform.SetParent(PickupPoint);
                m_pickableObject.transform.localPosition = Vector3.zero;
                m_pickableObject.transform.localRotation = Quaternion.identity;
                m_pickableObject.transform.GetChild(0).gameObject.SetActive(false);
                m_pickedState = true;

                PickUpIndicatorAnimator.SetBool("ShowIndicator", false);

                PlayerAnimator.SetBool("MirrorState", m_pickableObject.transform.tag == "Mirror");
                PlayerAnimator.SetBool("PickedState", m_pickedState);
            }
            else
            {
                if (MirrorsLeft > 0)
                {
                    SoundManager.instance.PlayPickUp(transform.position);
                    MirrorsLeft--;
                    m_pickableObject = Instantiate(MirrorPrefab);

                    m_pickableObject.transform.SetParent(PickupPoint);
                    m_pickableObject.transform.localPosition = Vector3.zero;
                    m_pickableObject.transform.localRotation = Quaternion.identity;
                    m_pickableObject.transform.GetChild(0).gameObject.SetActive(false);
                    m_pickedState = true;

                    GameManager.instance.Mirrors.Add(m_pickableObject);

                    PlayerAnimator.SetBool("MirrorState", m_pickableObject.transform.tag == "Mirror");
                    PlayerAnimator.SetBool("PickedState", m_pickedState);
                }
                else
                {
                    GameManager.instance.UIManagerObject.TriggerMirrorCross();
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (SplittersLeft > 0)
            {
                SoundManager.instance.PlayPickUp(transform.position);
                SplittersLeft--;
                m_pickableObject = Instantiate(SplitterPrefab);

                m_pickableObject.transform.SetParent(PickupPoint);
                m_pickableObject.transform.localPosition = Vector3.zero;
                m_pickableObject.transform.localRotation = Quaternion.identity;
                m_pickableObject.transform.GetChild(0).gameObject.SetActive(false);
                m_pickedState = true;

                GameManager.instance.Splitter.Add(m_pickableObject);

                PlayerAnimator.SetBool("MirrorState", m_pickableObject.transform.tag == "Mirror");
                PlayerAnimator.SetBool("PickedState", m_pickedState);
            }
            else
            {
                GameManager.instance.UIManagerObject.TriggerSplitterCross();
            }
        }

        GameManager.instance.UIManagerObject.UpdatePlayerInventory(MirrorsLeft, SplittersLeft);
    }

    public void PausePlayer()
    {
        m_rigidbody.velocity = Vector3.zero;
        PlayerAnimator.SetFloat("Movement", m_rigidbody.velocity.normalized.magnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.tag == "Mirror" || other.tag == "Splitter") && !m_pickedState)
        {
            if (other.transform.parent.GetComponentInChildren<ColorableObject>() != null) return;

            m_pickableObject = other.transform.parent.gameObject;
            PickUpIndicatorAnimator.transform.localPosition = m_pickableObject.transform.position;
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

    public void PickFromReset()
    {
        SoundManager.instance.PlayPickUp(transform.position);
        m_pickableObject.transform.SetParent(PickupPoint);
        m_pickableObject.transform.localPosition = Vector3.zero;
        m_pickableObject.transform.localRotation = Quaternion.identity;
        m_pickableObject.transform.GetChild(0).gameObject.SetActive(false);
        m_pickedState = true;

        PickUpIndicatorAnimator.SetBool("ShowIndicator", false);

        PlayerAnimator.SetBool("MirrorState", m_pickableObject.transform.tag == "Mirror");
        PlayerAnimator.SetBool("PickedState", m_pickedState);
    }
}
