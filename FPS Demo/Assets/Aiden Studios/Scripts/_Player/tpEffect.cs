﻿using UnityEngine;
using System.Collections;

public class tpEffect : MonoBehaviour {

    public Transform muzzle;
    public GameObject muzzleFlash;
    public AudioClip[] gunSFX;
    public bool bot = false;
    public GameObject[] tpWeps;
    public AnimationClip idleReload;
    public AnimationClip walkingReload;
    public Animation am;


    

    [PunRPC]
    public void shootEff(string gunShot)
    {
        if (!GetComponent<PhotonView>().isMine)
        {
            if (muzzleFlash && muzzle != null)
            {
                GameObject mf = Instantiate(muzzleFlash, muzzle.position, muzzle.rotation) as GameObject;
                mf.transform.parent = transform;
                Destroy(mf, 1);
            }
        }
        else
        {
            if (bot)
            {
                if (muzzleFlash && muzzle != null)
                {
                    GameObject mf = Instantiate(muzzleFlash, muzzle.position, muzzle.rotation) as GameObject;
                    mf.transform.parent = transform;
                    Destroy(mf, 1);
                }
            }
        }
    

        foreach(AudioClip sfx in gunSFX)
        {
            if(sfx.name == gunShot)
            {
                GetComponent<AudioSource>().PlayOneShot(sfx);
            }
        }
        if (idleReload != null)
        {
            if (am.IsPlaying(idleReload.name) && GetComponent<Rigidbody>().velocity.magnitude >= 0.1f)
            {
                am.CrossFade(walkingReload.name);
            }

            if (am.IsPlaying(walkingReload.name) && GetComponent<Rigidbody>().velocity.magnitude == 0)
            {
                am.CrossFade(idleReload.name);
            }
        }

    }

    [PunRPC]
    public void updateModel(string nm)
    {
        foreach(GameObject wep in tpWeps)
        {
            if(wep.name == nm)
            {
                wep.SetActive(true);
            } else
            {
                wep.SetActive(false);
            }
        }
    }

    [PunRPC]
    public void reload(string rel)
    {
        foreach (AudioClip sfx in gunSFX)
        {
            if (sfx.name == rel)
            {
                GetComponent<AudioSource>().PlayOneShot(sfx);
            }
        }

        if (idleReload != null)
        {
            if (GetComponent<Rigidbody>().velocity.magnitude <= 0)
            {
                am.CrossFade(idleReload.name);
            }
            else
            {
                am.CrossFade(walkingReload.name);
            }
        }
    }

}
