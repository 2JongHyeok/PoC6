using UnityEngine;

public class WheelchairCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // 추적 대상 (휠체어)

    [Header("Orbit Settings (마우스 조작)")]
    public float mouseSensitivity = 3.0f; // 마우스 감도
    public float distanceFromTarget = 5.0f; // 타겟과의 거리
    public Vector2 pitchMinMax = new Vector2(-20, 85); // 위아래 회전 제한 (땅 뚫기 방지)
    public float heightOffset = 1.5f; // 타겟의 발바닥이 아닌 허리/머리 쯤을 바라보게 함

    [Header("Zoom Settings")]
    public float scrollSensitivity = 2.0f; // 휠 줌 속도
    public Vector2 zoomMinMax = new Vector2(2f, 10f); // 줌 최소/최대 거리

    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f; // 따라가는 부드러움 정도

    // 내부 변수
    private float yaw;   // 가로 회전 (Y축)
    private float pitch; // 세로 회전 (X축)
    private Vector3 currentVelocity; // SmoothDamp용 변수

    void Start()
    {
        // 시작할 때 현재 카메라 각도를 가져옴
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // 마우스 커서를 화면에 가두고 숨김 (ESC 누르면 풀림)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 마우스 입력 받기
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. 위아래 회전 제한 (너무 위나 아래로 못 가게)
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // 3. 마우스 휠로 거리 조절 (줌)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distanceFromTarget -= scroll * scrollSensitivity;
        distanceFromTarget = Mathf.Clamp(distanceFromTarget, zoomMinMax.x, zoomMinMax.y);

        // 4. 회전 계산 (Euler -> Quaternion)
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 5. 위치 계산
        // 타겟 위치에서 heightOffset만큼 위로 올린 지점을 중심으로 회전
        Vector3 targetCenter = target.position + Vector3.up * heightOffset;

        // 타겟 중심에서 회전값만큼 뒤로 물러난 위치
        Vector3 desiredPosition = targetCenter - (rotation * Vector3.forward * distanceFromTarget);

        // 6. 부드럽게 이동 & 회전 적용
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);
        transform.rotation = rotation; // 카메라는 항상 타겟 쪽을 바라봄 (회전값 그대로 적용)
    }

    // [외부 호출용] 사고 났을 때 할머니 따라가기
    public void FocusOnRagdoll(Transform newTarget)
    {
        target = newTarget;
        // 할머니는 작으니까 높이 오프셋을 조금 낮춰줌 (선택사항)
        heightOffset = 0.5f;
    }
}