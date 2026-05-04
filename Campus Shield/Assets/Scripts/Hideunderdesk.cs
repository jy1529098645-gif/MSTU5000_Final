using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class HideUnderDesk : MonoBehaviour
{
    public Transform xrOrigin;
    public Transform hidePosition;
    public PCController pcController;

    private bool playerNearby = false;
    private bool isHiding = false;

    void Update()
    {
        if (playerNearby && !isHiding)
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                StartCoroutine(HidePlayer());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        playerNearby = false;
    }

    IEnumerator HidePlayer()
    {
        isHiding = true;

        if (pcController != null)
            pcController.enabled = false;

        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = xrOrigin.position;
        Vector3 endPos = hidePosition.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            xrOrigin.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        if (pcController != null)
            pcController.enabled = true;

        Debug.Log("Player is hiding under desk!");
    }
}