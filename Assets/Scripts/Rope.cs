using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "RopePiece")
        {
            Player.RopeStatic = other.gameObject;
            other.transform.GetChild(0).gameObject.SetActive(true);
            PlayerMove.ropeCount++;
            gameObject.SetActive(false);
           
        }
        if (other.gameObject.tag=="Player")
        {
            other.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
            PlayerMove.ropeCount++;
            gameObject.SetActive(false);
        }
    }
}
