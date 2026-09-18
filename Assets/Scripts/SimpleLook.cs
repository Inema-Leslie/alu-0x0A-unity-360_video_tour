using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleLook : MonoBehaviour
{
    public float sensitivity = 0.2f;
    private Vector2 rotation = Vector2.zero;

    void Start()
    {
        Vector3 angles = transform.localEulerAngles;
        rotation.x = angles.y;
        rotation.y = angles.x;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue() * sensitivity;
            rotation.x += delta.x;
            rotation.y -= delta.y;
            rotation.y = Mathf.Clamp(rotation.y, -85f, 85f);

            transform.localRotation = Quaternion.Euler(rotation.y, rotation.x, 0);
        }
    }
}