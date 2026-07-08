using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneButtons : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public    int variable =3;

    // Update is called once per frame

    public void ChangeScene()
    {
        Debug.Log(variable);
        SceneManager.LoadScene(variable);
    }
}
