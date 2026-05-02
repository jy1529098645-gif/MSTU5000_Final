using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorHandle : MonoBehaviour
{
    public DoorLock doorLock;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnHandleGrabbed);
    }

    void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnHandleGrabbed);
    }

    private void OnHandleGrabbed(SelectEnterEventArgs args)
    {
        doorLock.TryToggleDoor();
    }
}