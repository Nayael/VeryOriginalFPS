﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    #region Public Members
    public Health health;
    #endregion

    #region Private Members
    private bool _alive = true;
    #endregion

    public bool Alive {
        get { return _alive; }
        set { _alive = value; }
    }

    #region RPC
    [RPC]
    public void Die() {
        enabled = false;
        _alive = false;
        GetComponent<Shooter>().deaths++;

        //// If we are the server, remove the dead player from the list of players
        if (Network.isServer) {
            GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().RespawnPlayer(GetComponent<FPSController>().owner);
        }
    }

    [RPC]
    public void Hurt(float damage, NetworkPlayer shooter) {
        health.Current -= damage;
        // If we are the server and the unit has no health left, we make him die
        if (Network.isServer && health.Current < 1f) {
            // I am the killer, add a frag
            GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().playerScripts[shooter].GetComponent<Shooter>().networkView.RPC("AddFrag", shooter);

            networkView.RPC("Die", RPCMode.Others);
            Die();
        }
    }

    [RPC]
    void ShootAtMe(float damage, NetworkPlayer shooter) {
        if (!Network.isServer) {
            return;
        }

        // If the target doesn't exist, stop here
        int targetIndex = GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().players.IndexOf(GetComponent<FPSController>().owner);
        Unit unit = GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().playerScripts[GetComponent<FPSController>().owner];

        if (targetIndex == -1 || !_alive || !GetComponent<Shooter>().Weapon.CanHit(unit)) {
            return;
        }
        networkView.RPC("Hurt", GetComponent<FPSController>().owner, damage, shooter);
        unit.Hurt(damage, shooter);
    }

    [RPC]
    void Respawn(Vector3 position) {
        _alive = true;
        health.Fill();
        transform.localPosition = position;
        enabled = true;
        gameObject.active = true;

        if (GetComponent<Shooter>().enabled) {
            GetComponent<Shooter>().Init();
        }
    }
    #endregion

}
