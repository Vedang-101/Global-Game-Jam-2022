using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    public Image IconImage;
    public ColorableObject ObjectiveReference;
    public Animator Ticked;

    public void PopOut()
    {
        GetComponent<Animator>().SetBool("Visible", false);
    }

    public void PopIn()
    {
        GetComponent<Animator>().SetBool("Visible", true);
    }

    public void setIcon(Sprite icon)
    {
        IconImage.sprite = icon;
    }

    public void setTick(bool tick)
    {
        Ticked.SetBool("ShowTick", tick);
    }
}
