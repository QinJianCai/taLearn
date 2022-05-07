using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkNodeGroup
{
    public List<ChunkNode> nodes;
    public bool leaved;
    public ChunkNode minHight;

    public static Stack<ChunkNodeGroup> NodePool = new Stack<ChunkNodeGroup>();

    public static ChunkNodeGroup Get()
    {
        if (NodePool.Count > 0)
        {
            var n = NodePool.Pop();
            n.leaved = false;
            n.minHight = null;
            return n;
        }
        else
        {
            var n = new ChunkNodeGroup();
            n.nodes = new List<ChunkNode>(512);
            return n;
        }
    }

    public void ReturnPool()
    {
        nodes.Clear();

        minHight = null;
        NodePool.Push(this);
    }


}

public class ChunkMgr : MonoBehaviour
{
    public ChunkNode[] nodes;
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
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            node.cm = this;
            node.Setup();
        }
    }

    public void resetPos()
    {
        
      
    }
}
