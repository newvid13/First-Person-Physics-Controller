using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPhys_Look : MonoBehaviour
{
    public bool isActive;
    public float mainSens = 1f;
    [SerializeField] private Vector2 mouseSensitivity, mouseAcceleration;
    private Vector2 mouseVel, camRot;

    [SerializeField] private Transform orientation;

    void Update()
    {
        if (!isActive)
            return;

        Look();
    }

    private void Look()
    {
        Vector2 mouseInput;
        mouseInput.x = Input.GetAxisRaw("Mouse X") * mouseSensitivity.x * mainSens;
        mouseInput.y = -Input.GetAxisRaw("Mouse Y") * mouseSensitivity.y * mainSens;

        mouseVel = new Vector2(Mathf.MoveTowards(mouseVel.x, mouseInput.x, mouseAcceleration.x),
            Mathf.MoveTowards(mouseVel.y, mouseInput.y, mouseAcceleration.y));

        camRot += mouseVel * Time.deltaTime;
        camRot.y = Mathf.Clamp(camRot.y, -85f, 85f);
        transform.eulerAngles = new Vector3(camRot.y, camRot.x, 0);
        orientation.eulerAngles = new Vector3(0, camRot.x, 0);
    }
}
