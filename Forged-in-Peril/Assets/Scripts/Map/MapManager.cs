using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public List<MapNode> mapNodes = new List<MapNode>();
    public int currentNodeIndex;      
    public int currentBattleAreaIndex;  
    public int campIndex = -1;
    public int fightIndex = -1;

    public Dictionary<int, bool> campNode = new Dictionary<int, bool>();
    public Dictionary<int, bool> fightNode = new Dictionary<int, bool>();
    public Dictionary<int, bool> discoveredNodes = new Dictionary<int, bool>();

    public Camera mainCamera;
    public float cameraMoveSpeed = 2f;
    public float cameraZoomDuration = 1f;
    public float cameraDefaultSize = 5f;
    public float cameraZoomedSize = 1f;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip selectClip;

    public CameraController cameraController;
    public _GameManager gameManager;

    private bool isMoving = false;

    public int lastCheckpointIndex = 0;

    public int startNodeIndex = 0;
    public int finishNodeIndex = 12;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        if (scene.name == "MapScene")
        {
            AssignMainCamera();
            
            mapNodes.Clear();
            MapNode[] nodesInScene = FindObjectsOfType<MapNode>();
            foreach (MapNode node in nodesInScene) mapNodes.Add(node);
            loadNodes();

            foreach (MapNode node in mapNodes)
            {
                foreach (int nextIndex in node.nextNodeIndices)
                {
                    MapNode targetNode = mapNodes.Find(n => n.nodeIndex == nextIndex);
                    if (targetNode != null)
                    {
                        node.DrawPath(targetNode);
                    }
                }
            }

            discoveredNodes[0] = true;
            discoveredNodes[1] = true;
            discoveredNodes[2] = true;
            discoveredNodes[finishNodeIndex] = true;

            ApplyDiscoveredNodes();

            ToggleNodeInteraction(false);

            if (campIndex != -1)
            {
                MapNode campN = mapNodes.Find(n => n.nodeIndex == campIndex);
                if (campN != null)
                {
                    campN.treasure = false;
                    campN.camp = true;
                    campN.campSave = true;

                }
                campIndex = -1;
                saveNodes();
            }
            if (fightIndex != -1)
            {
                MapNode fightN = mapNodes.Find(n => n.nodeIndex == fightIndex);
                if (fightN != null) fightN.isFight = true;
                saveNodes();
            }

            if (_GameManager.Instance != null && _GameManager.Instance.diedAndRestarted)
            {
                currentNodeIndex = startNodeIndex;
                _GameManager.Instance.diedAndRestarted = false;
            }

            if (!discoveredNodes.ContainsKey(startNodeIndex) || !discoveredNodes[startNodeIndex])
            {
                DiscoverTwoSteps(startNodeIndex);
            }

            MapNode currentNode = mapNodes.Find(n => n.nodeIndex == currentNodeIndex);
            if (currentNode != null)
            {
                FocusCameraOnNode(currentNode.transform.position);
                HighlightCurrentNode();
                currentNode.SpawnCharacterIcon();

                if (currentNode.nextNodeIndices.Count > 0)
                {
                    foreach (int nextNodeIndex in currentNode.nextNodeIndices)
                    {
                        MapNode nextNode = MapManager.Instance.mapNodes.Find(n => n.nodeIndex == nextNodeIndex);

                        if (nextNode != null 
                            && nextNode.nodeButton != null 
                            && !nextNode.isFight)
                        {
                            nextNode.nodeButton.interactable = true;
                        }
                    }
                }

                foreach (MapNode possibleCheckpoint in mapNodes)
                {
                    if (possibleCheckpoint.isCheckpoint
                        && possibleCheckpoint.isDiscovered
                        && possibleCheckpoint.isFight
                        && possibleCheckpoint.nodeButton != null)
                    {
                        possibleCheckpoint.nodeButton.interactable = true;
                    }
                }

              
                if (currentNode.isDeadEnd)
                {
                    MapNode lastCheckpointNode = mapNodes.Find(n => n.nodeIndex == lastCheckpointIndex);
                    if (lastCheckpointNode != null)
                    {
                        lastCheckpointNode.HighlightCurrentNode();
                    }
                }
            }
        }
        else if (scene.name == "BattleScene")
        {
            BattleAreaManager bam = FindObjectOfType<BattleAreaManager>();
            if (bam != null)
            {
                bam.SelectedBattleAreaIndex = currentBattleAreaIndex;
            }
        }
    }

    public void MoveToNode(int targetNodeIndex, int targetBattleAreaIndex, int battleMap, System.Action onComplete = null)
    {
        if (isMoving) return;
        isMoving = true;

        //ToggleNodeInteraction(false);

        if (sfxSource != null && selectClip != null)
        {
            sfxSource.PlayOneShot(selectClip);
        }

        MapNode currentNode = mapNodes.Find(n => n.nodeIndex == currentNodeIndex);
        MapNode targetNode = mapNodes.Find(n => n.nodeIndex == targetNodeIndex);
        if (currentNode != null && targetNode != null)
        {
            currentNode.StopHighlighting();
            currentNode.RemoveCharacterIcon();

            currentNode.MoveCharacterToNextNode(targetNode, () =>
            {
                currentNodeIndex = targetNodeIndex;
                currentBattleAreaIndex = targetBattleAreaIndex;

                DiscoverTwoSteps(targetNodeIndex);

                if (cameraController != null) cameraController.DisableCameraControl();

                CameraZoomAndLoadScene(targetNode.transform.position, () =>
                {
                    isMoving = false;
                    HighlightCurrentNode();
                    onComplete?.Invoke();
                });
            });
        }
        else
        {
            isMoving = false;
        }
    }

    private void ToggleNodeInteraction(bool enable)
    {
        foreach (MapNode node in mapNodes)
        {
            if (node.nodeButton != null)
            {
                node.nodeButton.interactable = enable;
            }
        }
    }

    public void MoveToCheckpoint(int targetNodeIndex, System.Action onComplete = null)
    {
        if (isMoving) return;
        isMoving = true;
        //ToggleNodeInteraction(false);

        MapNode currentNode = mapNodes.Find(n => n.nodeIndex == currentNodeIndex);
        MapNode checkpointNode = mapNodes.Find(n => n.nodeIndex == targetNodeIndex);

        if (checkpointNode == null)
        {
            isMoving = false;
            return;
        }
        if (currentNode != null)
        {
            currentNode.StopHighlighting();
            currentNode.RemoveCharacterIcon();
            currentNodeIndex = targetNodeIndex;
            FocusCameraOnNode(checkpointNode.transform.position);
            checkpointNode.SpawnCharacterIcon();
            isMoving = false;
            HighlightCurrentNode();

                if (checkpointNode.nextNodeIndices.Count > 0)
                {
                    foreach (int nextNodeIndex in checkpointNode.nextNodeIndices)
                    {
                        MapNode nextNode = MapManager.Instance.mapNodes.Find(n => n.nodeIndex == nextNodeIndex);

                        if (nextNode != null && nextNode.nodeButton != null && !nextNode.isFight && !nextNode.isCheckpoint)
                        {
                            nextNode.nodeButton.interactable = true;
                        }
                    }
                }
        }
    }

    private void CameraZoomAndLoadScene(Vector3 nodePosition, System.Action onComplete)
    {
        mainCamera.transform.DOMove(new Vector3(nodePosition.x, nodePosition.y, mainCamera.transform.position.z), cameraMoveSpeed)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                mainCamera.DOOrthoSize(cameraZoomedSize, cameraZoomDuration).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        SceneManager.LoadScene("BattleScene");
                        if (cameraController != null) cameraController.EnableCameraControl();
                        onComplete?.Invoke();
                    });
            });
    }

    public void FocusCameraOnNode(Vector3 nodePosition)
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(nodePosition.x, nodePosition.y, mainCamera.transform.position.z);
        }
    }

    public void ResetMap()
    {
        currentNodeIndex = startNodeIndex;
        _GameManager.Instance.enemydiff = 0;
    }

    private void HighlightCurrentNode()
    {
        MapNode currentNode = mapNodes.Find(n => n.nodeIndex == currentNodeIndex);
        if (currentNode != null)
        {
            currentNode.HighlightCurrentNode();
        }
    }

    public void DiscoverTwoSteps(int index)
    {
        DiscoverNode(index);

        MapNode current = mapNodes.Find(n => n.nodeIndex == index);
        if (current == null) return;

        foreach (int firstStepIndex in current.nextNodeIndices)
        {
            DiscoverNode(firstStepIndex);

            MapNode firstStepNode = mapNodes.Find(n => n.nodeIndex == firstStepIndex);
            if (firstStepNode == null) continue;

            foreach (int secondStepIndex in firstStepNode.nextNodeIndices)
            {
                DiscoverNode(secondStepIndex);
            }
        }
        saveNodes();
    }

    private void DiscoverNode(int idx)
    {
        if (!discoveredNodes.ContainsKey(idx))
        {
            discoveredNodes[idx] = true;
        }
        MapNode node = mapNodes.Find(n => n.nodeIndex == idx);
        if (node != null)
        {
            node.isDiscovered = true;
            node.RefreshVisibility();
        }
    }
    private void saveNodes()
    {
        foreach (MapNode mn in mapNodes)
        {
            discoveredNodes[mn.nodeIndex] = mn.isDiscovered;

            if (mn.campSave) campNode[mn.nodeIndex] = true;
            if (mn.isFight) fightNode[mn.nodeIndex] = true;
        }
    }

    private void loadNodes()
    {
        foreach (MapNode mn in mapNodes)
        {
            mn.isDiscovered = false;
            mn.RefreshVisibility();
        }

        foreach (var kvp in discoveredNodes)
        {
            MapNode dn = mapNodes.Find(n => n.nodeIndex == kvp.Key);
            if (dn != null)
            {
                dn.isDiscovered = kvp.Value;
                dn.RefreshVisibility();
            }
        }

        foreach (var c in campNode)
        {
            MapNode cn = mapNodes.Find(n => n.nodeIndex == c.Key);
            if (cn != null && c.Value)
            {
                cn.treasure = false;
                cn.camp = true;
                cn.campSave = true;
                cn.ChestIcon.SetActive(false);
                cn.CampIcon.SetActive(true);
            }
        }
        foreach (var f in fightNode)
        {
            MapNode fn = mapNodes.Find(n => n.nodeIndex == f.Key);
            if (fn != null && f.Value)
            {
                fn.isFight = true;
            }
        }
    }
    private void ApplyDiscoveredNodes()
    {
        foreach (var kvp in discoveredNodes)
        {
            MapNode node = mapNodes.Find(n => n.nodeIndex == kvp.Key);
            if (node != null)
            {
                node.isDiscovered = kvp.Value;
                node.RefreshVisibility();
            }
        }
        saveNodes();
    }
    private void AssignMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographicSize = cameraDefaultSize;
                cameraController = mainCamera.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    cameraController.isCameraControlEnabled = true;
                }
            }
        }
    }
}
