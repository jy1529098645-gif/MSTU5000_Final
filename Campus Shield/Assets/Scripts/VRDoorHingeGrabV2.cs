using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRDoorHingeGrabV2 : MonoBehaviour
{
    [Header("Door Pivot")]
    public Transform doorHinge;

    [Header("Angle Limit")]
    public float minAngle = 0f;
    public float maxAngle = 120f;
    public bool invertDirection = false;

    [Header("Lock Settings")]
    public GameObject doorLockUI;
    public float lockAngleThreshold = 2f;
    public float lockMessageDuration = 3f;
    public bool lockWhenClosed = true;

    [Header("Next Interaction")]
    public GameObject curtainToActivate;

    [Header("Debug")]
    public bool isGrabbed;
    public bool hasOpenedBefore;
    public bool isLocked;
    public float startHandAngle;
    public float currentHandAngle;
    public float handDelta;
    public float currentAngle;
    public float targetAngle;

    [Header("Mission UI")]
    public MissionUIController missionUIController;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Transform grabbingHand;

    private Transform originalParent;
    private Vector3 grabEdgeStartLocalPosition;
    private Quaternion grabEdgeStartLocalRotation;
    private Vector3 grabEdgeStartLocalScale;

    private Quaternion lockedRotation;
    private float startDoorAngle;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (doorHinge == null)
            doorHinge = transform.parent;

        originalParent = transform.parent;
        grabEdgeStartLocalPosition = transform.localPosition;
        grabEdgeStartLocalRotation = transform.localRotation;
        grabEdgeStartLocalScale = transform.localScale;

        // Locked/closed rotation is always Door Hinge Y = 0.
        Vector3 hingeEuler = doorHinge.localEulerAngles;
        lockedRotation = Quaternion.Euler(hingeEuler.x, 0f, hingeEuler.z);

        // Door may start already open, like Y = 120.
        currentAngle = NormalizeAngle(doorHinge.localEulerAngles.y);
        targetAngle = currentAngle;

        if (Mathf.Abs(currentAngle) > lockAngleThreshold)
            hasOpenedBefore = true;

        if (doorLockUI != null)
            doorLockUI.SetActive(false);

        if (curtainToActivate != null)
            curtainToActivate.SetActive(false);
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabStarted);
        grabInteractable.selectExited.AddListener(OnGrabEnded);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabStarted);
        grabInteractable.selectExited.RemoveListener(OnGrabEnded);
    }

    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        if (isLocked)
            return;

        Debug.Log("Door grabbed");

        isGrabbed = true;

        Transform attachTransform = args.interactorObject.GetAttachTransform(grabInteractable);
        grabbingHand = attachTransform != null ? attachTransform : args.interactorObject.transform;

        startHandAngle = GetHandAngle();
        startDoorAngle = currentAngle;

        KeepGrabEdgeAttached();
    }

    private void OnGrabEnded(SelectExitEventArgs args)
    {
        Debug.Log("Door released");

        isGrabbed = false;
        grabbingHand = null;

        KeepGrabEdgeAttached();
    }

    private void Update()
    {
        if (isLocked)
        {
            KeepGrabEdgeAttached();
            return;
        }

        if (grabbingHand != null)
        {
            currentHandAngle = GetHandAngle();

            handDelta = Mathf.DeltaAngle(startHandAngle, currentHandAngle);

            if (invertDirection)
                handDelta *= -1f;

            targetAngle = Mathf.Clamp(startDoorAngle + handDelta, minAngle, maxAngle);
            currentAngle = targetAngle;
        }

        if (doorHinge != null)
        {
            doorHinge.localRotation = lockedRotation * Quaternion.Euler(0f, currentAngle, 0f);
        }

        CheckDoorLockCondition();
    }

    private void LateUpdate()
    {
        KeepGrabEdgeAttached();
    }

    private void CheckDoorLockCondition()
    {
        if (!lockWhenClosed || doorHinge == null)
            return;

        float hingeY = NormalizeAngle(doorHinge.localEulerAngles.y);

        if (Mathf.Abs(hingeY) > lockAngleThreshold)
            hasOpenedBefore = true;

        if (hasOpenedBefore && Mathf.Abs(hingeY) <= lockAngleThreshold)
            LockDoor();
    }

    private void LockDoor()
    {
        if (isLocked)
            return;

        Debug.Log("Door locked");

        isLocked = true;
        isGrabbed = false;
        grabbingHand = null;

        currentAngle = 0f;
        targetAngle = 0f;

        if (doorHinge != null)
            doorHinge.localRotation = lockedRotation;

        KeepGrabEdgeAttached();

        if (grabInteractable != null)
            grabInteractable.enabled = false;

        if (missionUIController != null)
        {
           missionUIController.CompleteCurrentMission();
        } 

        ActivateCurtainInteraction();

        StartCoroutine(ShowDoorLockedUI());
    }

    private void ActivateCurtainInteraction()
    {
        if (curtainToActivate == null)
            return;

        curtainToActivate.SetActive(true);

        // This calls ActivateCurtain() on your CurtainPullToClose script.
        // If the curtain script is not attached yet, Unity will not throw an error.
        curtainToActivate.SendMessage("ActivateCurtain", SendMessageOptions.DontRequireReceiver);

        Debug.Log("Curtain interaction activated");
    }

    private IEnumerator ShowDoorLockedUI()
    {
        if (doorLockUI != null)
            doorLockUI.SetActive(true);

        yield return new WaitForSeconds(lockMessageDuration);

        if (doorLockUI != null)
            doorLockUI.SetActive(false);
    }

    private void KeepGrabEdgeAttached()
    {
        if (originalParent != null && transform.parent != originalParent)
        {
            transform.SetParent(originalParent, false);
        }

        transform.localPosition = grabEdgeStartLocalPosition;
        transform.localRotation = grabEdgeStartLocalRotation;
        transform.localScale = grabEdgeStartLocalScale;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private float GetHandAngle()
    {
        if (grabbingHand == null || doorHinge == null)
            return startHandAngle;

        Vector3 localHandPosition;

        if (doorHinge.parent != null)
        {
            localHandPosition =
                doorHinge.parent.InverseTransformPoint(grabbingHand.position)
                - doorHinge.localPosition;
        }
        else
        {
            localHandPosition = grabbingHand.position - doorHinge.position;
        }

        localHandPosition.y = 0f;

        if (localHandPosition.sqrMagnitude < 0.0001f)
            return startHandAngle;

        return Mathf.Atan2(localHandPosition.x, localHandPosition.z) * Mathf.Rad2Deg;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}