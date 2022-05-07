using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkNode : MonoBehaviour
{
    public bool resetPos;
    public bool leaved;
    //包围框的体积
    public float boundSize;
    public Bounds b;
    public ChunkMgr cm;
    public MeshCollider col;
    public BoxCollider boxcol;
    public Vector3 pos, rot;
    public bool outside;
    public ChunkNode[] Neighbours;
    //  [HideInInspector]



    public Rigidbody rb;
    public bool frozen;
    public MeshRenderer render;
    public MeshFilter mf;

    public float unfrozenTime;
    public bool searched;
    public float minH;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        pos = col.bounds.center + cm.transform.position;
        b = col.bounds;
        boundSize = col.bounds.size.x * col.bounds.size.y * col.bounds.size.z;
        minH = col.bounds.min.y;
        boxcol = gameObject.AddComponent<BoxCollider>();
        boxcol.center = col.bounds.center;
        boxcol.size = col.bounds.size;
        boxcol.enabled = false;
        
        resetPos = true;
        Freeze();
    }

    public void Freeze()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.Sleep();
        }

        frozen = true;
    }

    public void Unfreeze()
    {
        if (!frozen)
        {
            return;
        }
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.WakeUp();


        resetPos = false;       
        frozen = false;
    }

}
