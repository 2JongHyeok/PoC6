using UnityEngine;
using System.Collections;

public class GrandmaGunController : MonoBehaviour
{
    [Header("Aiming Settings (Á¶ÁØ)")]
    public Transform spineBone;
    public Transform rightArmBone;
    public Transform wheelchairRoot;
    public LayerMask groundLayer; 

    [Header("Visuals (½Ã°¢ È¿°ú)")]
    public Transform aimDot; 

    [Header("Rotation Limits")]
    [Range(0, 180)]
    public float maxTurnAngle = 90f;
    public Vector3 spineOffset = new Vector3(0, 0, 0);
    public Vector3 armOffset = new Vector3(0, 90, 0);

    [Header("Shooting Settings")]
    public Transform firePoint;
    public float fireRate = 0.2f;
    public LayerMask hitLayer;

    private float nextFireTime = 0f;
    private Camera mainCam;
    private bool isAiming = false;
    private Vector3 lastAimPoint;
    private bool hasAimPoint = false;

    void Start()
    {
        mainCam = Camera.main;
        if (wheelchairRoot == null) wheelchairRoot = transform.root;

     
        if (aimDot != null) aimDot.gameObject.SetActive(false);
    }

    void Update()
    {

        isAiming = Input.GetMouseButton(1);


        if (aimDot != null) aimDot.gameObject.SetActive(isAiming);


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

        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            Vector3 targetPoint = hit.point;

            if (spineBone != null)
            {
                Vector3 spineTarget = new Vector3(targetPoint.x, spineBone.position.y, targetPoint.z);
                spineBone.LookAt(spineTarget);
                spineBone.Rotate(spineOffset);
            }

            if (rightArmBone != null)
            {
                rightArmBone.LookAt(targetPoint);
                rightArmBone.Rotate(armOffset);
            }

            lastAimPoint = targetPoint;
            hasAimPoint = true;

            if (aimDot != null)
            {
                aimDot.position = targetPoint + Vector3.up * 0.05f;
                aimDot.gameObject.SetActive(true);
            }
        }
        else
        {
            hasAimPoint = false;
            if (aimDot != null)
            {
                aimDot.gameObject.SetActive(false);
            }
        }
    }

    void Shoot()
    {

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
        if (Physics.Raycast(firePoint.position, rayDirection, out hit, 100f, hitLayer))
        {

            Debug.DrawLine(firePoint.position, hit.point, Color.red, 2f);


            SimpleEnemy enemy = hit.transform.GetComponentInParent<SimpleEnemy>();

            if (enemy != null)
            {
                enemy.Die();
                Debug.Log("Çìµå¼¦! Àû Ã³Ä¡!");
            }
            else
            {
                Debug.Log("ºø³ª°¨: " + hit.collider.name);
            }
        }
    }
}
