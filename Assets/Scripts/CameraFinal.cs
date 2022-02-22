using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFinal : MonoBehaviour
{
    public GameObject target;
    public GameObject clinder;
    public PlayerMove playerMove;
    public Transform seviye;
    public Quaternion _lookRotation;
    void Start()
    {

        target = seviye.GetChild(playerMove.seviyeChild).GetChild(1).gameObject;
        clinder = seviye.GetChild(playerMove.seviyeChild).GetChild(0).gameObject;
        StartCoroutine(MoveObj());
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.Lerp(transform.position, target.transform.position, 0.1f);
        //transform.LookAt(clinder.transform);
    }



    IEnumerator MoveObj()
    {
        float time = 2f; 
        float passTime = 0f;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.transform.position;

        


        while (passTime < time) 
        {
            passTime += Time.deltaTime;
            transform.position = Vector3.Lerp(currentPos, targetPos, passTime / time);
            transform.rotation = Quaternion.Euler(0, Mathf.Lerp(0,90,passTime/time), 0);
            yield return null;
        }

       
        transform.position = targetPos;
    }
}
