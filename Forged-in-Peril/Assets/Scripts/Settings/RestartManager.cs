using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartManager : MonoBehaviour
{
    public static RestartManager Instance;
    public _SelectionManager selectionManager;
    [Tooltip("First Scene Name")]
    public string firstSceneName = "PreperationScene";

    private void Awake()
    {
        selectionManager = FindObjectOfType<_SelectionManager>();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PreperationScene")
        {
            selectionManager = FindObjectOfType<_SelectionManager>();
        }
    }

    public void RestartGame()
    {
        if (MapManager.Instance != null)
        {
            if (MapManager.Instance.gameObject != this.gameObject)
            {
                Destroy(MapManager.Instance.gameObject);
            }

            MapManager.Instance.ResetMap();
        }

        if (_GameManager.Instance != null)
        {
            if (_GameManager.Instance.gameObject != this.gameObject)
            {
                Destroy(_GameManager.Instance.gameObject);
            }
        }
        
        if (MusicManager.Instance != null && MusicManager.Instance.gameObject != this.gameObject)
        {
            Destroy(MusicManager.Instance.gameObject);
        }
        if (SFXManager.Instance != null && SFXManager.Instance.gameObject != this.gameObject)
        {
            Destroy(SFXManager.Instance.gameObject);
        }
        if (ESCMenuManager.Instance != null && ESCMenuManager.Instance.gameObject != this.gameObject)
        {
            Destroy(ESCMenuManager.Instance.gameObject);
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        selectionManager.selfDestroy();
        SceneManager.LoadScene(firstSceneName, LoadSceneMode.Single);
    }
}
