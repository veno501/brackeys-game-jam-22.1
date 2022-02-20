using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 10f;
    Rigidbody rbody;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();

        transform.GetChild(0).gameObject.SetActive(Input.GetKey(KeyCode.Space));
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        rbody.MovePosition(rbody.position + new Vector3(h, 0, v) * Time.deltaTime * speed);

        Vector3 cursorPos = Input.mousePosition;
        Vector3 dir = Vector3.Normalize(cursorPos - (Vector3)(Vector2)Camera.main.WorldToScreenPoint(rbody.position));
        dir = new Vector3(dir.x, 0, dir.y);
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        rot = Quaternion.Lerp(rbody.rotation, rot, Time.deltaTime * rotationSpeed);
        rbody.MoveRotation(rot);
    }
}
