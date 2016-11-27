using UnityEngine;
using System.Collections;

public class wepManager : MonoBehaviour
{

    public string className = "Infantry";
    public GameObject[] primaries;
    GameObject primary;
    public GameObject secondary;
    public PhotonView tpEffect;
    public Animation knifeAm;
    public AnimationClip knifeSwipe;
    public int knifeDmg = 200;
    public float knifeRange = 3;

    public float knifeRecoil = -0.2f;
    public camRecoil recoilScript;

    public GameObject bloodHit;

    public AudioClip knifeSound;


    void Awake()
    {

        primary = primaries[0];

        foreach (GameObject pr in primaries)
        {
            if (pr.name == PlayerPrefs.GetString(className + "wep"))
            {
                primary = pr;
            }
            else {
                pr.SetActive(false);
            }
        }

        choosePrim();
    }


    public void choosePrim()
    {
        gunScript gS = secondary.GetComponent<gunScript>();
        projectileLauncher pL = secondary.GetComponent<projectileLauncher>();
        curIsPrim = true;
        if (gS != null)
        {
            if (gS.anim.IsPlaying(gS.reload.name))
            {
                gS.ammo = gS.oldAmmo;
                gS.magazines += 1;
                gS.anim.Play(gS.shoot.name);
            }
        }

        if (pL != null)
        {
            if (pL.anim.IsPlaying(pL.reload.name))
            {
                pL.ammo = pL.oldAmmo;
                pL.magazines += 1;
                pL.anim.Play(pL.shoot.name);
            }
        }

        primary.SetActive(true);
        secondary.SetActive(false);
        tpEffect.RPC("updateModel", PhotonTargets.AllBuffered, primary.name);
    }

    public void chooseSec()
    {
        gunScript gS = primary.GetComponent<gunScript>();
        projectileLauncher pL = primary.GetComponent<projectileLauncher>();
        curIsPrim = false;
        if (gS != null)
        {
            if (gS.anim.IsPlaying(gS.reload.name))
            {
                gS.ammo = gS.oldAmmo;
                gS.magazines += 1;
                gS.anim.Play(gS.shoot.name);
            }
        }

        if (pL != null)
        {
            if (pL.anim.IsPlaying(pL.reload.name))
            {
                pL.ammo = pL.oldAmmo;
                pL.magazines += 1;
                pL.anim.Play(pL.shoot.name);
            }
        }
        primary.SetActive(false);
        secondary.SetActive(true);
        tpEffect.RPC("updateModel", PhotonTargets.AllBuffered, secondary.name);
    }

    void Update()
    {

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            choosePrim();
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            chooseSec();
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (primary.activeInHierarchy)
            {
                chooseSec();
            }
            else
            {
                choosePrim();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (primary.activeInHierarchy)
            {

            }
            meleeAttack();
        }

        if (knifeAm.IsPlaying(knifeSwipe.name))
        {
            primary.SetActive(false);
            secondary.SetActive(false);
        } else
        {
            if(!primary.activeInHierarchy || !secondary.activeInHierarchy)
            {
                if (curIsPrim)
                {
                    choosePrim();
                } else
                {
                    chooseSec();
                }
            }
        }

        
    }

    bool curIsPrim = true;

    public void pickup(string nme)
    {

        foreach (GameObject pr in primaries)
        {
            if (pr.name == nme)
            {
                if (primary != pr)
                {
                    primary = pr;
                }
                else
                {
                    if (primary.GetComponent<projectileLauncher>() == null)
                    {
                        primary.GetComponent<gunScript>().magazines += 5;
                    }
                    else
                    {
                        primary.GetComponent<projectileLauncher>().magazines += 3;
                    }
                }
            }
            else {
                pr.SetActive(false);
            }
        }

        choosePrim();
    }

    public void meleeAttack()
    {
        if (knifeAm != null && knifeSwipe != null)
        {
            if (!knifeAm.IsPlaying(knifeSwipe.name))
            {
                knifeAm.Play(knifeSwipe.name);

                if (recoilScript != null)
                {
                    recoilScript.StartRecoil(knifeRecoil, -5, 5);
                }

                if(knifeSound != null)
                {
                    tpEffect.RPC("shootEff", PhotonTargets.All, knifeSound.name);
                }
                RaycastHit hit;

                if (Physics.Raycast(transform.position, transform.forward, out hit, knifeRange))
                {
                    if (hit.transform.GetComponent<PhotonView>() != null)
                    {
                        if (hit.transform.GetComponent<tpEffect>() != null || hit.transform.GetComponent<zombieAi>() != null)
                        {
                            hit.transform.GetComponent<PhotonView>().RPC("ApplyDamage", PhotonTargets.AllBuffered, knifeDmg, PhotonNetwork.playerName, hit.collider.name);
                            GameObject par = Instantiate(bloodHit, hit.point, Quaternion.identity) as GameObject;
                            Destroy(par, 5f);
                        }
                        else
                        {
                            if(hit.transform.GetComponentInParent<PhotonView>() != null)
                            {
                                if (hit.transform.GetComponentInParent<tpEffect>() != null || hit.transform.GetComponentInParent<zombieAi>() != null)
                                {
                                    hit.transform.GetComponentInParent<PhotonView>().RPC("ApplyDamage", PhotonTargets.AllBuffered, knifeDmg, PhotonNetwork.playerName, hit.collider.name);
                                    GameObject par = Instantiate(bloodHit, hit.point, Quaternion.identity) as GameObject;
                                    Destroy(par, 5f);
                                }
                            }
                        }
                    } 
                }
            }
        }
    }
}
