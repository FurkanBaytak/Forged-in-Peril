using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    public int nodeIndex;
    public List<int> nextNodeIndices = new List<int>();
    public int battleAreaIndex;

    public Button nodeButton;
    public GameObject characterIconPrefab;
    private GameObject characterIcon;

    public bool treasure = false;
    public bool camp = false;
    public _weapon[] treasureWeapon;
    public _WeaponMaterial[] treasureWeaponMaterial;

    public bool campSave = false;
    public bool isFight = false;
    public bool isCheckpoint = false;
    public bool isDeadEnd = false;
    public bool isDiscovered = false;

    public GameObject ChestIcon;
    public GameObject CampIcon;

    public int nodeDiff = 0;
    public bool enemydiffUp = false;
    
    private struct Connection
    {
        public MapNode targetNode;
        public LineRenderer lineRenderer;
    }

    private List<Connection> connections = new List<Connection>();
    private Gradient gradient;

    private Tween buttonTween;
    private bool isMoving = false;

    private void Start()
    {
        if (nodeButton == null)
            nodeButton = GetComponent<Button>();

        if (nodeButton != null)
        {
            nodeButton.onClick.AddListener(OnNodeClicked);
        }

        if (MapManager.Instance.currentNodeIndex == nodeIndex)
        {
            HighlightCurrentNode();
            SpawnCharacterIcon();
        }


        if (nextNodeIndices.Count == 0)
        {
            isDeadEnd = true;
        }

        gradient = new Gradient();
        ResetGradient();
        RefreshVisibility();
    }

    private void OnNodeClicked()
    {
        if (isMoving) return;

        int currentNodeIndex = MapManager.Instance.currentNodeIndex;
        if (currentNodeIndex == nodeIndex) return;

        MapNode currentNode = MapManager.Instance.mapNodes.Find(n => n.nodeIndex == currentNodeIndex);
        if (currentNode == null) return;

        if (currentNode.isCheckpoint)
        {
            MapManager.Instance.lastCheckpointIndex = currentNodeIndex;
        }

        if (currentNode.isDeadEnd)
        {
            if (this.isCheckpoint)
            {
                MapManager.Instance.MoveToCheckpoint(MapManager.Instance.lastCheckpointIndex, null);
            }
            return;
        }

        if (currentNode.nextNodeIndices.Contains(nodeIndex) && !isFight)
        {
            _GameManager.Instance.fightIndex = nodeIndex;
            _GameManager.Instance.campIndex = -1;

            if (enemydiffUp) {
                SetDiff(nodeDiff); 
            }

            if (treasure)
            {
                if (treasureWeaponMaterial != null)
                {
                    foreach (var mat in treasureWeaponMaterial)
                    {
                        _GameManager.Instance.newWeaponMaterials.Add(mat);
                    }
                }
                if (treasureWeapon != null)
                {
                    foreach (var wpn in treasureWeapon)
                    {
                        _GameManager.Instance.newWeapon.Add(wpn);
                    }
                }
                _GameManager.Instance.campIndex = nodeIndex;
            }
            if (camp)
            {
                _GameManager.Instance.selectedCharacter.Health = Mathf.Clamp(
                    _GameManager.Instance.selectedCharacter.Health + 35,
                    0,
                    _GameManager.Instance.selectedCharacter.MaxHealth);
                _GameManager.Instance.UpdateCharacterHealth(_GameManager.Instance.selectedCharacter.Health);
            }

            isMoving = true;
            MapManager.Instance.MoveToNode(nodeIndex, battleAreaIndex, 2, () => isMoving = false);
        }
    }

    public void SetDiff(int diff)
    {
        _GameManager.Instance.enemydiff = diff;
    }

    public void HighlightCurrentNode()
    {
        if (nodeButton == null) return;

        buttonTween?.Kill();
        Vector3 originalScale = nodeButton.transform.localScale;
        buttonTween = nodeButton.transform.DOScale(originalScale * 1.2f, 0.7f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void StopHighlighting()
    {
        if (buttonTween != null && buttonTween.IsActive())
        {
            buttonTween.Kill();
            buttonTween = null;
            Vector3 originalScale = nodeButton.transform.localScale / 1.1f;
            nodeButton.transform.DOScale(originalScale, 0.2f).SetEase(Ease.InOutSine);
        }
    }

    public void RemoveCharacterIcon()
    {
        if (characterIcon != null)
        {
            Destroy(characterIcon);
            characterIcon = null;
        }
    }

    public void SpawnCharacterIcon()
    {
        if (characterIcon == null && characterIconPrefab != null)
        {
            characterIcon = Instantiate(characterIconPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    public void MoveCharacterToNextNode(MapNode targetNode, System.Action onComplete)
    {
        if (characterIcon == null && characterIconPrefab != null)
        {
            characterIcon = Instantiate(characterIconPrefab, transform.position, Quaternion.identity, transform);
        }

        Connection connection = connections.Find(c => c.targetNode == targetNode);
        if (connection.lineRenderer == null)
        {
            onComplete?.Invoke();
            return;
        }

        LineRenderer lr = connection.lineRenderer;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.sortingOrder = 5;
        lr.startColor = Color.white;
        lr.endColor = Color.white;

        ResetGradient();
        lr.colorGradient = gradient;

        characterIcon.transform.SetParent(null);
        float pathDuration = 2.0f;
        Vector3[] path = new Vector3[] { transform.position, targetNode.transform.position };

        float elapsedTime = 0f;

        characterIcon.transform.DOPath(path, pathDuration, PathType.Linear)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / pathDuration);
                UpdateGradient(lr, progress);
            })
            .OnComplete(() =>
            {
                UpdateGradient(lr, 1f);
                Destroy(characterIcon); // Eski ikonu yok et
                targetNode.SpawnCharacterIcon();
                onComplete?.Invoke();
            });
    }

    private void UpdateGradient(LineRenderer lr, float progress)
    {
        colorKeys[1].time = progress;
        colorKeys[2].time = progress;
        gradient.SetKeys(colorKeys, alphaKeys);
        lr.colorGradient = gradient;
    }

    private GradientColorKey[] colorKeys;
    private GradientAlphaKey[] alphaKeys;

    private void ResetGradient()
    {
        colorKeys = new GradientColorKey[4];
        alphaKeys = new GradientAlphaKey[4];

        colorKeys[0].color = Color.black;
        colorKeys[0].time = 0f;
        colorKeys[1].color = Color.black;
        colorKeys[1].time = 0f;
        colorKeys[2].color = Color.white;
        colorKeys[2].time = 0f;
        colorKeys[3].color = Color.white;
        colorKeys[3].time = 1f;

        alphaKeys[0].alpha = 1f;
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f;
        alphaKeys[1].time = 0f;
        alphaKeys[2].alpha = 1f;
        alphaKeys[2].time = 0f;
        alphaKeys[3].alpha = 1f;
        alphaKeys[3].time = 1f;

        gradient.SetKeys(colorKeys, alphaKeys);
    }

    public void DrawPath(MapNode targetNode)
    {
        GameObject lineObj = new GameObject($"Line_{nodeIndex}_to_{targetNode.nodeIndex}");
        lineObj.transform.SetParent(this.transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, targetNode.transform.position);
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.sortingOrder = 5;

        connections.Add(new Connection { targetNode = targetNode, lineRenderer = lr });
    }

    public void RefreshVisibility()
    {
        gameObject.SetActive(isDiscovered);
    }
}