using UnityEngine;

public class LockButton : MonoBehaviour
{
    public DoorLock doorLock;
    public Material lockedMaterial;
    public Material unlockedMaterial;
    private Renderer btn;

    void Start()
    {
        btn = GetComponent<Renderer>();
    }

    public void ToggleLock()
    {
        if (doorLock.isBlockedByChair)
        {
            Debug.Log("Door is blocked by chair!");
            return;
        }

        if (doorLock.isLocked)
        {
            doorLock.UnlockDoor();
            doorLock.isOpen = true;
            if (btn) btn.material = unlockedMaterial;
        }
        else
        {
            doorLock.isOpen = false;
            doorLock.LockDoor();
            if (btn) btn.material = lockedMaterial;
        }
    }
}