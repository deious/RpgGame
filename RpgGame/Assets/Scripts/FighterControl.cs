using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterControl : MonoBehaviour
{
    [Header("이동관련속성")]
    public float MoveSpeed = 2.0f;
    public float RunSpeed = 3.5f;
    public float DirectionRotateSpeed = 100.0f; // 이동방향 변경을 위한 속도
    public float BodyRotateSpeed = 2.0f;        // 몸통의 방향을 변경하기 위한 속도
    [Range(0.01f, 5.0f)]
    public float VelocityChangeSpeed = 0.1f;
    private Vector3 CurrentVelocity = Vector3.zero;
    private Vector3 MoveDirection = Vector3.zero;
    private CharacterController myCharacterController = null;
    private CollisionFlags collisionFlags = CollisionFlags.None;

    // Start is called before the first frame update
    void Start()
    {
        myCharacterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        Transform CameraTransform = Camera.main.transform;
        Vector3 forward = CameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

        float vetical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 targetDirection = horizontal * right + vetical * forward;

        MoveDirection = Vector3.RotateTowards(MoveDirection, targetDirection, DirectionRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        MoveDirection = MoveDirection.normalized;

        float speed = MoveSpeed;

        Vector3 moveAmount = (MoveDirection * speed * Time.deltaTime);
        collisionFlags = myCharacterController.Move(moveAmount);
    }

    private void OnGUI()
    {
        if(myCharacterController != null && myCharacterController.velocity != Vector3.zero)
        {
            GUILayout.Label("current Velocity Vector : " + myCharacterController.velocity.ToString());
            GUILayout.Label("current Velocity Magnitude : " + myCharacterController.velocity.magnitude.ToString());
        }
    }
}
