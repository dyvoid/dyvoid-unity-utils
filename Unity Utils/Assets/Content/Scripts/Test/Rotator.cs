using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 _eulers;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(_eulers * Time.deltaTime);
    }
}
