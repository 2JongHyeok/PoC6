using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RealtimeWheelchairController : MonoBehaviour
{
    [Header("Wheel Force Positions")]
    public Transform leftWheelTr;
    public Transform rightWheelTr;

    [Header("Movement Settings")]
    public float accelerationForce = 1000f;
    public float turnDamping = 5f;
    public float moveDamping = 2f;

    [Header("Stability (실시간 조절 가능)")]
    // 값을 바꾸면 빨간 공이 바로 움직일 겁니다.
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    [Header("Ragdoll Connection")]
    public GameObject grandmaRoot;
    public float crashThreshold = 5000f;
    private FixedJoint seatBelt;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 60f;

        // 초기화
        UpdatePhysicsSettings();

        if (grandmaRoot != null)
        {
            seatBelt = grandmaRoot.AddComponent<FixedJoint>();
            seatBelt.connectedBody = rb;
        }
    }

    void FixedUpdate()
    {
        // [중요] 튜닝을 위해 매 프레임 설정을 갱신합니다. 
        // (게임 완성 후에는 성능을 위해 이 줄을 지우거나 Start로 옮기세요)
        UpdatePhysicsSettings();

        float leftInput = 0f;
        float rightInput = 0f;

        if (Input.GetKey(KeyCode.Q)) leftInput = 1f;
        else if (Input.GetKey(KeyCode.A)) leftInput = -1f;

        if (Input.GetKey(KeyCode.E)) rightInput = 1f;
        else if (Input.GetKey(KeyCode.D)) rightInput = -1f;

        if (leftWheelTr != null)
        {
            rb.AddForceAtPosition(leftWheelTr.forward * leftInput * accelerationForce, leftWheelTr.position);
        }

        if (rightWheelTr != null)
        {
            rb.AddForceAtPosition(rightWheelTr.forward * rightInput * accelerationForce, rightWheelTr.position);
        }
    }

    // 물리 설정을 적용하는 함수
    void UpdatePhysicsSettings()
    {
        if (rb == null) return;

        rb.linearDamping = moveDamping;
        rb.angularDamping = turnDamping;

        // 무게 중심 계속 업데이트
        rb.centerOfMass = centerOfMassOffset;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (seatBelt != null && collision.impulse.magnitude > crashThreshold)
        {
            Destroy(seatBelt);
            seatBelt = null;
            Debug.Log("교통사고 발생! 할머니 사출!");
        }
    }

    void OnDrawGizmos()
    {
        // [수정됨] 이제 리지드바디 값이 아니라, 당신이 설정한 변수 위치를 직접 그립니다.
        // 따라서 에디터에서도 빨간 공이 움직이는 게 보일 겁니다.
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(centerOfMassOffset), 0.2f);

        Gizmos.color = Color.blue;
        if (leftWheelTr != null) Gizmos.DrawSphere(leftWheelTr.position, 0.1f);
        if (rightWheelTr != null) Gizmos.DrawSphere(rightWheelTr.position, 0.1f);
    }
}