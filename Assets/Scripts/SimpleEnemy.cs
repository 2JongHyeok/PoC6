using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    private Animator anim;
    private Rigidbody[] limbs; // 적의 모든 관절

    void Start()
    {
        anim = GetComponent<Animator>();
        limbs = GetComponentsInChildren<Rigidbody>();

        // 처음엔 살아있으므로 레그돌 끄기 (Kinematic 켜기)
        foreach (var limb in limbs) limb.isKinematic = true;
    }

    // 총에 맞았을 때 호출될 함수
    public void Die()
    {
        // 1. 애니메이터 끔 (애니메이션 중단)
        if (anim != null) anim.enabled = false;

        // 2. 레그돌 켬 (물리 적용 -> 쓰러짐)
        foreach (var limb in limbs)
        {
            limb.isKinematic = false;
            // 약간 뒤로 날아가게 충격 주기
            limb.AddForce(Vector3.forward * 50f, ForceMode.Impulse);
        }

        // 3. 더 이상 적(Enemy)으로 인식되지 않게 태그나 컴포넌트 제거 (선택사항)
        Destroy(this); // 스크립트만 제거해서 시체는 남김
        Debug.Log("으악! 적 사망!");
    }
}