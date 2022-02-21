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


        float forward = new Vector2(h, v).magnitude;
        anim.SetFloat("Forward", forward * 1.0f, 0.02f, Time.deltaTime);

        float turn = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
  			anim.SetFloat("Turn", turn, 0.02f, Time.deltaTime);

				float turnAnim = Mathf.Sign(turn) * Mathf.Sqrt(Mathf.Abs(turn / 60f));
				anim.SetFloat("CameraTurn", turnAnim, 0.1f, Time.deltaTime);
    }
}
