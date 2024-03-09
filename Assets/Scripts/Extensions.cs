using UnityEngine;

public static class Extensions
{
    public static void ClearChildren(this Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}