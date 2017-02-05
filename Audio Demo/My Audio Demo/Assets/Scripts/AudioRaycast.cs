﻿using UnityEngine;
using System.Collections;

public class AudioRaycast : MonoBehaviour {

	private Vector3 directionToPlayer, directionCenter;
	private Vector3 edgeL, edgeR, directionToEdgeL, directionToEdgeR, directionLToPlayer, directionRToPlayer;
	private Vector3 ScannerL, ScannerR;
	private GameObject thePlayer;
	private AudioSource source;
	private AudioLowPassFilter LPfilter;
	private float rayDistance;
    private float scanAngleL, scanAngleR;
	private float edgeLAngle, edgeRAngle, maxAngle, angleL, angleR, LToPAngle, RToPAngle;
	public float staticMaxAngle = 25f;
	private int timerWakeUp;
	[SerializeField][Range(0,1)]private float filterPercentage = 0f;
	private bool edgeLFound = false, edgeRFound = false, pathClear = true, wake = false, transitionDone = false;
	private Collider hitCollider;
	public AnimationCurve filterCurve, Curve2;

	void Start () 
	{
		thePlayer = GameObject.FindGameObjectWithTag("Player");
		source = GetComponent<AudioSource>();
		source.mute = true;
		LPfilter = GetComponent<AudioLowPassFilter>();
		timerWakeUp = 0;
		//InvokeRepeating("Occlusion", 0f, 1.5f);
	}
	
	void FixedUpdate () 
	{
		directionCenter = thePlayer.transform.position + new Vector3(0f, 0.6f, 0f) - transform.position;
		rayDistance = directionCenter.magnitude;
		directionToPlayer = directionCenter.normalized;

		Ray rayMain = new Ray(transform.position, directionCenter);
		RaycastHit hit;
		Debug.DrawRay(transform.position, directionCenter, Color.red, 0, false);

		Scan();
		Occlusion();
		WakeUp();
		Debug.DrawRay(transform.position, ScannerL, Color.green, 0, false);
		Debug.DrawRay(transform.position, directionToEdgeL, Color.blue, 0, false);
		Debug.DrawRay(transform.position, ScannerR, Color.green, 0, false);
		Debug.DrawRay(transform.position, directionToEdgeR, Color.blue, 0, false);
		//Debug.DrawRay(edgeL, Vector3.up, Color.yellow, 0, false);

		LPfilter.cutoffFrequency = 22000f - (21500f * filterPercentage);
	}

	void Scan()
	{
		Ray rayMain = new Ray(transform.position, directionCenter);
		RaycastHit hit;
		if (Physics.Raycast(rayMain, out hit, rayDistance) && hit.collider.tag =="Wall")
		{
			if(hitCollider != hit.collider || pathClear == true)
			{
				edgeLFound = false;
				edgeRFound = false;
				pathClear = false;
				edgeLAngle = 0f;
				edgeRAngle = 0f;
				scanAngleL = 0.1f;
				scanAngleR = 0.1f;
				ScannerL = rayMain.direction * hit.distance;
				ScannerR = rayMain.direction * hit.distance;
				hitCollider = hit.collider;
			}

			else
			{
				Ray clearPathL = new Ray(transform.position, ScannerL);
				RaycastHit scanLHit;
				Ray clearPathR = new Ray(transform.position, ScannerR);
				RaycastHit scanRHit;

				if(edgeLFound == false)
				{
					int layerMask = 1 << 8;
					bool hitColl = Physics.Raycast(clearPathL, out scanLHit, ScannerL.magnitude * 1.3f, layerMask);
					if(!hitColl)
					{
						edgeLFound = true;
						//Debug.Log("edge Left found");
					}

					else
					{
						ScannerL.y = rayMain.direction.y * ScannerL.magnitude;
						ScannerL = ScannerL.normalized * scanLHit.distance * 1.05f;
						ScannerL = Quaternion.Euler(new Vector3(0f, scanAngleL)) * ScannerL;
						edgeLAngle = edgeLAngle + scanAngleL;
						scanAngleL = scanAngleL <= 2f ? scanAngleL + 0.1f : 2f;
					}
				}

				if(edgeRFound == false)
				{
					int layerMask = 1 << 8;
					bool hitColl = Physics.Raycast(clearPathR, out scanRHit, ScannerR.magnitude * 1.3f, layerMask);
					if(!hitColl)
					{
						edgeRFound = true;
						//Debug.Log("edge Right found");
					}

					else
					{
						ScannerR.y = rayMain.direction.y * ScannerR.magnitude;
						ScannerR = ScannerR.normalized * scanRHit.distance * 1.05f;
						ScannerR = Quaternion.Euler(new Vector3(0f, -scanAngleR)) * ScannerR;
						edgeRAngle = edgeRAngle + scanAngleR;
						scanAngleR = scanAngleR <= 2f ? scanAngleR + 0.1f : 2f;
					}
				}

				if(edgeLFound == true)
				{
					ScannerL.y = rayMain.direction.y * ScannerL.magnitude;
					edgeL = transform.position + ScannerL;
					directionLToPlayer = thePlayer.transform.position - edgeL;
					Debug.DrawRay(edgeL, directionLToPlayer, Color.yellow, 0, false);
					LToPAngle = Vector3.Angle(-ScannerL, directionLToPlayer);
				}

				if(edgeRFound == true)
				{
					ScannerR.y = rayMain.direction.y * ScannerR.magnitude;
					edgeR = transform.position + ScannerR;
					directionRToPlayer = thePlayer.transform.position - edgeR;
					Debug.DrawRay(edgeR, directionRToPlayer, Color.yellow, 0, false);
					RToPAngle = Vector3.Angle(-ScannerR, directionRToPlayer);
				}	
					
			}

		}
			
		else if (pathClear == false)
		{
			pathClear = true;
			//Debug.Log("set true");
		}
			
	}

