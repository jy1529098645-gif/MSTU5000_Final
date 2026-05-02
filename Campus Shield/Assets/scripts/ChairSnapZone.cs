using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChairSnapZone : MonoBehaviour
{
    public Transform snapPoint;
    public float snapDistance = 0.5f;
    public GameObject chair;
    public GameObject chairGhost;
    public DoorLock doorLock;

    private bool isSnapped = false;
    private XRGrabInteractable grab;

    void Start()
    {
        if (chairGhost != null)
        {
            chairGhost.SetActive(false);
            chairGhost.transform.position = snapPoint.position;
            chairGhost.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        grab = chair.GetComponent<XRGrabInteractable>();
        grab.enabled = false;
    }

    void Update()
    {
        if (isSnapped) return;

        if (doorLock != null && !doorLock.isOpen)
        {
            grab.enabled = true;
        }
        else
        {
            grab.enabled = false;
        }

        if (grab.isSelected)
        {
            chairGhost.SetActive(true);
        }
        else
        {
            chairGhost.SetActive(false);
        }

        float distance = Vector3.Distance(chair.transform.position, snapPoint.position);
        if (distance < snapDistance && grab.isSelected)
        {
            SnapChair();
        }
    }

    private void SnapChair()
    {
        isSnapped = true;
        chairGhost.SetActive(false);
        chair.transform.position = snapPoint.position;
        chair.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (grab != null) grab.enabled = false;

        Rigidbody rb = chair.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        doorLock.isBlockedByChair = true;

        Debug.Log("Chair snapped to door!");
    }
}