using UnityEngine;
using Mirror;

public class NetworkPlayerKeyboardMovement : NetworkBehaviour
{
    [SerializeField]
    float _moveSpeed = 0.05f;

    [SerializeField]
    float _rotateSpeed = 1f;

    void Update()
    {
        if (!isLocalPlayer) return;

        var move = Vector3.zero;
        var rotate = 0f;

        if (Input.GetKey(KeyCode.W))
            move += transform.forward;
        if (Input.GetKey(KeyCode.A))
            move += -transform.right;
        if (Input.GetKey(KeyCode.S))
            move += -transform.forward;
        if (Input.GetKey(KeyCode.D))
            move += transform.right;

        if (Input.GetKey(KeyCode.E))
            rotate += 1;
        if (Input.GetKey(KeyCode.Q))
            rotate -= 1;

        var moveSpeed = move * _moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            moveSpeed *= 2;
        transform.Rotate(Vector3.up, _rotateSpeed * rotate);
        transform.position += moveSpeed;
    }
}
