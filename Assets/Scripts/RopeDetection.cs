using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeDetection : MonoBehaviour
{
    public GameObject otherPlane;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            otherPlane.SetActive(true);
            GameManager.wrap+=0.5f;
            gameObject.SetActive(false);
            GameManager.run = false;
          
        }
    }

}
