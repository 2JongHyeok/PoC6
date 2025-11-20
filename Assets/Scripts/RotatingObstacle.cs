using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    [Tooltip("회전 속도 (높을수록 세게 때림)")]
    public float rotationSpeed = 100f;

    void FixedUpdate()
    {
        // Y축을 기준으로 빙글빙글 돌립니다.
        // 물리 충돌을 위해 transform.Rotate 대신 Rigidbody를 쓰면 더 좋지만,
        // Kinematic 상태에서는 트랜스폼 회전도 물리 엔진이 "무한한 힘"으로 인식합니다.
        transform.Rotate(Vector3.up * rotationSpeed * Time.fixedDeltaTime);
    }
}