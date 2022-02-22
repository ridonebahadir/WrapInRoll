using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collection : MonoBehaviour
{
                     
    private Vector3 centerPos;                       

    public GameObject pointPrefab;                   
   

    public bool isCircular = false;                 
    public bool vertical = true;                 
    Vector3 pointPos;

    public static int totalPiece;
    int piece = 5;
   
    
    void Start()
    {
        //int level = PlayerPrefs.GetInt("Level",1);
        //for (int i = 1; i <= level; i++)
        //{
        //    Instantiate(10*i, 10*i, piece*i);
        //}

        Instantiate(20, 20 , 10);

    }
   void Instantiate(float radiusX,float radiusY,int piece)
    {
        centerPos = transform.position;
        for (int i = 0; i < piece; i++)
        {
         
            float pointNum = (i * 1.0f) / piece;

          
            float angle = pointNum * Mathf.PI * 2;

            float x = Mathf.Sin(angle) * radiusX;
            float y = Mathf.Cos(angle) * radiusY;

         
            if (vertical)
                pointPos = new Vector3(x, y) + centerPos;
            else if (!vertical)
            {
                pointPos = new Vector3(0, x, y) + centerPos;
            }

           
           Instantiate(pointPrefab, pointPos, Quaternion.identity,transform);
            totalPiece++;

        }

       
        if (isCircular)
        {
            radiusY = radiusX;
        }
    }
}
