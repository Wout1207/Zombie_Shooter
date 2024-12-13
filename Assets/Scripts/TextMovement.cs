using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMovement : MonoBehaviour
{
    public float speed;
    public float distance;
    public float startY;
    // Start is called before the first frame update
    void Start()
    {
        startY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0, speed * Time.deltaTime), 0);
        if (Mathf.Abs(transform.position.y - startY) >= distance)
        {
            Destroy(gameObject);
        }
    }
}
