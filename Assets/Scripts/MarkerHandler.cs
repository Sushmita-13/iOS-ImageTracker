using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class MaskMarkerHandler : MonoBehaviour
{
    [SerializeField] private GameObject prefabTracker;
    //[SerializeField] private TextMeshProUGUI text;

    [SerializeField] private ARTrackedImageManager trackedImageManager;
    private GameObject instantiatedPrefab;
    private Vector3 trackedPosition;
    private Quaternion trackedRotation;


    void Awake()
    {
        //trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    public void Replay()
    {
        Destroy(instantiatedPrefab);
        StartCoroutine(CreatePrefab());
    }

    IEnumerator CreatePrefab()
    {
        yield return new WaitForSeconds(2f);
        if (prefabTracker != null)
        {
            instantiatedPrefab = Instantiate(prefabTracker, trackedPosition, trackedRotation);
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            // Handle the detection of a new marker
            Debug.Log("Detected a new marker: " + trackedImage.referenceImage.name);
            //text.text = "Detected a new marker: " + trackedImage.referenceImage.name +" "+ trackedImage.transform.position + " " + trackedImage.transform.rotation;
            trackedPosition = trackedImage.transform.position;
            trackedRotation = trackedImage.transform.rotation;
            //DebugHandler.Instance.ChangeText("Detected a new marker: " + trackedImage.referenceImage.name);
            // Example: Instantiate a prefab at the marker location
            if (prefabTracker != null)
            {
                instantiatedPrefab = Instantiate(prefabTracker, trackedPosition, trackedRotation);
            }

            // 
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            // Handle the update of an existing marker
            /*if(instantiatedPrefab != null)
            {
                instantiatedPrefab.transform.Rotate(0, 15f, 0);
            }*/
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            // Handle the removal of a marker
            //Destroy(instantiatedPrefab);
        }
    }
}
