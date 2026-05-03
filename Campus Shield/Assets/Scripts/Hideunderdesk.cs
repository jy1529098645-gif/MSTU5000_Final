using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class HideUnderDesk : MonoBehaviour
{
    public Transform xrOrigin;
    public Transform hidePosition;

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
        Debug.Log("Triggered by: " + other.name);
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    IEnumerator HidePlayer()
    {
        isHiding = true;

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

        Debug.Log("Player is hiding under desk!");
    }
}