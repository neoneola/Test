using UnityEngine;
using System.Collections;
using System.Linq;

public class AudioObstruction : MonoBehaviour {

	private GameObject thePlayer;
	private AudioSource source;
	private AudioLowPassFilter LPfilter;
	private Vector3 directionToPlayer, directionCenter, directionRandom, directionRandomToPlayer;
	private Vector3 randomPointPlayer, randomPointSource;
	//private float lerpTime, currentLerpTime;
	private float lerpModifier;
	private float rayDistance;
	private float filterPercentage, avgNumber, avgTotal, occlusionPercentage, volPercentage;
	private int[] detectionList = new int[50];
	private int detectionCount;
	[SerializeField] private float detectionSize = 2f;
	private Ray rayMain, rayRandom, rayRandomToPlayer;
	private RaycastHit hitMain, hitRandom, hitRandomToPlayer;
	private bool wake;

	void Start () 
	{
		
		thePlayer = GameObject.FindGameObjectWithTag("Player");
		source = GetComponent<AudioSource>();
		source.volume = 0;
		lerpModifier = 10f;
		wake = false;
		LPfilter = GetComponent<AudioLowPassFilter>();
		detectionCount = 0;
		avgNumber = 0;
		avgTotal = 0;
	}

	void FixedUpdate ()
	{
		RayGenerator();
		Detection();
		Occlusion();

		LPfilter.cutoffFrequency = 22000f - (21500f * filterPercentage);

	}
		
	void RayGenerator()
	{
		directionCenter = thePlayer.transform.position + new Vector3(0f, 0.6f, 0f) - transform.position;
		rayDistance = directionCenter.magnitude;
		directionToPlayer = directionCenter.normalized;

		randomPointPlayer = thePlayer.transform.position + new Vector3(0f, 0.6f, 0f) + Random.insideUnitSphere * (rayDistance/5);
		directionRandom = randomPointPlayer - transform.position;
		randomPointSource = transform.position + Random.insideUnitSphere * detectionSize;

		directionRandomToPlayer = thePlayer.transform.position + new Vector3(0f, 0.6f, 0f) - randomPointPlayer;

		rayMain = new Ray(transform.position, directionCenter);
		rayRandom = new Ray(randomPointSource, directionRandom);
		rayRandomToPlayer = new Ray(randomPointPlayer, directionRandomToPlayer);

		Debug.DrawRay (transform.position, directionCenter, Color.red, 0, false);
		Debug.DrawRay (randomPointSource, directionRandom, Color.yellow, 0.2f, false);
		Debug.DrawRay (randomPointPlayer, directionRandomToPlayer, Color.blue, 0.2f, false);
	}

	void Detection()
	{

		for (int i=0; i<=detectionList.Length-2; i++)
		{
			detectionList[i] = detectionList[i+1];
		}

		//Physics.Raycast(rayMain, out hitMain, directionRandom.magnitude);

//		if (Physics.Raycast(rayMain, out hitMain, directionRandom.magnitude) && hitMain.collider.tag == "Wall")
//		{
		if (Physics.Raycast(rayRandom, out hitRandom, directionRandom.magnitude) && hitRandom.collider.tag == "Wall")
		{
			detectionList[detectionList.Length-1] = 1;
		}

		else if(Physics.Raycast(rayRandomToPlayer, out hitRandomToPlayer, directionRandomToPlayer.magnitude) && hitRandomToPlayer.collider.tag == "Wall")
		{
			detectionList[detectionList.Length-1] = 1;
		}

		else
		{
			detectionList[detectionList.Length-1] = 0;
		}
			
		avgNumber = (float) detectionList.Average();

//			detectionCount = detectionCount + 1;
//			avgTotal = avgTotal + avgNumber;
//			outputNumber = avgTotal / detectionCount;

//		}
//
//		else
//		{
//			avgNumber = 0;
//		}
			
	}

	void Occlusion()
	{
		if (wake == false)
		{
			lerpModifier -= 0.1f;
			source.volume += 0.01f;
		}

		if (lerpModifier <= 1.5f && wake == false)
		{
			lerpModifier = 1.5f;
			wake = true;
		}

		occlusionPercentage = Mathf.Lerp(occlusionPercentage, avgNumber, lerpModifier * Time.fixedDeltaTime);

		//Low Pass filter section
		filterPercentage = occlusionPercentage;

		//Volume section
		if(wake == true)
		{
		volPercentage = 0.6f + (1-occlusionPercentage) * 0.4f;
		source.volume = Mathf.Lerp(source.volume, volPercentage, 1);
		}
	}

//	void OnDrawGizmos()
//	{
//		if(Physics.Raycast(rayRandom, out hit, directionRandom.magnitude) && hit.collider.tag == "Wall")
//		{
//			Gizmos.color = Color.white;
//			Gizmos.DrawSphere(hit.point, 0.5f);
//		}
//	}


}
