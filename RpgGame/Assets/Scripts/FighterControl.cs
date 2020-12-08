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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
