using UnityEngine;
using System.Collections;

public class smoothFollow : MonoBehaviour {

     
    public Transform target;
     
    public float distance = 10.0f;
     
    public float height = 5.0f;
     
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;

    GUISkin skin;

    roomManager rM;

    void Awake()
    {
        rM = GameObject.FindObjectOfType<roomManager>();
        if (rM != null)
        {
            skin = rM.skin;
        }
    }


    void LateUpdate()
    {
        
        if (!target)
            return;        

        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        
        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        
        transform.LookAt(target);
    }

    void OnGUI()
    {
        GUI.skin = skin;

        if (target != null && !rM.spawned)
        {
            GUI.Box(new Rect(Screen.width - 350, 10, 300, 30), "Observing: " + target.name);
        }
    }

}
