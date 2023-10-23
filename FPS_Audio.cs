using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Audio : MonoBehaviour
{
    private AudioSource playerAudio;
    private Object_Base scrObject;
    [SerializeField] private List<AudioClip> clipConc = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipCarpet = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipMetal = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipWet = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipLeaves = new List<AudioClip>();

    [SerializeField] private List<AudioClip> clipLanding = new List<AudioClip>();

    [SerializeField] private float landVol, stepVol;

    void Start()
    {
        playerAudio = GetComponent<AudioSource>();
    }

    public void Footsteps(Transform hitObject)
    {
        scrObject = hitObject.GetComponent<Object_Base>();
        playerAudio.pitch = Random.Range(0.95f, 1.05f);

        if(scrObject != null)
        {
            switch (scrObject.mySound)
            {
                case SoundType.Wet:
                    playerAudio.PlayOneShot(clipWet[Random.Range(0, clipWet.Count)], stepVol);
                    break;
                case SoundType.Carpet:
                    playerAudio.PlayOneShot(clipCarpet[Random.Range(0, clipCarpet.Count)], stepVol);
                    break;
                case SoundType.Metal:
                    playerAudio.PlayOneShot(clipMetal[Random.Range(0, clipMetal.Count)], stepVol);
                    break;
                case SoundType.Leaves:
                    playerAudio.PlayOneShot(clipLeaves[Random.Range(0, clipLeaves.Count)], stepVol);
                    break;
                case SoundType.Concrete:
                    playerAudio.PlayOneShot(clipConc[Random.Range(0, clipConc.Count)], stepVol);
                    break;
                default:
                    playerAudio.PlayOneShot(clipConc[Random.Range(0, clipConc.Count)], stepVol);
                    break;
            }
        }
        else
        {
            playerAudio.PlayOneShot(clipConc[Random.Range(0, clipConc.Count)], stepVol);
        }
    }

    public void FootLanding(Transform hitObject)
    {
        scrObject = hitObject.GetComponent<Object_Base>();
        playerAudio.pitch = Random.Range(0.95f, 1.05f);

        if (scrObject != null)
        {
            switch (scrObject.mySound)
            {
                case SoundType.Concrete:
                    playerAudio.PlayOneShot(clipLanding[0], landVol);
                    break;
                case SoundType.Carpet:
                    playerAudio.PlayOneShot(clipLanding[1], landVol);
                    break;
                case SoundType.Metal:
                    playerAudio.PlayOneShot(clipLanding[2], landVol);
                    break;
                case SoundType.Wet:
                    playerAudio.PlayOneShot(clipLanding[3], landVol);
                    break;
                case SoundType.Leaves:
                    playerAudio.PlayOneShot(clipLanding[4], landVol);
                    break;
                default:
                    break;
            }
        }
        else
        {
            playerAudio.PlayOneShot(clipLanding[0], landVol);
        }
    }
}
