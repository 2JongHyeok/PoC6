using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WheelchairController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2500f;   // 전진 힘
    public float turnSpeed = 1500f;   // 회전 힘 (높을수록 제자리 턴이 빠름)

    [Header("Resistance")]
    public float moveDamping = 2f;    // 이동 저항
    public float turnDamping = 5f;    // 회전 저항

    [Header("Stability")]
    public bool lockTilt = true;      // 체크하면 절대 안 넘어짐 (X, Z 회전 강제 고정)

    [Header("Grandma Settings")]
    public GameObject grandmaRoot;
    public Rigidbody grandmaHips;
    public float crashThreshold = 25000f;
    public float safetyDelay = 1.0f;

    private Rigidbody rb;
    private Animator grandmaAnim;
    private Rigidbody[] grandmaLimbs;
    private Collider[] grandmaColliders;
    private Collider[] wheelchairColliders;
    private bool isSafetyPeriod = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 60f;
        wheelchairColliders = GetComponentsInChildren<Collider>();

        // 1. 기본적으로 물리 엔진의 회전 잠금 사용
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (grandmaRoot != null)
        {
            grandmaAnim = grandmaRoot.GetComponent<Animator>();
            grandmaLimbs = grandmaRoot.GetComponentsInChildren<Rigidbody>();
            grandmaColliders = grandmaRoot.GetComponentsInChildren<Collider>();

            // 할머니 입양 & 충돌 무시
            grandmaRoot.transform.SetParent(this.transform);
            IgnoreCollisionBetweenGrandmaAndWheelchair(true);
            SetRagdollState(false);
        }

        StartCoroutine(DisableSafetyPeriod());
    }

    void FixedUpdate()
    {
        // 물리 설정 갱신
        rb.linearDamping = moveDamping;
        rb.angularDamping = turnDamping;

        // [핵심 1] 입력 계산 (탱크 컨트롤 로직)
        float leftInput = 0f;
        float rightInput = 0f;

        if (Input.GetKey(KeyCode.Q)) leftInput = 1f;
        else if (Input.GetKey(KeyCode.A)) leftInput = -1f;

        if (Input.GetKey(KeyCode.E)) rightInput = 1f;
        else if (Input.GetKey(KeyCode.D)) rightInput = -1f;

        // 이동 입력 = (왼쪽 + 오른쪽)
        // 예: 둘 다 1이면 전진(2), 하나만 1이면 전진(1), 서로 반대면(1, -1) 전진(0)
        float moveForce = (leftInput + rightInput) * moveSpeed;

        // 회전 입력 = (왼쪽 - 오른쪽)
        // 예: 왼쪽만(1) -> 1(우회전), 오른쪽만(1) -> -1(좌회전), 서로 반대(1, -1) -> 2(급회전)
        float turnTorque = (leftInput - rightInput) * turnSpeed;

        // [핵심 2] 힘과 토크를 분리해서 적용 (안정성 최고)
        // 이제 바퀴 위치(Transform)가 필요 없습니다. 중심에서 힘이 나갑니다.
        if (moveForce != 0)
            rb.AddRelativeForce(Vector3.forward * moveForce);

        if (turnTorque != 0)
            rb.AddRelativeTorque(Vector3.up * turnTorque);

        // [핵심 3] 강제 오뚝이 기능 (비틀어짐 완전 차단)
        // 회전하다가 X, Z축이 조금이라도 틀어지면 강제로 0으로 맞춥니다.
        if (lockTilt && rb.constraints != RigidbodyConstraints.None)
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isSafetyPeriod) return;
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Road")) return;
        if (collision.transform.root == grandmaRoot.transform) return;

        float currentImpact = collision.impulse.magnitude;

        if (currentImpact > crashThreshold)
        {
            // 사고 발생! 오뚝이 기능 해제 & 물리 잠금 해제 -> 구르기 시작
            lockTilt = false;
            rb.constraints = RigidbodyConstraints.None;

            grandmaRoot.transform.SetParent(null);
            SetRagdollState(true);

            Debug.Log("쾅! 대형 사고! 할머니 사출!");

            if (grandmaHips != null)
                grandmaHips.AddForce(Vector3.up * 500f + transform.forward * 500f, ForceMode.Impulse);
        }
    }

    void IgnoreCollisionBetweenGrandmaAndWheelchair(bool ignore)
    {
        if (grandmaColliders == null || wheelchairColliders == null) return;
        foreach (var grandmaCol in grandmaColliders)
        {
            foreach (var wheelchairCol in wheelchairColliders)
            {
                Physics.IgnoreCollision(grandmaCol, wheelchairCol, ignore);
            }
        }
    }

    void SetRagdollState(bool isRagdoll)
    {
        if (grandmaAnim != null)
            grandmaAnim.enabled = !isRagdoll;

        foreach (var limb in grandmaLimbs)
        {
            limb.isKinematic = !isRagdoll;
        }
    }

    IEnumerator DisableSafetyPeriod()
    {
        yield return new WaitForSeconds(safetyDelay);
        isSafetyPeriod = false;
    }
}