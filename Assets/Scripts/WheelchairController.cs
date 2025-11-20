using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WheelchairController : MonoBehaviour
{
    [Header("Camera Connection (필수)")]
    public WheelchairCamera camScript; // 방금 만든 카메라 스크립트를 여기에 연결하세요!

    [Header("Movement Settings")]
    public float moveSpeed = 2500f;
    public float turnSpeed = 1500f;

    [Header("Resistance")]
    public float moveDamping = 2f;
    public float turnDamping = 5f;

    [Header("Stability")]
    public bool lockTilt = true;

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

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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
        rb.linearDamping = moveDamping;
        rb.angularDamping = turnDamping;

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
        if (isSafetyPeriod) return;
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Road")) return;
        if (collision.transform.root == grandmaRoot.transform) return;

        float currentImpact = collision.impulse.magnitude;

        if (currentImpact > crashThreshold)
        {
            // [사고 발생]
            lockTilt = false;
            rb.constraints = RigidbodyConstraints.None;
            grandmaRoot.transform.SetParent(null);
            SetRagdollState(true);

            // [핵심 추가] 카메라에게 할머니 엉덩이를 따라가라고 명령!
            if (camScript != null && grandmaHips != null)
            {
                camScript.FocusOnRagdoll(grandmaHips.transform);
            }

            Debug.Log("쾅! 할머니 사출! 카메라 전환!");

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