using UnityEngine;
using Mirror;

public class NetworkPlayerMouseRotation : NetworkBehaviour
{
    public float sensitivity = 150f;

    float _xRotation = 0f;

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

        _xRotation -= y;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        transform.Rotate(Time.deltaTime * x * sensitivity * Vector3.up);

        NetworkPlayerUtilities.PlayerCenterEyeAnchor(gameObject).transform.parent.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }
}
