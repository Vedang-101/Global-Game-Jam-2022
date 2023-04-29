using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource SFXSource;
    
    public AudioClip HitSFX;
    public AudioClip PickUpSFX;
    public AudioClip ErrorSFX;
    public AudioClip SuccessSFX;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance= this;
        }
    }

    public void PlayGameWin()
    {
        SFXSource.PlayOneShot(SuccessSFX);
    }

    public void PlayPickUp(Vector3 position)
    {
        SFXSource.transform.position = position;
        SFXSource.PlayOneShot(PickUpSFX);
    }

    public void PlayTick(Vector3 position)
    {
        SFXSource.transform.position = position;
        SFXSource.PlayOneShot(HitSFX);
    }

    public void PlayError(Vector3 position)
    {
        SFXSource.transform.position = position;
        SFXSource.PlayOneShot(ErrorSFX);
    }
}
