using System;
using UnityEngine;

// TODO: GetNearestObjectHit should have raycast line check.
public class PhysicsUtils {
    public enum HitType {
        All,
        OnlyColliders,
        OnlyTriggers
    }

    public static T GetNearestObjectHit<T>(RaycastHit[] hits, Transform source, HitType hitType = HitType.All) where T : Component {
        if (hits.Length == 0)
            return default;
        T nearest = null;
        float dist = float.PositiveInfinity;
        foreach (RaycastHit hit in hits) {
            T p;
            bool sameLayer = hit.transform.gameObject.layer == source.gameObject.layer ||
                             hit.transform.gameObject.layer == 8 && source.gameObject.layer == 9 ||
                             hit.transform.gameObject.layer == 9 && source.gameObject.layer == 8;
            if (hit.collider.isTrigger && hitType == HitType.OnlyColliders ||
                !hit.collider.isTrigger && hitType == HitType.OnlyTriggers ||
                hit.transform == source ||
                (Physics.GetIgnoreLayerCollision(hit.transform.gameObject.layer, source.gameObject.layer) && !sameLayer) ||
                (p = (T) hit.transform.GetComponent(typeof(T))) is null) continue;

            if (hit.distance < dist) {
                dist = hit.distance;
                nearest = p;
            }
        }

        return nearest;
    }

    public static Transform GetNearestPlayerHit(RaycastHit[] hits, Transform source) {
        if (hits.Length == 0)
            return null;
        Transform nearest = null;
        float dist = float.PositiveInfinity;
        foreach (RaycastHit hit in hits) {
            Player p;
            bool sameLayer = hit.transform.gameObject.layer == source.gameObject.layer ||
                             hit.transform.gameObject.layer == 8 && source.gameObject.layer == 9 ||
                             hit.transform.gameObject.layer == 9 && source.gameObject.layer == 8;
            if (hit.transform == source ||
                (Physics.GetIgnoreLayerCollision(hit.transform.gameObject.layer, source.gameObject.layer) && !sameLayer) ||
                !hit.transform.CompareTag("Player") ||
                (p = hit.transform.GetComponent<Player>()) is null ||
                p.Lives == 0) continue;

            if (hit.distance < dist) {
                dist = hit.distance;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    public static Transform GetNearestHit(RaycastHit[] hits, Transform source, HitType hitType = HitType.All) {
        if (hits.Length == 0)
            return null;
        Transform nearest = null;
        float dist = float.PositiveInfinity;
        foreach (RaycastHit hit in hits) {
            bool sameLayer = hit.transform.gameObject.layer == source.gameObject.layer ||
                             hit.transform.gameObject.layer == 8 && source.gameObject.layer == 9 ||
                             hit.transform.gameObject.layer == 9 && source.gameObject.layer == 8;
            if (hit.collider.isTrigger && hitType == HitType.OnlyColliders ||
                !hit.collider.isTrigger && hitType == HitType.OnlyTriggers ||
                hit.transform == source ||
                (Physics.GetIgnoreLayerCollision(hit.transform.gameObject.layer, source.gameObject.layer) && !sameLayer)) continue;

            if (hit.distance < dist) {
                dist = hit.distance;
                nearest = hit.transform;
            }
        }

        return nearest;
    }
}