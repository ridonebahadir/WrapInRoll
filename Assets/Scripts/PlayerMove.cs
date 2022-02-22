using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Range(0, 30)]
    public float mousesensibility;
    //private bool touchStart = false;
    private Vector2 pointA;
    private Vector2 pointB;
    private bool touch;
    
    
    [Range(0, 1)]
    public float speed;


    [Header("Camera Settings")]
    public GameObject mainCamera;

    [Header("Finish")]
    public Transform finishPoint;
    public Animator anim;
    public GameObject hook;
    public dg_simpleCamFollow dg_SimpleCamFollow;
    public CameraFinal cameraFinal;
    public CameraMultiTarget cameraMulti;
    public PlayerMove playerMove;
    public Rigidbody rb;
    public static int ropeCount;
    public int seviyeChild;
    public Transform Seviye;
    public int countRope;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointA = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, transform.position.z));
            touch = true;


        }
        if (Input.GetMouseButton(0))
        {
            //touchStart = true;
            pointB = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, transform.position.z));
        }
        if (Input.GetMouseButtonUp(0))
        {
            touch = false;
        }

        countRope = ropeCount;
    }
    private void FixedUpdate()
    {
        if (touch)
        {
            Vector2 offset = pointB - pointA;
            Vector2 direction = Vector2.ClampMagnitude(offset, 1.0f);

            moveCharacter(direction);
           
        }

        if (transform.position.x > 2)
            transform.position = new Vector3(2,0, transform.position.z);
        if (transform.position.x < -2)
            transform.position = new Vector3(-2, 0, transform.position.z);
       
       
        //transform.position = Vector3.Lerp(new Vector3(transform.position.x,0,transform.position.z), new Vector3(transform.position.x,0,finalBorderpos.position.z), speed);
        if (transform.position.z < finishPoint.transform.position.z+8)
        {
           
            transform.position = new Vector3(transform.position.x, 0, transform.position.z + speed);
        }
        if (transform.position.z < finishPoint.transform.position.z)
        {
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, transform.position.z - 10);
        }
        else
        {
            rb.useGravity = true;
            //anim.applyRootMotion = true;
            anim.SetBool("Dive",true);
            //anim.enabled = false;
            //dg_SimpleCamFollow.enabled = true;
            Invoke("Late", 0.5f);
            //cameraMulti.enabled = true;
            hook.SetActive(true);
            Invoke("AnimClose", 1.06f);
            //playerMove.enabled = false;
            seviyeChild = (ropeCount / 3)-1;
            Seviye.GetChild(seviyeChild).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
            Debug.Log("ropeCount = " + ropeCount);
        }
      
       
    }
    void AnimClose()
    {
        anim.enabled = false;
        anim.SetBool("Falling", true);
        //anim.applyRootMotion = false;
    }
    void moveCharacter(Vector2 direction)
    {
        transform.Translate(direction * mousesensibility * Time.deltaTime);
    }

   void Late()
    {
        cameraFinal.enabled = true;
        //cameraMulti.enabled = true;
    }
}