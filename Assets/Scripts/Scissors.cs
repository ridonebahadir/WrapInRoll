using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scissors : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.tag=="RopePiece")
        {
            other.gameObject.SetActive(false);
            PlayerMove.ropeCount--;
            gameObject.SetActive(false);
           
        }
    }
}
