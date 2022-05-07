using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {

        var node = collision.transform.GetComponent<ChunkNode>();
        if (node != null)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
