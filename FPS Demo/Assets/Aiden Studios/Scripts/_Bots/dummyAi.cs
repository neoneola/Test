using UnityEngine;
using System.Collections;

public class dummyAi : MonoBehaviour {

    public int health = 100;
    public Collider head;
    public GameObject ragdoll;
    string killer = "World";
    PhotonView pv;
    public string prefName = "Bot";

    public GameObject wepDrop;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void FixedUpdate()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    [PunRPC]
    public void ApplyDamage(int dmg, string name, string col)
    {
        killer = name;
        if (col != head.name)
        {
            health += -dmg;
        } else
        {
            health += -dmg * 4;
        }      

        if (pv.isMine)
        {
            pv.RPC("setTarget", PhotonTargets.AllBuffered, name);

           
        }
        

    }


    bool ragDollSpawned = false;

    
    public void Die()
    {
        if (pv.isMine && pv.owner.isMasterClient)
        {
            if (!ragDollSpawned)
            {
                PhotonNetwork.Instantiate(ragdoll.name, transform.position, transform.rotation, 0);
                ragDollSpawned = true;              

                if (GameObject.Find(killer) != null)
                {
                    if (GameObject.Find(killer).GetComponent<PhotonView>() != null)
                    {
                        if (GameObject.Find(killer).GetComponent<tpEffect>() != null)
                        {
                            GameObject.Find(killer).GetComponent<PhotonView>().RPC("gotKill", PhotonTargets.AllBuffered, gameObject.name);
                            GameObject.Find("_Network").GetComponent<PhotonView>().RPC("addFeed", PhotonTargets.AllBuffered, killer, gameObject.name);
                        }
                    }
                }

            }

            GameObject.Find("_Room").SendMessage("spawnBot", prefName);

            if (wepDrop != null)
            {
                PhotonNetwork.Instantiate(wepDrop.name, transform.position, transform.rotation, 0);
            }

            pv.RPC("killMe", PhotonTargets.AllBuffered, null);

        }
    }

    [PunRPC]
    public void killMe()
    {
        Destroy(gameObject);
    }

    [PunRPC]
    public void gotKill(string vic)
    {
        //
    }
}
