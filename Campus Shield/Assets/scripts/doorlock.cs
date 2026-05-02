using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [Header("Door Settings")]
    public float slideDistance = 1.5f;
    public float slideSpeed = 2f;

    [Header("State")]
    public bool isLocked = false;
    public bool isOpen = true;
    public bool isBlockedByChair = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(slideDistance, 0, 0);
        isOpen = true;
        transform.position = openPosition;
    }

    void Update()
    {
        Vector3 targetPosition = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.Lerp(
            transform.position, targetPosition, Time.deltaTime * slideSpeed
        );
    }

    public void TryToggleDoor()
    {
        if (isLocked)
        {
            Debug.Log("Door is locked!");
            return;
        }
        if (isBlockedByChair)
        {
            Debug.Log("Door is blocked by chair!");
            return;
        }
        isOpen = !isOpen;
    }

    public void LockDoor()
    {
        isLocked = true;
        isOpen = false;
        Debug.Log("Door locked");
    }

    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("Door unlocked");
    }
}