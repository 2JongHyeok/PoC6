using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class WheelchairController : MonoBehaviour
{
    [Header("Camera Connection")]
    public WheelchairCamera camScript;

    [Header("Movement Settings")]
    
    [TabGroup("Setup")]
    [SerializeField]
    public float moveSpeed = 2500f;
    [TabGroup("Setup")]
    [SerializeField]
    public float turnSpeed = 1500f;

    [Header("Resistance")]
    [SerializeField]
    [HideIf(@"turnSpeed")]
    public float moveDamping = 2f;
    public float turnDamping = 5f;

    [Header("Stability")]
    public bool lockTilt = true;
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    [Header("Grandma Settings")]
    public GameObject grandmaRoot;
    public Rigidbody grandmaHips;
    public float crashThreshold = 25000f;
    public float safetyDelay = 1.0f;

    // [새로 추가됨] 사고 날 때 날아가는 힘 조절 (여기서 조절하세요!)
    [Header("Crash Settings (사고 강도 조절)")]
    [Tooltip("휠체어가 날아가는 힘 (기본값: 2000)")]
    public float wheelchairFlyForce = 2000f;

    [Tooltip("휠체어가 뱅글뱅글 도는 힘 (기본값: 1000)")]
    public float wheelchairSpinForce = 1000f;

    [Tooltip("할머니가 날아가는 힘 (기본값: 1500)")]
    public float grandmaFlyForce = 1500f;


    private Rigidbody rb;
    private Animator grandmaAnim;
    private Rigidbody[] grandmaLimbs;
    private Collider[] grandmaColliders;
    private Collider[] wheelchairColliders;
    private bool isSafetyPeriod = true;
    private bool isCrashed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 60f;
        wheelchairColliders = GetComponentsInChildren<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = centerOfMassOffset;

        if (grandmaRoot != null)
        {
            grandmaAnim = grandmaRoot.GetComponent<Animator>();
            grandmaLimbs = grandmaRoot.GetComponentsInChildren<Rigidbody>();
            grandmaColliders = grandmaRoot.GetComponentsInChildren<Collider>();

            grandmaRoot.transform.SetParent(this.transform);
            IgnoreCollisionBetweenGrandmaAndWheelchair(true);
            SetRagdollState(false);
        }

        StartCoroutine(DisableSafetyPeriod());
    }

    void FixedUpdate()
    {
        if (isCrashed) return;

        rb.linearDamping = moveDamping;
        rb.angularDamping = turnDamping;
        rb.centerOfMass = centerOfMassOffset;

        float leftInput = 0f;
        float rightInput = 0f;

        if (Input.GetKey(KeyCode.Q)) leftInput = 1f;
        else if (Input.GetKey(KeyCode.A)) leftInput = -1f;
        if (Input.GetKey(KeyCode.E)) rightInput = 1f;
        else if (Input.GetKey(KeyCode.D)) rightInput = -1f;

        float moveForce = (leftInput + rightInput) * moveSpeed;
        float turnTorque = (leftInput - rightInput) * turnSpeed;

        if (moveForce != 0) rb.AddRelativeForce(Vector3.forward * moveForce);
        if (turnTorque != 0) rb.AddRelativeTorque(Vector3.up * turnTorque);

        if (lockTilt && rb.constraints != RigidbodyConstraints.None)
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isSafetyPeriod || isCrashed) return;
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Road")) return;
        if (collision.transform.root == grandmaRoot.transform) return;

        float currentImpact = collision.impulse.magnitude;

        // 충격량이 크거나 OR Obstacle 태그면 사고
        if (currentImpact > crashThreshold || collision.gameObject.CompareTag("Obstacle"))
        {
            Crash(collision);
        }
    }

    void Crash(Collision collision)
    {
        isCrashed = true;

        // 1. 제약 해제
        lockTilt = false;
        rb.constraints = RigidbodyConstraints.None;

        // 2. 저항 제거 (잘 구르게)
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;
        rb.centerOfMass = Vector3.zero;

        // 3. 할머니 분리
        if (grandmaRoot != null)
        {
            grandmaRoot.transform.SetParent(null);
            SetRagdollState(true);
        }

        // 4. 카메라 전환
        if (camScript != null && grandmaHips != null)
        {
            camScript.FocusOnRagdoll(grandmaHips.transform);
        }

        Debug.Log("쾅! 사고 발생!");

        // 5. 힘 적용 (Inspector에서 설정한 변수 사용)
        Vector3 hitDir = collision.contacts[0].normal;

        // 할머니 날리기
        if (grandmaHips != null)
            grandmaHips.AddForce(Vector3.up * (grandmaFlyForce * 0.5f) + hitDir * grandmaFlyForce, ForceMode.Impulse);

        // 휠체어 날리기
        rb.AddForce(Vector3.up * (wheelchairFlyForce * 0.5f) + hitDir * wheelchairFlyForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * wheelchairSpinForce, ForceMode.Impulse);
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