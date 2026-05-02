using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ToggleGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private IXRSelectInteractor currentInteractor;
    private bool isHeld = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!isHeld)
        {
            isHeld = true;
            currentInteractor = args.interactorObject;
        }
        else
        {
            isHeld = false;
            grabInteractable.interactionManager.SelectExit(
                currentInteractor, grabInteractable
            );
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        currentInteractor = null;
    }
}