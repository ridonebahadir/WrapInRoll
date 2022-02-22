using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class HumanRoll : MonoBehaviour
{
    public GameManager gm;
    public int starPuan;
  
    public ParticleContain BombParticle;
    private ParticleSystemRenderer particleSystemRenderer;
    

    public CoinManager coinManager;
    public Rigidbody rb;

    
    public Transform clinder;

    public Slider slider;
    public Transform rectTransform;

    [Header("Shake")]
    public float shakePower;
    public float shakeTime;
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Fruit")
        {
            
            int puan = other.GetComponent<Fruit>().puan;
            int z=(int)clinder.position.z;
            Vector3 pos = Camera.main.ViewportToWorldPoint(rectTransform.position);
           


            Color color= other.GetComponent<Fruit>().color;
            Fruit(other.gameObject, puan, Camera.main.WorldToViewportPoint(pos), color);
        }
        //if (other.gameObject.tag == "Star1")
        //{
        //    gm.Score += starPuan;
        //    slider.value += starPuan;
        //    gm.ScoreText.text = "Score = " + gm.Score.ToString();
        //    coinManager.AddCoins(other.transform.position, 7, new Vector3(0, 0, clinder.position.z),1);
        //    Instantiate(BombParticle[1].particle, other.transform.position, Quaternion.identity);
        //    other.gameObject.SetActive(false);
        //}
        //if (other.gameObject.tag == "Star2")
        //{
        //    gm.Score += starPuan;
        //    slider.value += starPuan;
        //    gm.ScoreText.text = "Score = " + gm.Score.ToString();
        //    coinManager.AddCoins(other.transform.position, 7, new Vector3(0, 0, clinder.position.z-30),2);
        //    Instantiate(BombParticle[2].particle, other.transform.position, Quaternion.identity);
        //    other.gameObject.SetActive(false);
        //}
        if (other.tag=="Bomb")
        {
            gm.Score += (BombArea.bombItemCount * starPuan);
            slider.value += (BombArea.bombItemCount * starPuan);
            Debug.Log("sadasd = "+BombArea.bombItemCount* starPuan);
            gm.ScoreText.text = "Score = " + gm.Score.ToString();
            CameraShake.Instance.ShakeCamera(shakePower,shakeTime);
           
        }
    }

  

    private void FixedUpdate()
    {
        transform.position = new Vector3(0, transform.position.y, transform.position.z);
    }


    void Fruit(GameObject other,int puan,Vector3 target,Color color)
    {
        gm.Score += puan;
        slider.value += puan;
        gm.ScoreText.text = "Score = " + gm.Score.ToString();
        coinManager.AddCoins(other.transform.position, 7, target, 0,color);
        //particleSystemRenderer.material.color = color;
        //Instantiate(BombParticle.particle, other.transform.position, Quaternion.identity);
        other.gameObject.SetActive(false);
    }
   
}





[System.Serializable]//makes sure this shows up in the inspector
public class ParticleContain
{
   
    public string name;//your name variable to edit
    public GameObject particle;//place texture in here
}