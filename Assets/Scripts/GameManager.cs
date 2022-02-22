using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;

public class GameManager : MonoBehaviour
{

    private CollectionRoll collectionRoll;

    public static float wrap;
    public Text wrapText;
    public Text ResultText;
    public float Score;
    public Text ScoreText;
    public Text levelText;
    public GameObject finalPanel;
    public GameObject SuccessButton;
    public static bool run = false;
    public int level = 1;
    public int circlePiece = 1;
    public GameObject dolanma;
    public GameObject[] star;
    public Text perfect;

    [Header("START")]
    public Button startButton;
    public PlayerRoll playerRoll;
   
    public Animator human;
    public GameObject jointHere;
    public CinemachineTargetGroup cinemachineTargetGroup;

    [Header("FINAL")]
    public GameObject confetti;
    public Transform Clinder;
    private void Awake()
    {
        level = PlayerPrefs.GetInt("Level", 1);
        Clinder = dolanma.transform.GetChild(level - 1).GetChild(0).gameObject.transform;
        cinemachineTargetGroup.m_Targets[1].target = dolanma.transform.GetChild(level - 1).GetChild(0).GetChild(2);
        cinemachineTargetGroup.m_Targets[2].target = dolanma.transform.GetChild(level - 1).GetChild(1);
        dolanma.transform.GetChild(level-1).gameObject.SetActive(true);
        collectionRoll = dolanma.transform.GetChild(level - 1).GetChild(0).gameObject.GetComponent<CollectionRoll>();
        collectionRoll.enabled=true;

    }
    private void Start()
    {
        if (oldStart)
        {
            StartButton();
        }
        circlePiece = PlayerPrefs.GetInt("CirclePiece", 1);
        levelText.text ="Level = " +level.ToString();
        ScoreText.text ="Score = " +Score.ToString();
        wrapText.text = "Wrap = " + wrap.ToString();
    }
    private void Update()
    {
        
       

        if (Score>((collectionRoll.totalScore*50)/100))
        {
            Success();
           
        }
       
        if (wrap%1==0)
        {
            if (!run)
            {
                wrapText.text ="Wrap = "+ wrap.ToString();
                //Score *= wrap;
               
                run = true;
                return;
            }
            
        }
       
    }
    public void Restart()
    {
        BombArea.bombItemCount = 0;
        wrap = 0;
        //dg_simpleCamFollow.changeTarget = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
      
    }
    public void RestartAll()
    {
        PlayerPrefs.DeleteAll();
        Restart();
    }


   public  void Fail()
    {
        string text = "Restart Game";
        StartCoroutine(Final(text, 1f));
    }

    public void Success()
    {

      
        SuccessButton.SetActive(true);
        ScoreText.text = "Score = " + Score.ToString();
    }
    bool switchh = true;
    public void SuccessButtonn()
    {
        if (switchh)
        {
            string text = "Next Level";
            //Score *= wrap;
            PlayerPrefs.SetInt("Level", level + 1);

            if (level % 2 == 0)
            {
                PlayerPrefs.SetInt("CirclePiece", circlePiece + 1);
            }
            Instantiate(confetti, Clinder.position - new Vector3(0, 120, 0), Quaternion.identity);
            StartCoroutine(Final(text, 0f));
            switchh = false;
            return;
        }
        else
        {
            Restart();
        }
        
    }
  

    IEnumerator Final(string result, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        finalPanel.SetActive(true);
        ResultText.text = result;


        if (Score > ((collectionRoll.totalScore * 50) / 100))
        {
            star[0].SetActive(true);
            perfect.text = "Keep Going";
        }
        if (Score > ((collectionRoll.totalScore * 75) / 100))
        {
            star[0].SetActive(true);
            star[1].SetActive(true);
            perfect.text = "Well Done";
        }
        if (Score > ((collectionRoll.totalScore * 90) / 100))
        {
            star[0].SetActive(true);
            star[1].SetActive(true);
            star[2].SetActive(true);
            perfect.text = "Good Job";

        }


    }
    public static bool oldStart;
    public void StartButton()
    {
        playerRoll.enabled = true;
       
        human.SetBool("Backwards", true);
        startButton.gameObject.SetActive(false);
        jointHere.SetActive(true);
        oldStart = true;

       
    }
   
}
