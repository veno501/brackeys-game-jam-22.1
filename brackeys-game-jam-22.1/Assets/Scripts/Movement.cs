using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 10f;
    Rigidbody rbody;
    Animator anim;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Move();

        // transform.GetChild(0).gameObject.SetActive(Input.GetKey(KeyCode.Space));
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        rbody.MovePosition(rbody.position + new Vector3(h, 0, v) * Time.deltaTime * speed);

        // Vector3 cursorPos = Input.mousePosition;
        // Vector3 dir = Vector3.Normalize(cursorPos - (Vector3)(Vector2)Camera.main.WorldToScreenPoint(rbody.position));
        // dir = new Vector3(dir.x, 0, dir.y);

        Vector3 dir = new Vector3(h, 0, v).normalized;
        if (new Vector3(h, 0, v).magnitude == 0) dir = transform.forward;


        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        rot = Quaternion.Lerp(rbody.rotation, rot, Time.deltaTime * rotationSpeed);
        rbody.MoveRotation(rot);


        float forward = Vector3.Dot(new Vector3(h, 0, v)/1.4f, dir)*2f;
        anim.SetFloat("Forward", forward * 1.0f, 0.02f, Time.deltaTime);

        // float turn = Mathf.Sign(Vector3.SignedAngle(new Vector3(h, 0, v)/1.4f, dir, Vector3.up)) * (1.0f - Mathf.Abs(Vector3.Dot(new Vector3(h, 0, v)/1.4f, dir)));
        float turn = Vector3.SignedAngle(transform.forward, dir, Vector3.up) / 30f;
  			anim.SetFloat("Turn", turn, 0.02f, Time.deltaTime);

				float turnAnim = Mathf.Sign(turn) * Mathf.Sqrt(Mathf.Abs(turn / 60f));
				anim.SetFloat("CameraTurn", turnAnim, 0.1f, Time.deltaTime);
    }
}
