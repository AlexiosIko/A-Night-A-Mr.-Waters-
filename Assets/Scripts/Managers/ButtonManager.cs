using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ButtonManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Main");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
