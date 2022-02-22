using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public List<string> skill;
    private void Awake()
    {
        int random = Random.Range(0, skill.Count);
        transform.GetChild(0).gameObject.tag = skill[random];
       

        int randomAgain = Random.Range(0, skill.Count);
        transform.GetChild(1).gameObject.tag = skill[randomAgain];
    }
}
