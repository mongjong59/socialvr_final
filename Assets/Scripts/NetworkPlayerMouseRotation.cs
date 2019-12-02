using UnityEngine;
using Mirror;

public class NetworkPlayerMouseRotation : NetworkBehaviour
{
    public float sensitivity = 150f;

    float xRotation = 0f;

    private void Start()
    {
        if (!isLocalPlayer) return;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.Rotate((Vector3.up * x) * sensitivity * Time.deltaTime);

        Camera camera = GetComponentInChildren<Camera>();

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
