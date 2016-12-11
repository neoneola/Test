using UnityEngine;
using System.Collections;

public class RotateLight : MonoBehaviour {
	public GameObject target;
	[Range(0.0f,0.3f)]
	public float speed = 0.01f;
	public float initialAngle = 0.0f;
	private float raidus = 0.0f;
	private float angel = 0.0f;
	// Use this for initialization
	void Start () {
		var positionT = target.transform.position;
		//print("sin 90 is: "+Mathf.Sin(Mathf.PI/2));
		var positionS = transform.position;
		print("Position for self is: "+positionS);
		raidus = Vector3.Distance(positionT,positionS);
		print("Raidus length is: "+raidus);

		TestClass TC=new TestClass();
		TC.TestPrint();

	}
	
	// Update is called once per frame
	void Update () {
		transform.position=new Vector3(Mathf.Cos(angel+speed+initialAngle*Mathf.PI) * raidus,Mathf.Sin(angel+speed+initialAngle*Mathf.PI) * raidus,transform.position.z);
		angel+=speed;
	}
}
