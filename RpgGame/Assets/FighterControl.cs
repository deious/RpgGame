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

    [Header("애니메이션 관련 속성")]
    public AnimationClip IdleAnimClip = null;
    public AnimationClip WalkAnimClip = null;
    public AnimationClip RunAnimClip = null;
    public AnimationClip Attack1AnimClip = null;
    public AnimationClip Attack2AnimClip = null;
    public AnimationClip Attack3AnimClip = null;
    public AnimationClip Attack4AnimClip = null;
    private Animation myAnimation = null;

    public enum FighterState { None, Idle, Walk, Run, Attack, Skill }
    [Header("캐릭터 상태")]
    public FighterState myState = FighterState.None;

    // Start is called before the first frame update
    void Start()
    {
        myCharacterController = GetComponent<CharacterController>();

        myAnimation = GetComponent<Animation>();
        myAnimation.playAutomatically = false;
        myAnimation.Stop();

        myState = FighterState.Idle;
        myAnimation[IdleAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[WalkAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[RunAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[Attack1AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack2AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack3AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack4AnimClip.name].wrapMode = WrapMode.Once;
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        BodyDirectionChange();

        AnimationControl();

        CheckState();
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
        if (myState == FighterState.Run) speed = RunSpeed;

        Vector3 moveAmount = (MoveDirection * speed * Time.deltaTime);
        collisionFlags = myCharacterController.Move(moveAmount);
    }

    private void OnGUI()
    {
        GUILayout.Label("현재 속도 : " + GetVelocitySpeed().ToString());

        if(myCharacterController != null && myCharacterController.velocity != Vector3.zero)
        {
            GUILayout.Label("current Velocity Vector : " + myCharacterController.velocity.ToString());
            GUILayout.Label("current Velocity Magnitude : " + myCharacterController.velocity.magnitude.ToString());
        }
    }

    float GetVelocitySpeed()
    {
        if(myCharacterController.velocity == Vector3.zero)
        {
            CurrentVelocity = Vector3.zero;
        }
        else
        {
            Vector3 goalVelocity = myCharacterController.velocity;
            goalVelocity.y = 0.0f;
            CurrentVelocity = Vector3.Lerp(CurrentVelocity, goalVelocity, VelocityChangeSpeed * Time.fixedDeltaTime);
        }

        return CurrentVelocity.magnitude;
    }

    void BodyDirectionChange()
    {
        if(GetVelocitySpeed() > 0.0f)
        {
            Vector3 newFoward = myCharacterController.velocity;
            newFoward.y = 0.0f;
            transform.forward = Vector3.Lerp(transform.forward, newFoward, BodyRotateSpeed * Time.deltaTime);
        }
    }

    void AnimationPlay(AnimationClip clip)
    {
        myAnimation.clip = clip;
        myAnimation.CrossFade(clip.name);
    }

    void AnimationControl()
    {
        switch(myState)
        {
            case FighterState.Idle:
                AnimationPlay(IdleAnimClip);
                break;
            case FighterState.Walk:
                AnimationPlay(WalkAnimClip);
                break;
            case FighterState.Run:
                AnimationPlay(RunAnimClip);
                break;
            case FighterState.Attack:
                break;
            case FighterState.Skill:
                break;
        }
    }

    void CheckState()
    {
        float currentSpeed = GetVelocitySpeed();
        switch(myState)
        {
            case FighterState.Idle:
                if (currentSpeed > 0.0f) myState = FighterState.Walk;
                break;
            case FighterState.Walk:
                if (currentSpeed > 0.5f) myState = FighterState.Run;
                else if (currentSpeed < 0.01f) myState = FighterState.Idle;
                break;
            case FighterState.Run:
                if (currentSpeed < 0.5f) myState = FighterState.Walk;
                else if (currentSpeed < 0.01f) myState = FighterState.Idle;
                break;
            case FighterState.Attack:
                break;
            case FighterState.Skill:
                break;
        }
    }
}
