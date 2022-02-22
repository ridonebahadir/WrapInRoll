using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class PlayerRoll : MonoBehaviour
{
    public bool run;
    bool jump;
    public Animator anim;
    public Animator cameraAnim;
    public GameObject tap;
    public GrapplingHook grapplingHook;
    public dg_simpleCamFollow dg;
    public CameraMultiTarget cmt;

    public CinemachineBrain cb;
    public Rigidbody rb;
    public float runSpeed = 0.3f;
    bool azalma;
    void Start()
    {
        
        run = true;
    }

    
    void Update()
    {
        if (run)
        {
            transform.position += Vector3.forward * runSpeed;
           
            if (transform.position.z>0)
            {
                //cmt.enabled = true;
               
                anim.SetBool("Dive", true);
                Invoke("LateAnim", 1.06f);
                Invoke("LateCam", 2f);
               



            }
           
        }
        if(jump)
        {
            transform.position += Vector3.down * 0.1f;
            //cmt.Yaw = Mathf.Lerp(0, 90, 3f * Time.deltaTime);
        }
        if (azalma)
        {
            if (runSpeed > 0)
            {
                runSpeed -= 0.003f;
            }
            else
            {
                run = false;
                runSpeed = 0;
            }
        }
    }
    void LateAnim()
    {

        jump = true;
        //run = false;
        azalma = true;
        rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
       
        //dg.enabled = false;




        //cameraAnim.enabled = true;
    }
    bool oneRun = false;
    void LateCam()
    {
      
        if (!oneRun)
        {
            grapplingHook.enabled = true;
            tap.SetActive(true);
            oneRun = true;
            return;
        }
       
       
    }

}
