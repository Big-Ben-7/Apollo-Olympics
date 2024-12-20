using UnityEditor;
using UnityEngine;

public class ReplaceSkinnedMeshRenderers : EditorWindow
{
    public static void ReplaceInPrefabs()
    {
        // Define the folder to search (relative to the Assets folder)
        string targetFolder = "Assets/Prefabs/Interactables/Collectables/BadPerformance";

        // Get all prefab paths in the specified folder
        string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] {targetFolder});

        foreach (string prefabPath in prefabPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(prefabPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                // Open the prefab for editing
                GameObject prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);

                // Process the prefab instance
                SkinnedMeshRenderer[] skinnedRenderers = prefabInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinned in skinnedRenderers)
                {
                    GameObject obj = skinned.gameObject;

                    // Add a MeshFilter if it doesn't exist
                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter == null)
                        meshFilter = obj.AddComponent<MeshFilter>();

                    // Assign the shared mesh to the MeshFilter
                    meshFilter.sharedMesh = skinned.sharedMesh;

                    // Add a MeshRenderer if it doesn't exist
                    MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                    if (meshRenderer == null)
                        meshRenderer = obj.AddComponent<MeshRenderer>();

                    // Copy the materials from the SkinnedMeshRenderer
                    meshRenderer.sharedMaterials = skinned.sharedMaterials;

                    // Remove the SkinnedMeshRenderer
                    DestroyImmediate(skinned);
                }

                // Save the changes to the prefab
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }

        Debug.Log("Replaced all SkinnedMeshRenderers with MeshRenderers in prefabs.");
    }
}