using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DrawSector : MonoBehaviour
{
    [Header("[��������]")]
    public float radius; // �뾶
    GameObject origin; // ԭ��GameObject����ʾԭ��λ���ã�
    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        origin = gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        material.SetFloat("_Radius", radius);
        material.SetFloat("_CenterPosX", transform.position.x);
        material.SetFloat("_CenterPosY", transform.position.y);
        material.SetFloat("_CenterPosZ", transform.position.z);
    }
}