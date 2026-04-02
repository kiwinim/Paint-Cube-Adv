using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    [Header("상태 정보 (읽기 전용)")]
    public bool isTouchingGround = false;
    public bool isTouchingWall = false;
    public int wallDirection = 0; // -1: 왼쪽 벽, 1: 오른쪽 벽

    private void OnCollisionStay2D(Collision2D collision)
    {
        isTouchingGround = false;
        isTouchingWall = false;
        wallDirection = 0;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            // 바닥 판정 (Y축 위쪽을 바라보는 노말)
            if (normal.y > 0.5f)
            {
                isTouchingGround = true;
            }
            // 벽 판정 (X축 기울기가 큰 경우)
            else if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
            {
                isTouchingWall = true;
                // 노말 벡터는 부딪힌 면의 수직 방향 (오른쪽 벽에 부딪히면 노말은 왼쪽(-1))
                wallDirection = normal.x < 0 ? 1 : -1;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 떨어지면 상태 초기화
        isTouchingGround = false;
        isTouchingWall = false;
        wallDirection = 0;
    }
}