	void Occlusion()
	{
		if (pathClear == false)
		{
			if (edgeLFound == true && edgeRFound == true)
			{
				if (edgeLAngle <= 180f && edgeRAngle >= 180f)
				{
					angleL = Vector3.Angle(ScannerL, directionCenter);
					//float x = Mathf.Clamp01(Curve2.Evaluate((LToPAngle/((180f-angleL)/2f))));
					//Debug.Log(x);
					if (angleL <= staticMaxAngle)
					{
						float x = occlusionModifier((LToPAngle/((180f-angleL)/2f)));
						filterPercentage = Mathf.Clamp01(filterCurve.Evaluate(angleL/staticMaxAngle)) * x;
						//Debug.Log(x);
					}

					else
					{
						filterPercentage = 1f;
					}

				}

				if (edgeLAngle >= 180f && edgeRAngle <= 180f)
				{
					angleR = Vector3.Angle(ScannerR, directionCenter);
					if (angleR <= staticMaxAngle)
					{
						filterPercentage = Mathf.Clamp01(filterCurve.Evaluate(angleR/staticMaxAngle));
					}

					else
					{
						filterPercentage = 1f;
					}

				}

				if (edgeLAngle < 180f && edgeRAngle < 180f)
				{
					maxAngle = (edgeLAngle + edgeRAngle)/2;
					angleL = Vector3.Angle(ScannerL, directionCenter);
					angleR = Vector3.Angle(ScannerR, directionCenter);

					if (angleL < maxAngle)
					{
						if (filterPercentage > Mathf.Clamp01(filterCurve.Evaluate(angleL/staticMaxAngle)) && transitionDone == false)
						{
							filterPercentage = filterPercentage - 0.001f;
						}

						else
						{
							transitionDone = true;
							filterPercentage = Mathf.Clamp01(filterCurve.Evaluate(angleL/staticMaxAngle));
						}
							
					}

					else
					{
						if (filterPercentage > Mathf.Clamp01(filterCurve.Evaluate(angleR/staticMaxAngle)) && transitionDone == false)
						{
							filterPercentage = filterPercentage - 0.001f;
						}

						else
						{
							transitionDone = true;
							filterPercentage = Mathf.Clamp01(filterCurve.Evaluate(angleR/staticMaxAngle));
						}
					}
				}
			}

			else if (edgeLFound == true)
			{
				angleL = Vector3.Angle(ScannerL, directionCenter);
				if (angleL <= staticMaxAngle)
				{
					filterPercentage = Mathf.Clamp01(filterCurve.Evaluate(angleL/staticMaxAngle));
				}

				else
				{
					filterPercentage = 1f;
				}
			}

			else if (edgeRFound == true)
			{
				angleR = Vector3.Angle(ScannerR, directionCenter);
				if (angleR <= staticMaxAngle)
				{
					filterPercentage = Mathf.Clamp01(filterCurve.Evaluate(angleR/staticMaxAngle));
				}

				else
				{
					filterPercentage = 1f;
				}
			}

			else if (filterPercentage == 0f)
			{
				filterPercentage = 0f;
			}

			else
			{
				filterPercentage = 1f;
			}
				
		}

		else
		{
			filterPercentage = 0f;
			transitionDone = false;
		}
	}

	void WakeUp()
	{
		if (wake == false)
		{
			if (filterPercentage != 0f || timerWakeUp >= 60)
			{
				source.mute = false;
				wake = true;
			}

			else
			{
				timerWakeUp = timerWakeUp + 1;
			}
		}
	}

	float occlusionModifier(float a)
	{
		if (a > 0 && a < 1)
		{
			return (1-a);
		}

		else
		{
			return 1f;
		}

	}
		
}