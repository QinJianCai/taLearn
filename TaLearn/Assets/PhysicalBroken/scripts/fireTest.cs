using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireTest : MonoBehaviour
{
    List<ChunkNode> hitNode = new List<ChunkNode>(128);
    [SerializeField] public Transform barrelEnd;
    [SerializeField] public Transform b;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private RaycastHit hit;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
           Fire();
        }

        ///整体破坏
        if (Input.GetMouseButton(0))
        {
            Rigidbody r = null;
            // 主相机屏幕点转换为射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //射线碰到了物体
            if (Physics.Raycast(ray, out hit))
            {               
                var n1 = hit.collider.GetComponent<ChunkNode>();
                if (n1)
                {
                    var parent = GameObject.Find("Cube_Fracture").gameObject;
                    foreach (var n2 in parent.GetComponentsInChildren<ChunkNode>())
                    {
                        Debug.Log("1111111111111111"+ n2.name);
                        if (n2)
                        {
                            n2.Unfreeze();

                            n2.rb.mass = n2.boundSize * 1f;
                            r = n1.rb;
                            r.AddExplosionForce(10f, n2.col.bounds.center, 0.3f);
                        }
                    }
                    
                }
            }
        }
    }

    /// <summary>
    /// 局部破坏
    /// </summary>
    private void Fire()
    {
        hitNode.Clear();
       
        Vector3 targetPos;
        RaycastHit hit;
        bool hitChunk = false;
        // if (Physics.Raycast(ray, out hit, 100f))
        RaycastHit[] hits = Physics.SphereCastAll(barrelEnd.position, 0.2f, barrelEnd.forward, 100);
        if (hits.Length > 0)
        {
            for (var j = 0; j < hits.Length; j++)
            {
                var n1 = hits[j].collider.GetComponent<ChunkNode>();
                if (n1)
                {
                    hitNode.Add(n1);
                }
            }
            if (hitNode.Count > 0)
            {
                Rigidbody r = null;
                hitNode.Sort((a, b) => a.boundSize.CompareTo(b.boundSize));

                int c = hitNode.Count - 1;
                int cc = 0;

                for (int j = c; j > -1; j--)

                {
                    cc++;

                    var n = hitNode[j];
                    n.Unfreeze();

                    n.rb.mass = n.boundSize * 1f;
                    if (cc > 20)
                    {
                        n.gameObject.SetActive(false);
                    }
                    else
                    {
                        r = n.rb;   
                    } 
                }

                r.AddExplosionForce(10000f, hitNode[0].col.bounds.center, 0.3f);
            }
        }
    }
}
