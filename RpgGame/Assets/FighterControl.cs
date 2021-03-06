﻿using System.Collections;
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
    private float gravity = 9.8f;           // 중력값
    private float verticalSpeed = 0.0f;     // 수직 속도
    private bool CannotMove = false;        // 이동 불가 플래그

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

    public enum FighterAttackState { Attack1, Attack2, Attack3, Attack4 }
    public FighterAttackState AttackState = FighterAttackState.Attack1;

    public bool NextAttack = false;             // 다음 공격 활성화 여부를 확인하는 플래그

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

        AddAnimationEvent(Attack1AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack2AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack3AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack4AnimClip, "OnAttackAnimFinished");
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        BodyDirectionChange();

        AnimationControl();

        CheckState();

        InputControl();

        ApplyGravity();
    }

    void Move()
    {
        if (CannotMove) return;

        Transform CameraTransform = Camera.main.transform;                              // 메인카메라 게임오브젝트의 트랜스폼 컴포넌트
        Vector3 forward = CameraTransform.TransformDirection(Vector3.forward);          // 카메라가 바라보는 방향이 월드상에서는 어떤 방향인지 얻어옴
        forward.y = 0.0f;
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

        float vetical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 targetDirection = horizontal * right + vetical * forward;               // 이동하고자 하는 방향

        MoveDirection = Vector3.RotateTowards(MoveDirection, targetDirection, DirectionRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        MoveDirection = MoveDirection.normalized;

        float speed = MoveSpeed;
        if (myState == FighterState.Run) speed = RunSpeed;

        Vector3 gravityVector = new Vector3(0.0f, verticalSpeed, 0.0f);

        Vector3 moveAmount = (MoveDirection * speed * Time.deltaTime) + gravityVector;                  // 이번 프레임에 움직일 양
        collisionFlags = myCharacterController.Move(moveAmount);                        // 실제 이동
    }

    private void OnGUI()
    {

        GUILayout.Label("충돌 :" + collisionFlags.ToString());
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
                AttackAnimationControl();
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
                CannotMove = true;
                break;
            case FighterState.Skill:
                CannotMove = true;
                break;
        }
    }

    void InputControl()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(myState != FighterState.Attack)
            {
                myState = FighterState.Attack;
                AttackState = FighterAttackState.Attack1;
            }
            else
            {
                switch(AttackState)
                {
                    case FighterAttackState.Attack1:
                        if (myAnimation[Attack1AnimClip.name].normalizedTime > 0.1f) NextAttack = true;
                        break;
                    case FighterAttackState.Attack2:
                        if (myAnimation[Attack2AnimClip.name].normalizedTime > 0.1f) NextAttack = true;
                        break;
                    case FighterAttackState.Attack3:
                        if (myAnimation[Attack3AnimClip.name].normalizedTime > 0.1f) NextAttack = true;
                        break;
                    case FighterAttackState.Attack4:
                        if (myAnimation[Attack4AnimClip.name].normalizedTime > 0.1f) NextAttack = true;
                        break;
                }
            }
        }
    }

    void OnAttackAnimFinished()
    {
        if(NextAttack)
        {
            NextAttack = false;

            switch(AttackState)
            {
                case FighterAttackState.Attack1:
                    AttackState = FighterAttackState.Attack2;
                    break;
                case FighterAttackState.Attack2:
                    AttackState = FighterAttackState.Attack3;
                    break;
                case FighterAttackState.Attack3:
                    AttackState = FighterAttackState.Attack4;
                    break;
                case FighterAttackState.Attack4:
                    AttackState = FighterAttackState.Attack1;
                    break;
            }
        }
        else
        {
            CannotMove = false;
            myState = FighterState.Idle;
            AttackState = FighterAttackState.Attack1;
        }
    }

    void AddAnimationEvent(AnimationClip clip, string FuncName)
    {
        AnimationEvent newEvent = new AnimationEvent();
        newEvent.functionName = FuncName;
        newEvent.time = clip.length - 0.1f;
        clip.AddEvent(newEvent);
    }

    void AttackAnimationControl()
    {
        switch(AttackState)
        {
            case FighterAttackState.Attack1:
                AnimationPlay(Attack1AnimClip);
                break;
            case FighterAttackState.Attack2:
                AnimationPlay(Attack2AnimClip);
                break;
            case FighterAttackState.Attack3:
                AnimationPlay(Attack3AnimClip);
                break;
            case FighterAttackState.Attack4:
                AnimationPlay(Attack4AnimClip);
                break;
        }
    }

    void ApplyGravity()
    {
        if ((collisionFlags & CollisionFlags.CollidedBelow) != 0) verticalSpeed = 0.0f;
        else verticalSpeed -= gravity * Time.deltaTime;
    }
}
