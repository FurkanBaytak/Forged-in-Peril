using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Collections.Generic;


public class ExitButton : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void OnRestartButtonPressed()
    {
        RestartManager.Instance.RestartGame();
    }


}
