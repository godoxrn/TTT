using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction*100f,Color.red,0.1f);
            if(Physics.Raycast(ray,out hit, 200.0f,( 1 << 6)))
            {
                hit.transform.GetComponent<Chess>().UpdateMe();
            }
        }
    }
}

