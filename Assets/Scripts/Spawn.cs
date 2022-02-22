using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    
    public GameObject player;
    [Header("Prefab")]
    public GameObject rope;
    public int ropePiece = 5;
    public GameObject door;
    public GameObject scissors;
    public GameObject obj;
    int a = 9;
    void Start()
    {
        for (int i = 1; i < 4; i++)
        {
            RopeInstantiate(75*i);
           
        }
       


    }

   void RopeInstantiate(int distance)
    {
        
        int side = Random.Range(0, 2) == 0 ? -2 : 2;
        for (int i = 0; i < ropePiece; i++)
        {
            Instantiate(rope, new Vector3(side,0, distance + i*3),Quaternion.identity,transform);
            
        }
      
        DoorInstantiate(distance+ropePiece*3+10);
    }
    void DoorInstantiate(int distance)
    {
        Instantiate(door, new Vector3(0, 0,distance), Quaternion.identity, transform);
        ScissorsInstantiate(distance+15);
    }
    void ScissorsInstantiate(int distance)
    {
        int side = Random.Range(0, 2) == 0 ? -2 : 2;
        Instantiate(scissors, new Vector3(side, 0, distance), Quaternion.identity, transform);
        ObjInstantiate(distance+15);
    }
    void ObjInstantiate(int distance)
    {
        int side = Random.Range(0, 2) == 0 ? -2 : 2;
        Instantiate(obj, new Vector3(side, 0, distance), Quaternion.identity, transform);
    }
}
