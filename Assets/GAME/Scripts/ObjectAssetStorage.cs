using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectAssetStorage : MonoBehaviour {
    private static readonly Dictionary<GameObject, ObjectAssetStorage> _storage = new Dictionary<GameObject, ObjectAssetStorage>();

    private static ObjectAssetStorage RequireComponent(GameObject obj) {
        if (_storage.TryGetValue(obj, out ObjectAssetStorage storage)) return storage;
        storage = obj.GetComponent<ObjectAssetStorage>();
        _storage.Add(obj, storage);
        return storage;
    }

    public GameObject[] List;

    public GameObject Get(string assetName) {
        return List.FirstOrDefault(o => o.name.Equals(assetName, StringComparison.InvariantCultureIgnoreCase));
    }

    public GameObject Get(int assetId) {
        return List[assetId];
    }

    public int Index(string assetName) {
        for (var i = 0; i < List.Length; i++) {
            if (List[i].name == assetName)
                return i;
        }

        return -1;
    }

    public static GameObject Get(Component obj, string assetName) {
        return RequireComponent(obj.gameObject).Get(assetName);
    }

    public static GameObject Get(GameObject obj, string assetName) {
        return RequireComponent(obj).Get(assetName);
    }

    public static GameObject Get(Component obj, int assetId) {
        return RequireComponent(obj.gameObject).Get(assetId);
    }

    public static GameObject Get(GameObject obj, int assetId) {
        return RequireComponent(obj).Get(assetId);
    }

    public static int Index(Component obj, string assetName) {
        return RequireComponent(obj.gameObject).Index(assetName);
    }

    public static int Index(GameObject obj, string assetName) {
        return RequireComponent(obj).Index(assetName);
    }
}