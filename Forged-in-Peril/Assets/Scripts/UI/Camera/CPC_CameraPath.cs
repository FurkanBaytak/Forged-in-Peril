using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CPC_Visual
{
    public Color pathColor = Color.green;
    public Color inactivePathColor = Color.gray;
    public Color frustrumColor = Color.white;
    public Color handleColor = Color.yellow;
}

public enum CPC_ECurveType
{
    EaseInAndOut,
    Linear,
    Custom
}

public enum CPC_EAfterLoop
{
    Continue,
    Stop
}

[System.Serializable]
public class CPC_Point
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 handleprev;
    public Vector3 handlenext;
    public CPC_ECurveType curveTypeRotation;
    public AnimationCurve rotationCurve;
    public CPC_ECurveType curveTypePosition;
    public AnimationCurve positionCurve;
    public bool chained;

    public CPC_Point(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
        handleprev = Vector3.back;
        handlenext = Vector3.forward;
        curveTypeRotation = CPC_ECurveType.EaseInAndOut;
        rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        curveTypePosition = CPC_ECurveType.Linear;
        positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        chained = true;
    }
}

public class CPC_CameraPath : MonoBehaviour
{
    public bool useMainCamera = true;
    public Camera selectedCamera;
    public bool lookAtTarget = false;
    public Transform target;
    public bool playOnAwake = false;
    public float playOnAwakeTime = 10;
    public List<CPC_Point> points = new List<CPC_Point>();
    public CPC_Visual visual;
    public bool looped = false;
    public bool alwaysShow = true;
    public CPC_EAfterLoop afterLoop = CPC_EAfterLoop.Continue;

    private int currentWaypointIndex;
    private float currentTimeInWaypoint;
    private float timePerSegment;

    private bool paused = false;
    private bool playing = false;

    void Start()
    {
        if (Camera.main == null) { 
            //Debug.LogError("There is no main camera in the scene!");
        }
        if (useMainCamera)
            selectedCamera = Camera.main;
        else if (selectedCamera == null)
        {
            selectedCamera = Camera.main;
            //Debug.LogError("No camera selected for following path, defaulting to main camera");
        }

        if (lookAtTarget && target == null)
        {
            lookAtTarget = false;
            //Debug.LogError("No target selected to look at, defaulting to normal rotation");
        }

        foreach (var index in points)
        {
            if (index.curveTypeRotation == CPC_ECurveType.EaseInAndOut) index.rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (index.curveTypeRotation == CPC_ECurveType.Linear) index.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
            if (index.curveTypePosition == CPC_ECurveType.EaseInAndOut) index.positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (index.curveTypePosition == CPC_ECurveType.Linear) index.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }

        if (playOnAwake)
            PlayPath(playOnAwakeTime);
    }

    public void PlayPathReverse(float time)
    {
        if (time <= 0) time = 0.001f;
        paused = false;
        playing = true;
        StopAllCoroutines();
        StartCoroutine(FollowPathReverse(time));
    }

    IEnumerator FollowPathReverse(float time)
    {
        UpdateTimeInSeconds(time);
        //Debug.Log($"FollowPathReverse: Setting currentWaypointIndex to {points.Count - 1}");
        currentWaypointIndex = points.Count - 1;
        while (currentWaypointIndex >= 0)
        {
            currentTimeInWaypoint = 1;
            //Debug.Log($"FollowPathReverse: Moving from waypoint {currentWaypointIndex} to {currentWaypointIndex - 1}");
            while (currentTimeInWaypoint > 0)
            {
                if (!paused)
                {
                    currentTimeInWaypoint -= Time.deltaTime / timePerSegment;
                    selectedCamera.transform.position = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
                    selectedCamera.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
                }
                yield return null;
            }
            --currentWaypointIndex;
            if (currentWaypointIndex < 0 && !looped) break;
            if (currentWaypointIndex < 0 && afterLoop == CPC_EAfterLoop.Continue) currentWaypointIndex = points.Count - 1;
        }
        //Debug.Log("FollowPathReverse: Reverse path completed.");
        StopPath();
    }

    private Coroutine currentCoroutine;

    public void PlayPath(float time)
    {
        if (time <= 0) time = 0.001f;
        paused = false;
        playing = true;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(FollowPath(time));
    }

    public void StopPath()
    {
        playing = false;
        paused = false;
        StopAllCoroutines();
    }

    public void UpdateTimeInSeconds(float seconds)
    {
        timePerSegment = seconds / ((looped) ? points.Count : points.Count - 1);
    }

