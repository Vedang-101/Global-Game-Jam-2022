using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillboardFollow : MonoBehaviour
{
    public Vector3 RotationOffset = new Vector3(-90, 0, -90);
    public Text PlayerTag;

    public void setPlayerTagText(string name)
    {
        PlayerTag.text = name;
    }
}
