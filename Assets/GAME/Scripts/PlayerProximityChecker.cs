using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerProximityChecker : NetworkProximityChecker {
    private Player _self;

    public override bool OnCheckObserver(NetworkConnection conn) {
        Player p;
        if (_self == null)
            _self = GetComponent<Player>();
        if (!GameManager.Instance.GameStarted || (p = conn.identity.GetComponent<Player>()).Lives == 0 || p.SeesEveryone || _self.KilledBy == p)
            return true;
        if (forceHidden)
            return false;

        Vector3 position = conn.identity.transform.position + new Vector3(0f, 1f, 0f);
        Vector3 connPosition = transform.position + new Vector3(0f, 1f, 0f);
        RaycastHit[] hits = Physics.RaycastAll(position, connPosition - position, p.Vision - 1f);
        Transform nearest = PhysicsUtils.GetNearestHit(hits, conn.identity.transform, PhysicsUtils.HitType.OnlyColliders);

        return nearest != null && nearest.transform == transform;
    }

    public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize) {
        if (forceHidden)
            return;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values) {
            if (conn != null && conn.identity != null) {
                // check distance
                if (OnCheckObserver(conn)) {
                    observers.Add(conn);
                }
            }
        }
    }
}