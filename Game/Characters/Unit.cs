using UnityEngine;
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
    }

    #region RPC
    [RPC]
    public void Die() {
        Debug.Log("Dead");
        enabled = false;
        gameObject.active = false;
        _alive = false;
        GameObject.Destroy(gameObject);

        // Disconnect the player who died
        if (GetComponent<FPSController>().owner == Network.player) {
            GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().DisconnectFromServer();
        }

        // If we are the server, remove the dead player from the list of players
        if (Network.isServer) {
            GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().DestroyPlayer(GetComponent<FPSController>().owner);
        }
    }

    [RPC]
    public void Hurt(float damage) {
        health.Current -= damage;
        Debug.Log("hurt me " + health.Current);
        if (Network.isServer && health.Current == 0) {
            networkView.RPC("Die", RPCMode.OthersBuffered);
            Die();
        }
    }

    [RPC]
    void ShootAt(float damage, NetworkPlayer target) {
        if (!Network.isServer) {
            return;
        }
        Debug.Log("_alive " + _alive);
        // If the target doesn't exist, stop here
        int targetIndex = GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().players.IndexOf(target);
        if (targetIndex == -1 || !_alive) {
            return;
        }
        Unit unit = GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>().playerScripts[target];

        networkView.RPC("Hurt", target, damage);
        unit.Hurt(damage);
    }
    #endregion

}
