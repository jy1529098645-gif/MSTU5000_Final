using UnityEngine;
using UnityEngine.InputSystem;

public class PCController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float lookSpeed = 2f;
    public Transform cameraTransform; // 拖入 Main Camera

    private float rotY = 0f;

    void Update()
    {
        // 基于相机方向移动
        float h = Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0;
        float v = Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * v + right * h) * moveSpeed * Time.deltaTime;
        transform.Translate(moveDir, Space.World);

        // 视角转动
        if (Mouse.current.rightButton.isPressed)
        {
            float mouseX = Mouse.current.delta.x.ReadValue() * lookSpeed;
            rotY += mouseX;
            transform.localEulerAngles = new Vector3(0, rotY, 0);
        }
    }
}