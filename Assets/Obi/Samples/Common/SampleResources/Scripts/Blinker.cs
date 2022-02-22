using System;
using UnityEngine;
using Cinemachine;
public class Blinker : MonoBehaviour {

	//public GameManager gm;
	//public GameObject player;
	//public Animator anim;
	public Color highlightColor;
	public GameManager gm;
 	private Renderer rend;
	private Color original;
	public Animator PlayerAnim;
	public CinemachineTargetGroup ct;
	public GrapplingHook grapplingHook;
	//public CoinManager coinManager;
	void Awake(){
		rend = GetComponent<Renderer>();
		original = rend.material.color;
		
	}
	bool azalma;
	float weight = 1;
    private void Update()
    {
        if (azalma)
        {
            if (weight>0)
            {
				weight -= 0.001f;

			}
            else
            {
				azalma = false;
				weight = 0;
            }
        }
		ct.m_Targets[0].weight = weight;
	}
    public void Blink(){
		rend.material.color = highlightColor;
		//anim.enabled = false;
		//player.SetActive(true);
		//PlayerAnim.SetBool("Finish",true);

		PlayerAnim.enabled = false;
		//azalma = true;
		
		ct.m_Targets[0].radius = 0;
		ct.m_Targets[1].radius = 20;
		ct.m_Targets[2].target = null;

		//dg_simpleCamFollow.changeTarget = false;
		//Invoke("RopeTouch", 8);
		//coinManager.TransformDetect();

	}
	
  void RopeTouch()
    {
        if (GameManager.wrap<1)
        {
			gm.Fail();
        }
    }


	void LateUpdate(){
		rend.material.color += (original - rend.material.color)*Time.deltaTime*5;
	}



}
