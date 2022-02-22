using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionRoll : MonoBehaviour
{
                     
    private Vector3 centerPos;                       

    public List<GameObject> pointPrefab;                   
    public GameObject bomb;
    public HumanRoll humanRoll;

    public bool isCircular = false;                 
    public bool vertical = true;                 
    Vector3 pointPos;

    public int totalPiece;
    public int totalScore;
    int piece = 5;
    public Slider slider;
    
    void Start()
    {
        
        int circlePiece = PlayerPrefs.GetInt("CirclePiece", 1);

       
        for (int i = 1; i <= circlePiece; i++)
            {
                int puan = pointPrefab[i - 1].gameObject.transform.GetChild(0).GetComponent<Fruit>().puan;
                Instantiate(10 * i, 10 * i, piece * i, pointPrefab[i - 1], puan);
            }
        
            
        
        
        

        //Instantiate(20, 20 , 10);

    }
   void Instantiate(float radiusX,float radiusY,int piece,GameObject obj,int puan)
    {
        int random = Random.Range(0,piece);
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

            if (i==random)
            {
                Instantiate(bomb, pointPos, Quaternion.identity, transform.parent);
            }
            else
            {
                Instantiate(obj, pointPos, Quaternion.identity, transform.parent);
            }
         
            totalPiece++;
           
        }
        totalScore = totalPiece * puan;
      
        slider.maxValue = totalScore;

        if (isCircular)
        {
            radiusY = radiusX;
        }
    }
}
