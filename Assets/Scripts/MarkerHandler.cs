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
                GameObject prefab = Instantiate(prefabTracker, trackedImage.transform);
                    isDetectedOnce = true;
                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localRotation = Quaternion.identity;
                instantiatedPrefabs[trackedImage.referenceImage.name] = prefab;
            }
        }
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            // The prefab will automatically update because it's parented to the tracked image
            // You can add logic here if you need to show/hide based on tracking state
            // if (instantiatedPrefabs.ContainsKey(trackedImage.referenceImage.name))
            // {
            //     GameObject prefab = instantiatedPrefabs[trackedImage.referenceImage.name];
            //     prefab.SetActive(trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking);
            // }
        }
        //foreach (ARTrackedImage trackedImage in eventArgs.removed)
        //{
        //    if (instantiatedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        //    {
        //        Destroy(instantiatedPrefabs[trackedImage.referenceImage.name]);
        //        instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
        //    }
        //}
        }
    }
}