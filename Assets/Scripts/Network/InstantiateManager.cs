using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateManager : MonoBehaviour
{
    public static InstantiateManager instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Spawn Points")]
    public Transform Player1Spawn;
    public Transform Player2Spawn;

    [Header("Mirrors and Splitters")]
    public GameObject MirrorPrefab;
    public List<GameObject> MirrorList;

    public GameObject SplitterPrefab;
    public List<GameObject> SplitterList;
    
    public List<GameObject> notPickables;

    public ParticleSystem DustFX;

    public Dictionary<string, GameObject> Holdings = new Dictionary<string, GameObject>();

    public void UpdateMirrors(NetworkedPlayerController caller, string key, bool wantToPickUp, int mirrorIndex)
    {
        if (!wantToPickUp)
        {
            GameObject mirrorObject = null;
            if (Holdings.ContainsKey(key) && Holdings[key] != null)
            {
                mirrorObject = Holdings[key];
                if (mirrorObject.tag == "Mirror")
                    MirrorList.Add(Holdings[key]);
                else
                    SplitterList.Add(Holdings[key]);
                notPickables.Remove(Holdings[key]);
                Holdings[key] = null;
            } else
                return;

            mirrorObject.transform.SetParent(null);
            mirrorObject.transform.position = mirrorObject.transform.position - new Vector3(0, mirrorObject.transform.position.y, 0);
            mirrorObject.transform.rotation = Quaternion.Euler(new Vector3(0, mirrorObject.transform.rotation.eulerAngles.y, 0));
            mirrorObject.transform.GetChild(0).gameObject.SetActive(true);
            DustFX.gameObject.transform.position = mirrorObject.transform.position;
            DustFX.Play();

            if (caller.PV.Owner.IsLocal)
            {
                caller.PickUpIndicatorAnimator.transform.localPosition = mirrorObject.transform.position;
                caller.PickUpIndicatorAnimator.SetBool("ShowIndicator", true);
            }

            caller.m_pickableObject = null;

            caller.m_pickedState = false;
            caller.PlayerAnimator.SetBool("PickedState", caller.m_pickedState);
        } else
        {
            if(mirrorIndex == -1)
            {
                GameObject mirrorObject = null;
                if (!Holdings.ContainsKey(key) || Holdings[key] == null)
                {
                    if (!Holdings.ContainsKey(key))
                        Holdings.Add(key, null);
                    
                    Holdings[key] = Instantiate(MirrorPrefab, Vector3.zero, Quaternion.identity);
                    mirrorObject = Holdings[key];
                    notPickables.Add(Holdings[key]);
                } else
                    return;
                
                mirrorObject.transform.SetParent(caller.PickupPoint);
                mirrorObject.transform.localPosition = Vector3.zero;
                mirrorObject.transform.localRotation = Quaternion.identity;
                mirrorObject.transform.GetChild(0).gameObject.SetActive(false);
                caller.m_pickedState = true;

                caller.PlayerAnimator.SetBool("MirrorState", mirrorObject.transform.tag == "Mirror");
                caller.PlayerAnimator.SetBool("PickedState", caller.m_pickedState);
            } else
            {
                GameObject mirrorObject = null;
                if (!Holdings.ContainsKey(key) || Holdings[key] == null)
                {
                    if (!Holdings.ContainsKey(key))
                        Holdings.Add(key, null);
                    Holdings[key] = MirrorList[mirrorIndex];
                    MirrorList.RemoveAt(mirrorIndex);
                    mirrorObject = Holdings[key];
                    notPickables.Add(Holdings[key]);
                }
                else
                    return;

                mirrorObject.transform.SetParent(caller.PickupPoint);
                mirrorObject.transform.localPosition = Vector3.zero;
                mirrorObject.transform.localRotation = Quaternion.identity;
                mirrorObject.transform.GetChild(0).gameObject.SetActive(false);
                caller.m_pickedState = true;

                if (caller.PV.Owner.IsLocal)
                {
                    caller.PickUpIndicatorAnimator.SetBool("ShowIndicator", false);
                }

                caller.PlayerAnimator.SetBool("MirrorState", mirrorObject.transform.tag == "Mirror");
                caller.PlayerAnimator.SetBool("PickedState", caller.m_pickedState);
            }
        }
    }

    public void UpdateSplitters(NetworkedPlayerController caller, string key, int splitterIndex)
    {
        {
            if (splitterIndex == -1)
            {
                GameObject mirrorObject = null;
                if (!Holdings.ContainsKey(key) || Holdings[key] == null)
                {
                    if (!Holdings.ContainsKey(key))
                        Holdings.Add(key, null);

                    Holdings[key] = Instantiate(SplitterPrefab, Vector3.zero, Quaternion.identity);
                    mirrorObject = Holdings[key];
                    notPickables.Add(Holdings[key]);
                }
                else
                    return;

                mirrorObject.transform.SetParent(caller.PickupPoint);
                mirrorObject.transform.localPosition = Vector3.zero;
                mirrorObject.transform.localRotation = Quaternion.identity;
                mirrorObject.transform.GetChild(0).gameObject.SetActive(false);
                caller.m_pickedState = true;

                caller.PlayerAnimator.SetBool("MirrorState", mirrorObject.transform.tag == "Mirror");
                caller.PlayerAnimator.SetBool("PickedState", caller.m_pickedState);
            }
            else
            {
                GameObject mirrorObject = null;
                if (!Holdings.ContainsKey(key) || Holdings[key] == null)
                {
                    if (!Holdings.ContainsKey(key))
                        Holdings.Add(key, null);
                    Holdings[key] = SplitterList[splitterIndex];
                    SplitterList.RemoveAt(splitterIndex);
                    mirrorObject = Holdings[key];
                    notPickables.Add(Holdings[key]);
                }
                else
                    return;

                mirrorObject.transform.SetParent(caller.PickupPoint);
                mirrorObject.transform.localPosition = Vector3.zero;
                mirrorObject.transform.localRotation = Quaternion.identity;
                mirrorObject.transform.GetChild(0).gameObject.SetActive(false);
                caller.m_pickedState = true;

                if (caller.PV.Owner.IsLocal)
                {
                    caller.PickUpIndicatorAnimator.SetBool("ShowIndicator", false);
                }

                caller.PlayerAnimator.SetBool("MirrorState", mirrorObject.transform.tag == "Mirror");
                caller.PlayerAnimator.SetBool("PickedState", caller.m_pickedState);
            }
        }
    }
}