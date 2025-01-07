using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UIElements;

public class BattleAreaManager : MonoBehaviour
{
    public List<BattleArea> battleAreas;
    [SerializeField]
    private int previousSelectedIndex = -1;
    public Camera mainCamera;
    [SerializeField]
    private int selectedBattleAreaIndex = -1;

    public float cameraMoveDuration = 2f;
    public float cameraMoveDelay = 0.5f;

    public Vector3 characterPosition;
    public Quaternion characterRotation;
    public Vector3 enemyPosition;
    public Quaternion enemyRotation;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }


    public void ApplyBattleArea(int index)
    {
        if (index < 0 || index >= battleAreas.Count)
        {
            Debug.LogError("Invalid Battle Area Index");
            return;
        }

        BattleArea selectedArea = battleAreas[index];

        if (selectedArea.cameraStartTransform != null && selectedArea.cameraEndTransform != null)
        {
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is not assigned.");
                return;
            }

            mainCamera.transform.position = selectedArea.cameraStartTransform.position;
            mainCamera.transform.rotation = selectedArea.cameraStartTransform.rotation;
            mainCamera.transform.localScale = selectedArea.cameraStartTransform.localScale;

            mainCamera.transform.DOKill();

            Sequence cameraSequence = DOTween.Sequence();
            cameraSequence.AppendInterval(cameraMoveDelay);
            cameraSequence.Append(mainCamera.transform.DOMove(selectedArea.cameraEndTransform.position, cameraMoveDuration).SetEase(Ease.InOutSine));
            cameraSequence.Join(mainCamera.transform.DORotate(selectedArea.cameraEndTransform.rotation.eulerAngles, cameraMoveDuration).SetEase(Ease.InOutSine));
            cameraSequence.Join(mainCamera.transform.DOScale(selectedArea.cameraEndTransform.localScale, cameraMoveDuration).SetEase(Ease.InOutSine));

            characterPosition = selectedArea.characterSpawnPoint.spawnTransform.position;
            characterRotation = selectedArea.characterSpawnPoint.spawnTransform.rotation;
            enemyPosition = selectedArea.enemySpawnPoint.spawnTransform.position;
            enemyRotation = selectedArea.enemySpawnPoint.spawnTransform.rotation;
        }
        else
        {
            Debug.LogWarning("Camera start or end transform is not assigned for Battle Area: " + selectedArea.battleAreaName);
        }
    }

    public int SelectedBattleAreaIndex
    {
        get { return selectedBattleAreaIndex; }
        set
        {
            if (value != selectedBattleAreaIndex)
            {
                selectedBattleAreaIndex = value;
                ApplyBattleArea(selectedBattleAreaIndex);
                previousSelectedIndex = selectedBattleAreaIndex;
            }
        }
    }
}
