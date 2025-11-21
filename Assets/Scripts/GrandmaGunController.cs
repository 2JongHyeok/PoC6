using UnityEngine;
using System.Collections;

public class GrandmaGunController : MonoBehaviour
{
    [Header("Aiming Settings (Á¶ÁØ)")]
    public Transform spineBone;
    public Transform rightArmBone;
    public Transform wheelchairRoot;
    public LayerMask groundLayer; // ¹Ù´Ú¸¸ Âï´Â ·¹ÀÌ¾î (Mouse Aim¿ë)

    [Header("Visuals (½Ã°¢ È¿°ú)")]
    public Transform aimDot; // [ÇÊ¼ö] ¾Æ±î ¸¸µç »¡°£ Á¡(AimDot) ¿¬°á

    [Header("Rotation Limits")]
    [Range(0, 180)]
    public float maxTurnAngle = 90f;
    public Vector3 spineOffset = new Vector3(0, 0, 0);
    public Vector3 armOffset = new Vector3(0, 90, 0);

    [Header("Shooting Settings")]
    public Transform firePoint;
    public float fireRate = 0.2f;
    public LayerMask hitLayer; // [Áß¿ä] ÀûÀ» ¸ÂÃâ ¼ö ÀÖ´Â ·¹ÀÌ¾î (Default, Enemy µî)

    private float nextFireTime = 0f;
    private Camera mainCam;
    private bool isAiming = false;
    private Vector3 lastAimPoint;
    private bool hasAimPoint = false;

    void Start()
    {
        mainCam = Camera.main;
        if (wheelchairRoot == null) wheelchairRoot = transform.root;

        // ½ÃÀÛÇÒ ¶§ Á¶ÁØÁ¡ ¼û±â±â
        if (aimDot != null) aimDot.gameObject.SetActive(false);
    }

    void Update()
    {
        // ¿ìÅ¬¸¯ À¯Áö -> Á¶ÁØ ¸ðµå
        isAiming = Input.GetMouseButton(1);

        // Á¶ÁØÁ¡ ÄÑ±â/²ô±â
        if (aimDot != null) aimDot.gameObject.SetActive(isAiming);

        // Á¶ÁØ Áß¿¡¸¸ ¹ß»ç °¡´É
        if (isAiming && Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void LateUpdate()
    {
        if (isAiming)
        {
            AimTowardsMouse();
        }
    }

    void AimTowardsMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ¸¶¿ì½º°¡ ¹Ù´Ú(Ground)ÀÌ³ª Àû µîÀ» °¡¸®Å°´ÂÁö È®ÀÎ
        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            Vector3 targetPoint = hit.point;

            lastAimPoint = targetPoint;
            hasAimPoint = true;

            // [Ãß°¡µÊ] Á¶ÁØÁ¡(»¡°£ Á¡)À» ¸¶¿ì½º À§Ä¡·Î ÀÌµ¿
            if (aimDot != null)
            {
                // »ìÂ¦ ¶ç¿ö¼­ ¹Ù´Ú¿¡ ÆÄ¹¯È÷Áö ¾Ê°Ô ÇÔ
                aimDot.position = targetPoint + Vector3.up * 0.05f;
            }

            // === °¢µµ Á¦ÇÑ ·ÎÁ÷ (ÀÌÀü°ú µ¿ÀÏ) ===
            Vector3 directionToTarget = targetPoint - wheelchairRoot.position;
            directionToTarget.y = 0;
            float angle = Vector3.SignedAngle(wheelchairRoot.forward, directionToTarget, Vector3.up);

            if (Mathf.Abs(angle) > maxTurnAngle)
            {
                float clampedAngle = Mathf.Sign(angle) * maxTurnAngle;
                Quaternion rotation = Quaternion.AngleAxis(clampedAngle, Vector3.up);
                Vector3 clampedDir = rotation * wheelchairRoot.forward;
                targetPoint = wheelchairRoot.position + clampedDir * 10f;
            }

            // Çã¸® È¸Àü
            if (spineBone != null)
            {
                Vector3 spineTarget = new Vector3(targetPoint.x, spineBone.position.y, targetPoint.z);
                spineBone.LookAt(spineTarget);
                spineBone.Rotate(spineOffset);
            }

            // ÆÈ È¸Àü
            if (rightArmBone != null)
            {
                rightArmBone.LookAt(targetPoint);
                rightArmBone.Rotate(armOffset);
            }
        }
        else
        {
            hasAimPoint = false;
        }
    }

    void Shoot()
    {
        // ÃÑ¾ËÀº Áï¹ß(Hitscan)ÀÌ¹Ç·Î ·¹ÀÌÄ³½ºÆ®·Î ÆÇÁ¤
        if (!hasAimPoint)
        {
            Debug.LogWarning("Shoot called without a valid aim target; falling back to firePoint forward.");
        }

        Vector3 rayDirection = hasAimPoint
            ? (lastAimPoint - firePoint.position).normalized
            : firePoint.forward;

        if (rayDirection == Vector3.zero)
        {
            rayDirection = firePoint.forward;
        }

        RaycastHit hit;

        Debug.DrawRay(firePoint.position, rayDirection * 100f, Color.green, 1f);
        // ÃÑ±¸ À§Ä¡¿¡¼­ Á¤¸éÀ¸·Î ·¹ÀÌ¸¦ ½ô
        if (Physics.Raycast(firePoint.position, rayDirection, out hit, 100f, hitLayer))
        {
            // µð¹ö±×¿ë: ¸ÂÀº °÷¿¡ 2ÃÊ°£ »¡°£ ¼± Ç¥½Ã (Åºµµ È®ÀÎ¿ë)
            Debug.DrawLine(firePoint.position, hit.point, Color.red, 2f);

            // ¸ÂÀº ¹°Ã¼¿¡¼­ SimpleEnemy ½ºÅ©¸³Æ® Ã£±â
            // (ÀûÀÇ ÆÈ, ´Ù¸® ¾îµð¸¦ ¸Â¾Æµµ ºÎ¸ð¿¡ ÀÖ´Â ½ºÅ©¸³Æ®¸¦ Ã£À½)
            SimpleEnemy enemy = hit.transform.GetComponentInParent<SimpleEnemy>();

            if (enemy != null)
            {
                enemy.Die(); // Àû »ç¸Á ÇÔ¼ö È£Ãâ
                Debug.Log("Çìµå¼¦! Àû Ã³Ä¡!");
            }
            else
            {
                Debug.Log("ºø³ª°¨: " + hit.collider.name);
            }
        }
    }
}
