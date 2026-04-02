using UnityEngine;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    [Header("슬라임 구성 요소")]
    [Tooltip("가운데 구슬을 포함한 모든 7개의 Rigidbody2D를 넣어주세요.")]
    public List<Rigidbody2D> allNodes;
    [Tooltip("바깥쪽 6개 구슬에 달린 GroundCheck 스크립트를 넣어주세요.")]
    public List<PlayerGroundCheck> groundChecks;

    [Header("이동 및 점프 설정")]
    public float moveForce = 10f;       // 좌우 이동 힘
    public float maxSpeed = 5f;         // 최대 좌우 이동 속도
    public float jumpImpulse = 15f;     // 바닥 점프력

    [Header("미끄러짐 설정")]
    [Range(0f, 1f)]
    [Tooltip("1에 가까울수록 얼음판처럼 계속 미끄러지고, 0에 가까울수록 팍 멈춥니다.")]
    public float groundFriction = 0.9f; // 추천 값: 0.85 ~ 0.95 사이

    [Header("벽 타기 및 벽 점프 설정")]
    public float minWallSlideSpeed = 1f; // 처음 벽에 붙었을 때 미끄러지는 속도
    public float maxWallSlideSpeed = 8f; // 최대 미끄러짐 속도
    public float wallAccelerationTime = 2f; // 최대 속도에 도달하는 시간
    public Vector2 wallJumpForce = new Vector2(10f, 15f); // 벽 점프 힘 (X, Y)

    private bool isGrounded;
    private bool isWallSliding;
    private int currentWallDir; // 1: 오른쪽 벽, -1: 왼쪽 벽
    private float wallSlideTimer = 0f;
    private Rigidbody2D coreRb;

    private void Awake()
    {
        coreRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckStates();
        HandleJump();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleWallSlide();
    }

    // 바깥쪽 구슬들의 상태를 취합하여 현재 슬라임의 전체 상태 판단
    private void CheckStates()
    {
        isGrounded = false;
        isWallSliding = false;
        currentWallDir = 0;

        foreach (var node in groundChecks)
        {
            if (node.isTouchingGround) isGrounded = true;

            // 바닥에 닿지 않았는데 벽에 닿아있다면 벽 타기 상태
            if (!isGrounded && node.isTouchingWall)
            {
                isWallSliding = true;
                currentWallDir = node.wallDirection;
            }
        }
    }

    [Header("감속 설정")] // 스크립트 위쪽 변수 선언부에 추가해 주세요.
    public float deceleration = 15f; // 방향키를 떼었을 때 멈추는 속도 (높을수록 팍! 멈춤)

    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (!isWallSliding)
        {
            // 1. 방향키를 누르고 있을 때 (가속)
            if (Mathf.Abs(moveInput) > 0.1f)
            {
                coreRb.AddForce(Vector2.right * moveInput * moveForce);

                // velocity 대신 linearVelocity 사용
                if (Mathf.Abs(coreRb.linearVelocity.x) > maxSpeed)
                {
                    coreRb.linearVelocity = new Vector2(Mathf.Sign(coreRb.linearVelocity.x) * maxSpeed, coreRb.linearVelocity.y);
                }
            }
            // 2. 방향키에서 손을 떼었을 때 (자연스러운 미끄러짐)
            else
            {
                // 슬라임 전체 구슬에 자연스러운 마찰력을 적용합니다.
                foreach (var rb in allNodes)
                {
                    // velocity 대신 linearVelocity 사용
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x * groundFriction, rb.linearVelocity.y);
                }
            }
        }
    }

    private void HandleWallSlide()
    {
        if (isWallSliding)
        {
            wallSlideTimer += Time.fixedDeltaTime;

            // 시간에 따라 미끄러지는 속도 점진적 증가
            float currentSlideSpeed = Mathf.Lerp(minWallSlideSpeed, maxWallSlideSpeed, wallSlideTimer / wallAccelerationTime);

            // 🌟 핵심: 슬라임 모양이 길쭉하게 찢어지지 않도록 모든 구슬의 낙하 속도를 동일하게 강제 제어
            foreach (var rb in allNodes)
            {
                // 현재 Y축 속도가 내려가는 중(-방향)이고, 그 속도가 제한 속도보다 빠르다면 속도 클램핑
                if (rb.linearVelocity.y < -currentSlideSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -currentSlideSpeed);
                }
            }
        }
        else
        {
            // 벽에서 떨어지면 타이머 초기화
            wallSlideTimer = 0f;
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // 일반 점프
                ApplyForceToAllNodes(new Vector2(0f, jumpImpulse));
            }
            else if (isWallSliding)
            {
                // 벽 점프 (벽의 반대 방향 대각선 위로 튕겨나감)
                Vector2 jumpDir = new Vector2(-currentWallDir * wallJumpForce.x, wallJumpForce.y);
                ApplyForceToAllNodes(jumpDir);

                // 점프 즉시 벽 타기 판정을 풀기 위해 타이머 초기화
                isWallSliding = false;
            }
        }
    }

    // 슬라임 전체가 한 덩어리처럼 점프하도록 모든 구슬에 동일한 충격량 적용
    private void ApplyForceToAllNodes(Vector2 force)
    {
        foreach (var rb in allNodes)
        {
            // 힘을 가하기 전 기존 Y축 속도를 초기화해야 일관된 점프 높이가 나옵니다.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}