    public void PausePath()
    {
        paused = true;
        playing = false;
    }
    public void ResumePath()
    {
        if (!playing)
        {
            paused = false;
            playing = true;
            //Debug.Log("Kamera hareketine devam ediyor.");
        }
        else
        {
            //Debug.LogWarning("Kamera zaten oynatılıyor veya duraklatılmamış. Coroutine sıfırlanabilir.");
            StopAllCoroutines();
            playing = true;
            paused = false;
            currentCoroutine = StartCoroutine(FollowPath(timePerSegment * points.Count));
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        //Debug.Log($"Kamera {currentWaypointIndex + 1}. noktada bekliyor.");
        playing = false; 
        yield return new WaitUntil(() => playing); 
        //Debug.Log($"Kamera {currentWaypointIndex + 1}. noktadan devam ediyor.");
    }

    public bool IsPaused()
    {
        return paused;
    }

    public bool IsPlaying()
    {
        return playing;
    }

    public int GetCurrentWayPoint()
    {
        return currentWaypointIndex;
    }

    public float GetCurrentTimeInWaypoint()
    {
        return currentTimeInWaypoint;
    }

    public void SetCurrentWayPoint(int value)
    {
        currentWaypointIndex = Mathf.Clamp(value, 0, points.Count - 1);
        //Debug.Log($"SetCurrentWayPoint called with {value}. CurrentWaypointIndex set to {currentWaypointIndex}");
        RefreshTransform();
    }

    public void SetCurrentTimeInWaypoint(float value)
    {
        currentTimeInWaypoint = value;
    }

    public void RefreshTransform()
    {
        selectedCamera.transform.position = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
        if (!lookAtTarget)
            selectedCamera.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
        else
            selectedCamera.transform.rotation = Quaternion.LookRotation((target.transform.position - selectedCamera.transform.position).normalized);
    }

    IEnumerator FollowPath(float time)
    {
        UpdateTimeInSeconds(time);
        currentWaypointIndex = Mathf.Max(0, currentWaypointIndex);

        while (currentWaypointIndex < points.Count)
        {
            currentTimeInWaypoint = 0;

            while (currentTimeInWaypoint < 1)
            {
                if (!paused)
                {
                    currentTimeInWaypoint += Time.deltaTime / timePerSegment;
                    selectedCamera.transform.position = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
                    selectedCamera.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
                }
                else
                {
                    yield return null;
                }

                yield return null;
            }

            //Debug.Log($"Kamera {currentWaypointIndex + 1}. noktaya ulaştı.");

            yield return WaitAtWaypoint();

            ++currentWaypointIndex;
        }

        StopPath();
    }

    int GetNextIndex(int index)
    {
        if (index == points.Count - 1)
            return 0;
        return index + 1;
    }

    Vector3 GetBezierPosition(int pointIndex, float time)
    {
        float t = points[pointIndex].positionCurve.Evaluate(time);
        int nextIndex = GetNextIndex(pointIndex);
        return
            Vector3.Lerp(
                Vector3.Lerp(
                    Vector3.Lerp(points[pointIndex].position,
                        points[pointIndex].position + points[pointIndex].handlenext, t),
                    Vector3.Lerp(points[pointIndex].position + points[pointIndex].handlenext,
                        points[nextIndex].position + points[nextIndex].handleprev, t), t),
                Vector3.Lerp(
                    Vector3.Lerp(points[pointIndex].position + points[pointIndex].handlenext,
                        points[nextIndex].position + points[nextIndex].handleprev, t),
                    Vector3.Lerp(points[nextIndex].position + points[nextIndex].handleprev,
                        points[nextIndex].position, t), t), t);
    }

    private Quaternion GetLerpRotation(int pointIndex, float time)
    {
        return Quaternion.LerpUnclamped(points[pointIndex].rotation, points[GetNextIndex(pointIndex)].rotation, points[pointIndex].rotationCurve.Evaluate(time));
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject == gameObject || alwaysShow)
        {
            if (points.Count >= 2)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (i < points.Count - 1)
                    {
                        var index = points[i];
                        var indexNext = points[i + 1];
                        UnityEditor.Handles.DrawBezier(index.position, indexNext.position, index.position + index.handlenext,
                            indexNext.position + points[i + 1].handleprev, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                    }
                    else if (looped)
                    {
                        var index = points[i];
                        var indexNext = points[0];
                        UnityEditor.Handles.DrawBezier(index.position, indexNext.position, index.position + index.handlenext,
                            indexNext.position + points[0].handleprev, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                    }
                }
            }

            for (int i = 0; i < points.Count; i++)
            {
                var index = points[i];
                Gizmos.matrix = Matrix4x4.TRS(index.position, index.rotation, Vector3.one);
                Gizmos.color = visual.frustrumColor;
                Gizmos.DrawFrustum(Vector3.zero, 90f, 0.25f, 0.01f, 1.78f);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
#endif

    public void PlayPathReverseTo(int targetWaypoint, float time)
    {
        if (time <= 0) time = 0.001f;
        paused = false;
        playing = true;
        StopAllCoroutines();
        StartCoroutine(FollowPathReverseTo(targetWaypoint, time));
    }

    IEnumerator FollowPathReverseTo(int targetWaypoint, float time)
    {
        UpdateTimeInSeconds(time);
        //Debug.Log($"Starting reverse path from {currentWaypointIndex} to {targetWaypoint}.");

        while (currentWaypointIndex > targetWaypoint)
        {
            currentTimeInWaypoint = 1;
            //Debug.Log($"Moving from waypoint {currentWaypointIndex} to {currentWaypointIndex - 1}.");
            while (currentTimeInWaypoint > 0)
            {
                if (!paused)
                {
                    currentTimeInWaypoint -= Time.deltaTime / timePerSegment;
                    selectedCamera.transform.position = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
                    selectedCamera.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
                }
                yield return null;
            }
            --currentWaypointIndex;
        }

        //Debug.Log("Reverse path completed.");
        StopPath();
    }
}