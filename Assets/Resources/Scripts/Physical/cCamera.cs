using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cCamera : MonoBehaviour {
    public cPhysicalObject target;
    public float viewDist;

    private void Start() {
        if ( target == null ) {
            target = FindObjectOfType<cPlayer>();
        }
        GetComponent<Camera>().orthographicSize = viewDist;
    }

    // Update is called once per frame
    void Update () {
        transform.position = target.transform.position;
    }
}
