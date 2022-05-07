using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broken : MonoBehaviour
{
    public BrokenObj _BrokenObj;
    [SerializeField] private Anchor anchor = Anchor.Bottom;
    [SerializeField] private int chunkCount = 500;
    [SerializeField] private float density = 50;
    [SerializeField] private float internalStrength = 100;

    System.Random rng = new System.Random();
    static HashSet<ChunkNode> overlapsChunk = new HashSet<ChunkNode>();

    // Start is called before the first frame update
    void Start()
    {
        BrokenGameObject();
        gameObject.SetActive(false);
    }

    void BrokenGameObject()
    {
        
        var seed = rng.Next();
        var mesh = GetWorldMesh(gameObject);
        NvBlastExtUnity.setSeed(seed);

        var nvMesh = new NvMesh(
                mesh.vertices,
                mesh.normals,
                mesh.uv,
                mesh.vertexCount,
                mesh.GetIndices(0),
                (int)mesh.GetIndexCount(0)
            );

        var meshes = BrokenMeshesWithNvblast(chunkCount, nvMesh);
        var chunkMass = GetMeshVolume(mesh) * density / chunkCount;
        var chunks = BuildChunks(meshes, chunkMass);
        foreach (var chunk in chunks)
        {
            ConnectTouchingChunks(chunk, internalStrength, 0.01f);
        }

        var fractureGo = new GameObject(gameObject.name+"_Fracture");
        var _ChunkMgr = fractureGo.AddComponent<ChunkMgr>();
        _ChunkMgr.nodes = chunks.ToArray();
        foreach (var chunk in chunks)
        {
            chunk.transform.SetParent(fractureGo.transform);
        }

        // Graph manager freezes/unfreezes blocks depending on whether they are connected to the graph or not
        _ChunkMgr.Setup();
    }

    private void ConnectTouchingChunks(ChunkNode chunk, float jointBreakForce, float touchRadius)
    {
        var mesh = chunk.mf.mesh;
        overlapsChunk.Clear();
        var vertices = mesh.vertices;
        for (var i = 0; i < vertices.Length; i++)
        {
            var worldPosition = chunk.transform.TransformPoint(vertices[i]);
            var hits = Physics.OverlapSphere(worldPosition, touchRadius, gameObject.layer);
            for (var j = 0; j < hits.Length; j++)
            {
                ChunkNode node = hits[j].GetComponent<ChunkNode>();
                overlapsChunk.Add(node);
            }
        }


        overlapsChunk.Remove(chunk);

        chunk.Neighbours = overlapsChunk.ToArray();
    }

    private static List<ChunkNode> BuildChunks(List<Mesh> meshes, float chunkMass)
    {
        return meshes.Select((chunkMesh, i) =>
        {
            var chunk = DoBuildChunk(chunkMesh, chunkMass);
            chunk.name += $" [{i}]";
            return chunk;
        }).ToList();
    }

    private static ChunkNode DoBuildChunk(Mesh mesh, float mass)
    {
        var chunk = new GameObject($"Chunk");
        var chunk2 = chunk.AddComponent<ChunkNode>();
        chunk2.render = chunk.AddComponent<MeshRenderer>();
       
        chunk2.mf = chunk.AddComponent<MeshFilter>();
        chunk2.mf.sharedMesh = mesh;

        chunk2.rb = chunk.AddComponent<Rigidbody>();
        chunk2.rb.mass = mass;

        chunk2.col = chunk.AddComponent<MeshCollider>();
        chunk2.col.convex = true;
        chunk2.GetComponent<MeshFilter>().sharedMesh = mesh;
        return chunk2;
    }

    float GetMeshVolume(Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var p1 = vertices[triangles[i + 0]];
            var p2 = vertices[triangles[i + 1]];
            var p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }
    private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    private static List<Mesh> BrokenMeshesWithNvblast(int totalChunks, NvMesh nvMesh)
    {
        var fractureTool = new NvFractureTool();
        fractureTool.setRemoveIslands(false);
        fractureTool.setSourceMesh(nvMesh);
        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(totalChunks);
        fractureTool.voronoiFracturing(0, sites);
        fractureTool.finalizeFracturing();

        var meshCount = fractureTool.getChunkCount();
        var meshes = new List<Mesh>(fractureTool.getChunkCount());

        for (var i = 1; i < meshCount; i++)
        {
            meshes.Add(GeneratorChunkMesh(fractureTool, i));
        }

        return meshes;
    }

    private static Mesh GeneratorChunkMesh(NvFractureTool fractureTool, int index)
    {
        var outside = fractureTool.getChunkMesh(index, false);
        var inside = fractureTool.getChunkMesh(index, true);
        var chunkMesh = outside.toUnityMesh();
        chunkMesh.subMeshCount = 2;
        chunkMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
        return chunkMesh;
    }

    private static Mesh GetWorldMesh(GameObject gameObject)
    {
        var combineInstances = gameObject
            .GetComponentsInChildren<MeshFilter>()
            .Where(mf => ValidateMesh(mf.mesh))
            .Select(mf => new CombineInstance()
            {
                mesh = mf.mesh,
                transform = mf.transform.localToWorldMatrix
            }).ToArray();

        var totalMesh = new Mesh();
        totalMesh.CombineMeshes(combineInstances, true);
        return totalMesh;
    }

    private static bool ValidateMesh(Mesh mesh)
    {
        if (mesh.isReadable == false)
        {
            Debug.LogError($"Mesh [{mesh}] has to be readable.");
            return false;
        }

        if (mesh.vertices == null || mesh.vertices.Length == 0)
        {
            Debug.LogError($"Mesh [{mesh}] does not have any vertices.");
            return false;
        }

        if (mesh.uv == null || mesh.uv.Length == 0)
        {
            Debug.LogError($"Mesh [{mesh}] does not have any uvs.");
            return false;
        }

        return true;
    }


}
