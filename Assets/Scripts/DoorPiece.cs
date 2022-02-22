using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPiece : MonoBehaviour
{
    public int count;
    public TextMesh CountText;
    public Material[] material;
    public Material ropePiece;
    private void Start()
    {
        if (gameObject.tag== "DecreaseRope")
        {
            count = Random.Range(-5, 0);
            CountText.gameObject.SetActive(true);
            CountText.text = count.ToString();
        }
        if (gameObject.tag== "IncreaseRope")
        {
            count = Random.Range(0, 5);
            CountText.gameObject.SetActive(true);
            CountText.text = count.ToString();
        }
        if (gameObject.tag=="ChangeMaterial")
        {
            int random = Random.Range(0, material.Length);
            ropePiece = material[random];
        }
     

    }
}
