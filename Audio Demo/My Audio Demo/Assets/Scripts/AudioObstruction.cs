using UnityEngine;
using System.Collections;
using System.Linq;

public class AudioObstruction : MonoBehaviour {

	private GameObject thePlayer;
	private AudioSource source;
	private AudioLowPassFilter LPfilter;
	private Vector3 direction_SourceToPlayer, direction_CenterRay, direction_RandomToRandom, directionRandomToPlayer;
	private Vector3 randomPoint_AroundPlayer, randomPoint_AroundSource;
    private Vector3 head = new Vector3(0f, 0.6f, 0f);
	//private float lerpTime, currentLerpTime;
	private float lerpModifier;
	private float rayDistance;
	private float filterPercentage, avgNumber, avgTotal, occlusionPercentage, volPercentage;
	private int[] detectionList;
    private int detectionCount, detectionCount2;
    [SerializeField] private int detectionSize = 64;
	[SerializeField] private float sourceSphereSize = 2f;
	private Ray ray_Main, ray_Random, ray_RandomToPlayer;
	private RaycastHit hit_Main, hit_Random, hit_RandomToPlayer;
	private bool wake;

	void Start () 
	{
		
		thePlayer = GameObject.FindGameObjectWithTag("Player");
		source = GetComponent<AudioSource>();
		source.volume = 0;
		lerpModifier = 10f;
		wake = false;
		LPfilter = GetComponent<AudioLowPassFilter>();
        detectionList = new int[detectionSize + 1];
		detectionCount = 0;
        detectionCount2 = 0;
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
		direction_CenterRay = thePlayer.transform.position + head - transform.position;
		rayDistance = direction_CenterRay.magnitude;
		direction_SourceToPlayer = direction_CenterRay.normalized;

		randomPoint_AroundPlayer = thePlayer.transform.position + head + Random.insideUnitSphere * (rayDistance/5);
		direction_RandomToRandom = randomPoint_AroundPlayer - randomPoint_AroundSource;
		randomPoint_AroundSource = transform.position + Random.insideUnitSphere * sourceSphereSize;

		directionRandomToPlayer = thePlayer.transform.position + head - randomPoint_AroundPlayer;

		ray_Main = new Ray (transform.position, direction_CenterRay);
		ray_Random = new Ray (randomPoint_AroundSource, direction_RandomToRandom);
		ray_RandomToPlayer = new Ray (randomPoint_AroundPlayer, directionRandomToPlayer);

		Debug.DrawRay (transform.position, direction_CenterRay, Color.red, 0, false);
		Debug.DrawRay (randomPoint_AroundSource, direction_RandomToRandom, Color.yellow, 0.2f, false);
		Debug.DrawRay (randomPoint_AroundPlayer, directionRandomToPlayer, Color.blue, 0.2f, false);
	}

	void Detection()
	{
        detectionCount += 1;

        if (detectionCount > detectionSize)
        {
            detectionCount = 0;
        }

        if (detectionCount <= (detectionSize / 2))
        {
            detectionCount2 = detectionCount + (detectionSize / 2);
        }

        if (detectionCount > (detectionSize / 2))
        {
            detectionCount2 = detectionCount - (detectionSize / 2);
        }

		//for (int i=0; i<=detectionList.Length-2; i++)
		//{
		//	detectionList[i] = detectionList[i+1];
		//}

		if (Physics.Raycast(ray_Random, out hit_Random, direction_RandomToRandom.magnitude) && hit_Random.collider.tag == "Wall")
		{
			int x = 0;
			RaycastHit[] allHits = Physics.RaycastAll(ray_Random, direction_RandomToRandom.magnitude);
			foreach(RaycastHit oneHit in allHits)
			{
				if (oneHit.collider.tag == "Wall")
				{
					x += 1;
				}
			}
            detectionList[detectionCount] = x;
            avgTotal += detectionList[detectionCount];
		}

		else if(Physics.Raycast(ray_RandomToPlayer, out hit_RandomToPlayer, directionRandomToPlayer.magnitude) && hit_RandomToPlayer.collider.tag == "Wall")
		{
			detectionList[detectionCount] = 1;
            avgTotal += detectionList[detectionCount];
		}

		else
		{
			detectionList[detectionCount] = 0;
            avgTotal += detectionList[detectionCount];
        }


        avgTotal -= detectionList[detectionCount2];
        //avgTotal = Mathf.Clamp(avgTotal, 0, detectionSize);
        Debug.Log("Count" + detectionCount + ": " + detectionList[detectionCount] + ", Count" + detectionCount2 + ": " + detectionList[detectionCount] + ", " + avgTotal);
        avgNumber = avgTotal / detectionSize;
        
        //avgNumber = Mathf.Clamp01((float)detectionList.Average());

    }

	void Occlusion()
	{
		if (wake == false)
		{
			lerpModifier -= 0.1f;
			source.volume += 0.001f;
		}

		if (lerpModifier <= 2.5f && wake == false)
		{
			lerpModifier = 2.5f;
			wake = true;
		}

		occlusionPercentage = Mathf.Lerp(occlusionPercentage, avgNumber, lerpModifier * Time.fixedDeltaTime);

		//Low Pass filter section
		filterPercentage = occlusionPercentage;

		//Volume section
		if(wake == true)
		{
		volPercentage = 0.5f + (1-occlusionPercentage) * 0.5f;
		source.volume = Mathf.Lerp(source.volume, volPercentage, 0.6f);
		}
	}

}
