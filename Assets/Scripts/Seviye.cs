using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seviye : MonoBehaviour
{
    public Color[] color;
    //public GameObject seviyePiece;
    //public Material material;
    int y = 15;
    int z = 15;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            //GameObject obj= Instantiate(seviyePiece, transform.position, Quaternion.Euler(0,0,90), transform);
            //obj.transform.localPosition = new Vector3(0, -i * y, i * z);
          
            transform.GetChild(i).GetChild(4).GetComponent<TextMesh>().text = (i+1).ToString();
            //transform.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material.color = color[i];
        }
       
    }
}
