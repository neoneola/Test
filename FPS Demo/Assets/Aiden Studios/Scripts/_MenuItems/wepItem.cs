using UnityEngine;
using System.Collections;

public class wepItem : MonoBehaviour {

    public Texture icon;
    public string displayName = "Aks-74u";
    public string prefabName = "Aks";
    public string description = "A cool gun";
    public int clipSize = 30;
    public int maxDamage = 15;
    public GameObject classFor;
    public GameObject[] sightAttachments;
    public GameObject[] barrelAttachments;
    public int xpRequired = 100;

    [HideInInspector]
    public bool showAttachments = false;
}
