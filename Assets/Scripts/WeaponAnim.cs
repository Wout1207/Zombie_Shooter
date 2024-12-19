using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnim : MonoBehaviour
{
    [SerializeField] private float recoilAmount = 0.1f;
    [SerializeField] private float recoilBackSpeed = 20f;
    [SerializeField] private float recoilReturnSpeed = 20f;

    private bool isRecoiling = false;
    private Vector3 currentRecoil = Vector3.zero;
    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyRecoil();
    }

    public void TriggerRecoil()
    {
        isRecoiling = true;
    }

    private void ApplyRecoil()
    {
        Vector3 targetRecoil = Vector3.zero;

        if (isRecoiling)
        {
            targetRecoil = new Vector3(0, 0, -recoilAmount); // Visual backward movement

            if (Vector3.Distance(currentRecoil, targetRecoil) < 0.01f)
            {
                isRecoiling = false; // Begin returning to original position
            }

            currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilBackSpeed);
        }
        else
        {
            // Return to initial position
            currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
        }

        transform.localPosition = initialPosition + currentRecoil;
    }
}
