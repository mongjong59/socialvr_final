using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotation : MonoBehaviour
{
    public float sensitivity = 150f;
    public GameObject playeCamera;

    float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.Rotate((Vector3.up * x) * sensitivity * Time.deltaTime);
    
        playeCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
