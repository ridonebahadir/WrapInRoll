using FIMSpace.FSpine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public FSpineAnimator fspineanimator; 
    public GameObject Bomb;
    public GameManager gm;
    public GameObject Rope;
    public static GameObject RopeStatic;
    public GrapplingHook grapplingHook;
    //public PlayerMove playerMove;
    //public Material ropePieceMaterial;
    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
         Rope = RopeStatic;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="Star")
        {
            gm.Score += 1;
            gm.ScoreText.text = "Score = " + gm.Score.ToString();
            Instantiate(Bomb, other.transform.position, Quaternion.identity);
            other.gameObject.SetActive(false);
        }
        if (other.gameObject.tag == "IncreaseRope")
        {
            other.gameObject.GetComponent<BoxCollider>().enabled = false;
            int a = other.gameObject.GetComponent<DoorPiece>().count;
            PlayerMove.ropeCount += a;
            for (int i = 0; i < a; i++)
            {
                RopeStatic.transform.GetChild(0).gameObject.SetActive(true);
                RopeStatic = RopeStatic.transform.GetChild(0).gameObject;
               
            }
           
        }
        if (other.gameObject.tag == "DecreaseRope")
        {

            other.gameObject.GetComponent<BoxCollider>().enabled = false;
            int a =Mathf.Abs(other.gameObject.GetComponent<DoorPiece>().count);
            PlayerMove.ropeCount += other.gameObject.GetComponent<DoorPiece>().count;
            if (PlayerMove.ropeCount<0)
            {
                PlayerMove.ropeCount = 0;
            }

            for (int i = 0; i <a; i++)
            {
                if (PlayerMove.ropeCount > 1)
                {
                    RopeStatic.gameObject.SetActive(false);
                    RopeStatic = RopeStatic.transform.parent.gameObject;
                }
               
            }
           
        }
        if (other.gameObject.tag== "ChangeMaterial")
        {
            int a = fspineanimator.SpineTransforms.Count;
            for (int i = 1; i < a; i++)
            {
                fspineanimator.SpineTransforms[i].GetComponent<MeshRenderer>().material= other.gameObject.GetComponent<DoorPiece>().ropePiece;
            }
          
            //ropePieceMaterial = other.gameObject.GetComponent<DoorPiece>().ropePiece;
        }
        //if (other.gameObject.tag == "Rope")
        //{
        //    other.gameObject.SetActive(false);
        //    Rope.transform.GetChild(0).gameObject.SetActive(true);
        //    Rope = Rope.transform.GetChild(0).gameObject;
        //    grapplingHook.time+=0.1f;
        //    PlayerMove.ropeCount++;

        //}
        //if (other.gameObject.tag == "Scissors")
        //{
        //    other.gameObject.SetActive(false);
        //    Rope.gameObject.SetActive(false);
        //    Rope = Rope.transform.parent.gameObject;

        //}

    }
}
