using UnityEngine;
using System.Collections.Generic;

public class SelectiveTreeCollision : MonoBehaviour
{
    public Terrain terrain;

    // Set this to the indices of trees you want colliders for
    // You'll see how to find these numbers in Step 3
    public List<int> treeTypesWithCollision = new List<int>();

    // Minimum size to create collisions for
    public float minTreeHeight = 2.0f;

    void Start()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        CreateSelectiveColliders();
    }

    void CreateSelectiveColliders()
    {
        // Create a parent object to keep things organized
        GameObject parent = new GameObject("TreeColliders");
        parent.transform.parent = terrain.transform;

        // Get all the trees you painted
        TreeInstance[] trees = terrain.terrainData.treeInstances;
        int colliderCount = 0;

        // For each tree, create an invisible cylinder
        foreach (TreeInstance tree in trees)
        {
            // ONLY create colliders for tree types in our list (not grass)
            if (!treeTypesWithCollision.Contains(tree.prototypeIndex))
                continue;

            // Skip small trees (like grass)
            if (tree.heightScale < minTreeHeight)
                continue;

            // Convert tree position to world space
            Vector3 position = Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.transform.position;

            // Create a game object for collision
            GameObject collider = new GameObject("TreeCollider");
            collider.transform.parent = parent.transform;
            collider.transform.position = position;

            // Add a capsule collider sized like a tree
            CapsuleCollider capsule = collider.AddComponent<CapsuleCollider>();
            capsule.radius = 0.5f * tree.widthScale;
            capsule.height = 5f * tree.heightScale; // Approximate tree height
            capsule.center = new Vector3(0, capsule.height / 2, 0);

            colliderCount++;
        }

        Debug.Log("Created " + colliderCount + " tree colliders");
    }
}
