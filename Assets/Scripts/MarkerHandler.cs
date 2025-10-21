using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class MaskMarkerHandler : MonoBehaviour
{
    [SerializeField] private GameObject prefabTracker;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> instantiatedPrefabs = new Dictionary<string, GameObject>();
    private bool isDetectedOnce = false;

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    public void Replay()
    {
        foreach (var prefab in instantiatedPrefabs.Values)
        {
            if (prefab != null)
            {
                Destroy(prefab);
            }
        }
        instantiatedPrefabs.Clear();
        isDetectedOnce = false; // Reset so marker can be detected again
    }

    void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (!isDetectedOnce)
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                Debug.Log("Detected a new marker: " + trackedImage.referenceImage.name);

                if (prefabTracker != null && !instantiatedPrefabs.ContainsKey(trackedImage.referenceImage.name))
                {
                    // Capture the current position and rotation of the marker
                    Vector3 spawnPosition = trackedImage.transform.position;
                    Quaternion spawnRotation = trackedImage.transform.rotation;

                    // Instantiate at world position WITHOUT parenting to the tracked image
                    GameObject prefab = Instantiate(prefabTracker, spawnPosition, spawnRotation);

                    isDetectedOnce = true;
                    instantiatedPrefabs[trackedImage.referenceImage.name] = prefab;

                    Debug.Log("Prefab spawned at static position: " + spawnPosition);
                }
            }
        }
    }
}