using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnim : MonoBehaviour
{

    [SerializeField] private float recoilAmount = 0.2f;
    [SerializeField] private float recoilSmoothness = 5f;

    [HideInInspector] public bool isRecoiling = false;
    private Vector3 currentRecoil = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        applyRecoil();
    }

    private void applyRecoil()
    {
        Vector3 targetRecoil = Vector3.zero;

        if (isRecoiling )
        {
            targetRecoil = new Vector3(0, 0, -recoilAmount);

            if (Vector3.Distance(currentRecoil, targetRecoil) < 0.1f)
            {
                isRecoiling = false;
            } 
        }

        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilSmoothness);
        transform.localPosition -= currentRecoil;
    }
}
