using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Added for TrackingState
using TMPro;

public class MaskMarkerHandler : MonoBehaviour
{
    [SerializeField] private GameObject prefabTracker;
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Tooltip("How long (in seconds) to average the marker's pose before placing the prefab.")]
    [SerializeField] private float placementDelay = 0.5f; // NEW: Inspector variable for the delay

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
        isDetectedOnce = false;
    }

    void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // This logic remains the same: only run this *once* until Replay() is called.
        if (!isDetectedOnce)
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                Debug.Log("Detected a new marker: " + trackedImage.referenceImage.name);

                if (prefabTracker != null)
                {
                    // *** CHANGED ***
                    // Instead of instantiating, set the flag and start the coroutine.

                    isDetectedOnce = true; // Set flag immediately to prevent multiple coroutines
                    StartCoroutine(RefineAndPlacePrefab(trackedImage));

                    // We only process the first marker found, so break the loop
                    break;
                }
            }
        }
    }

    /// <summary>
    /// NEW COROUTINE:
    /// Tracks the image for a short delay, averages its pose,
    /// and then instantiates the prefab at that stable position.
    /// </summary>
    private IEnumerator RefineAndPlacePrefab(ARTrackedImage trackedImage)
    {
        Debug.Log($"Starting pose refinement for {trackedImage.referenceImage.name}...");

        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();

        float startTime = Time.time;
        float endTime = startTime + placementDelay;

        // Loop for the duration of the delay
        while (Time.time < endTime)
        {
            // Only add pose data if the marker is being actively tracked
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                positions.Add(trackedImage.transform.position);
                rotations.Add(trackedImage.transform.rotation);
            }

            // Wait for the next frame
            yield return null;
        }

        Debug.Log($"Refinement complete. Sampled {positions.Count} poses.");

        // Check if we actually gathered any valid data
        if (positions.Count > 0)
        {
            // --- Calculate Average Position ---
            Vector3 averagePosition = Vector3.zero;
            foreach (Vector3 pos in positions)
            {
                averagePosition += pos;
            }
            averagePosition /= positions.Count;

            // --- Calculate "Average" Rotation ---
            // True quaternion averaging is complex. A good, simple approximation
            // is to Slerp (spherical interpolation) between the first and last rotation.
            Quaternion finalRotation = rotations[0]; // Default to first
            if (rotations.Count > 1)
            {
                finalRotation = Quaternion.Slerp(rotations[0], rotations[rotations.Count - 1], 0.5f);
            }

            // --- Instantiate ---
            // Instantiate at the averaged position and interpolated rotation
            GameObject prefab = Instantiate(prefabTracker, averagePosition, finalRotation);

            // Store it (as per your original logic)
            instantiatedPrefabs[trackedImage.referenceImage.name] = prefab;

            Debug.Log("Prefab spawned at averaged static position: " + averagePosition);
        }
        else
        {
            // This could happen if tracking was lost immediately after detection
            Debug.LogWarning($"Could not place prefab for {trackedImage.referenceImage.name} - lost tracking during refinement.");

            // Reset the flag so we can try detecting again
            isDetectedOnce = false;
        }
    }
}