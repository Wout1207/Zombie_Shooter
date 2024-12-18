using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickering : MonoBehaviour
{
    private float time;
    private float toggleDuration;
    private Light lightComponent;
    public GameObject lightOnView;

    // Start is called before the first frame update
    void Start()
    {
        time = Time.time;
        toggleDuration = Random.Range(0.05f, 0.5f);
        lightComponent = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > toggleDuration + time)
        {
            ToggleLight();
            toggleDuration = Random.Range(0.05f, 0.5f);
            time = Time.time;
        }
    }

    private void ToggleLight()
    {
        lightComponent.intensity = (lightComponent.intensity > 0) ? 0 : Random.Range(2f, 5f);
        if(lightOnView)
        {
            if (lightComponent.intensity == 0)
            {
                lightOnView.SetActive(false);
            }
            else
            {
                lightOnView.SetActive(true);
            }
        }
    }
}
