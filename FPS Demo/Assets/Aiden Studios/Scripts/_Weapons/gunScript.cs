﻿using UnityEngine;
using System.Collections;

public class gunScript : MonoBehaviour
{

    public int clipSize = 6;
    public Animation anim;
    public AnimationClip shoot;
    public AnimationClip reload;
    public AnimationClip draw;
    [HideInInspector]
    public int ammo = 0;
    [HideInInspector]
    public int oldAmmo = 0;
    float coolDown = 0;
    public int maxDamage = 50;
    public int lowDamage = 30;
    public bool singleShot = false;
    public float rateOfFire = 0.1f;
    public float range = 1500;
    public float recoilAmnt = 0.2f;
    public float recoilShake = 5f;
    public float recoilSpeed = 5f;
    public Transform forward;

    public float accuracy = 0.3f;

    public camRecoil recoilScript;

    public Transform[] pelletSpawns;

    public GameObject groundHit;
    public GameObject bloodHit;

    public Transform muzzle;
    public GameObject muzzleFlash;

    public AudioClip shootSFX;
    public AudioClip reloadSFX;

    public int magazines = 10;

    public PhotonView tpEff;

    void Awake()
    {
        ammo = clipSize;

        setLayer();
    }

    void Update()
    {
        if (!singleShot)
        {
            if (Input.GetButton("Fire1"))
            {
                Fire();
            }
        } else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                FireSingle();
            }
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            rel();
        }

        if(coolDown > 0)
        {
            coolDown += -Time.deltaTime;
        }
    }


    public void Fire()
    {
        if (coolDown <= 0 && !anim.IsPlaying(reload.name) && !anim.IsPlaying(draw.name))
        {
            if (ammo > 0)
            {
                anim.Play(shoot.name);
                ammo += -1;
                coolDown += rateOfFire;
                if(recoilScript != null)
                {
                    recoilScript.StartRecoil(recoilAmnt, recoilShake, recoilSpeed);
                }
                tpEff.RPC("shootEff", PhotonTargets.All, shootSFX.name);
                if (muzzleFlash && muzzle != null)
                {
                    GameObject mf = Instantiate(muzzleFlash, muzzle.position, muzzle.rotation) as GameObject;
                    mf.transform.parent = transform;
                    Destroy(mf, 1);
                }
                if (pelletSpawns.Length == 0)
                {
                    RaycastHit hit;
                    Vector3 fwd = new Vector3(forward.forward.x + Random.Range(-accuracy, accuracy) / 10, forward.forward.y + Random.Range(-accuracy, accuracy) / 10, forward.forward.z);
                    if (Physics.Raycast(forward.position, fwd, out hit, range))
                    {


                        if (hit.transform.tag == "Player")
                        {
                            //Debug.Log("hit!");
                            if (hit.transform.GetComponent<PhotonView>() != null)
                            {
                                tpEff.gameObject.SendMessage("hitCol", hit.collider);
                                hit.transform.GetComponent<PhotonView>().RPC("ApplyDamage", PhotonTargets.AllBuffered, Random.Range(lowDamage, maxDamage), PhotonNetwork.playerName, hit.collider.name);                               
                            }
                            GameObject par = Instantiate(bloodHit, hit.point, Quaternion.identity) as GameObject;
                            Destroy(par, 5f);
                        }
                        else
                        {
                            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            GameObject par = Instantiate(groundHit, hit.point, hitRotation) as GameObject;
                            Destroy(par, 5f);
                        }
                    }
                } else
                {
                   
                    foreach (Transform pelletSpawn in pelletSpawns)
                    {
                        RaycastHit hit;

                        if (Physics.Raycast(pelletSpawn.position, pelletSpawn.forward, out hit, range))
                        {


                            if (hit.transform.tag == "Player")
                            {
                                Debug.Log("hit!");
                                if (hit.transform.GetComponent<PhotonView>() != null)
                                {
                                    hit.transform.GetComponent<PhotonView>().RPC("ApplyDamage", PhotonTargets.AllBuffered, Random.Range(lowDamage, maxDamage), PhotonNetwork.playerName, hit.collider.name);
                                }
                                GameObject par = Instantiate(bloodHit, hit.point, Quaternion.identity) as GameObject;
                                Destroy(par, 5f);
                            }
                            else
                            {
                                Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                                GameObject par = Instantiate(groundHit, hit.point, hitRotation) as GameObject;
                                Destroy(par, 5f);
                            }
                        }
                    }
                }

                
            }
            else
            {
                rel();
            }
        }
    }


    public void FireSingle()
    {
        if (!anim.IsPlaying(shoot.name) && !anim.IsPlaying(reload.name) && !anim.IsPlaying(draw.name))
        {
            if (ammo > 0)
            {
                anim.Play(shoot.name);
                ammo += -1;
                coolDown += rateOfFire;
                tpEff.RPC("shootEff", PhotonTargets.All, shootSFX.name);
                if (recoilScript != null)
                {
                    recoilScript.StartRecoil(recoilAmnt, recoilShake, recoilSpeed);
                }
                if (muzzleFlash && muzzle != null)
                {
                    GameObject mf = Instantiate(muzzleFlash, muzzle.position, muzzle.rotation) as GameObject;
                    mf.transform.parent = transform;
                    Destroy(mf, 1);
                }
                if (pelletSpawns.Length == 0)
                {
                    RaycastHit hit;
                    Vector3 fwd = new Vector3(forward.forward.x + Random.Range(-accuracy, accuracy) / 10, forward.forward.y + Random.Range(-accuracy, accuracy) / 10, forward.forward.z);
                    if (Physics.Raycast(forward.position, fwd, out hit, range))
                    {


                        if (hit.transform.tag == "Player")
                        {
                            //Debug.Log("hit!");
                            if (hit.transform.GetComponent<PhotonView>() != null)
                            {
                                tpEff.gameObject.SendMessage("hitCol", hit.collider);
                                hit.transform.GetComponent<PhotonView>().RPC("ApplyDamage", PhotonTargets.AllBuffered, Random.Range(lowDamage, maxDamage), PhotonNetwork.playerName, hit.collider.name);
                            }
                            GameObject par = Instantiate(bloodHit, hit.point, Quaternion.identity) as GameObject;
                            Destroy(par, 5f);
                        }
                        else
                        {
                            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            GameObject par = Instantiate(groundHit, hit.point, hitRotation) as GameObject;
                            Destroy(par, 5f);
                        }
                    }
                }
                else
                {
                    foreach (Transform pelletSpawn in pelletSpawns)
                    {
                        RaycastHit hit;

                        if (Physics.Raycast(pelletSpawn.position, pelletSpawn.forward, out hit, range))
                        {


                            if (hit.transform.tag == "Player")
                            {
                                Debug.Log("hit!");
                                if (hit.transform.GetComponent<PhotonView>() != null)
                                {
                                    hit.transform.GetComponent<PhotonView>().RPC("ApplyDamage", PhotonTargets.AllBuffered, Random.Range(lowDamage, maxDamage), PhotonNetwork.playerName, hit.collider.name);
                                }
                                GameObject par = Instantiate(bloodHit, hit.point, Quaternion.identity) as GameObject;
                                Destroy(par, 5f);
                            }
                            else
                            {
                                Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                                GameObject par = Instantiate(groundHit, hit.point, hitRotation) as GameObject;
                                Destroy(par, 5f);
                            }
                        }
                    }
                }


            }
            else
            {
                rel();
            }
        }
    }

    public void rel()
    {
        if (ammo < clipSize && magazines > 0)
        {
            oldAmmo = ammo;
            anim.CrossFade(reload.name);
            ammo = clipSize;
            magazines += -1;
            tpEff.RPC("reload", PhotonTargets.AllBuffered, reloadSFX.name);
        }
    }

    void OnEnable()
    {
        anim.Play(draw.name);
    }

    void OnDisable()
    {
        if (anim.IsPlaying(reload.name))
        {
            ammo = 0;
            magazines += 1;
        }
    }

    void OnGUI()
    {
        if (GameObject.Find("_Room") != null)
        {
            GUI.skin = GameObject.Find("_Room").GetComponent<roomManager>().skin;
        }
        GUIStyle ammoStyle = new GUIStyle(GUI.skin.box);
        ammoStyle.fontSize = 36;
        GUIStyle ammoStyle2 = new GUIStyle(GUI.skin.box);
        ammoStyle2.fontSize = 24;
        ammoStyle2.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(10, Screen.height - 100, 100, 50), ammo + "/" + clipSize, ammoStyle);
        GUI.Box(new Rect(100, Screen.height - 100, 50, 50), "/" + magazines, ammoStyle2);
    }

    public void setLayer()
    {
        foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            mr.gameObject.layer = 9;
        }

        foreach (SkinnedMeshRenderer mr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            mr.gameObject.layer = 9;
        }     
    }
}
