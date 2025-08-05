using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace ForceCodeFPS
{
    public class r_WeaponPickup : MonoBehaviourPunCallbacks
    {
        [Header("Configuration")]
        public r_WeaponPickupBase m_WeaponPickupData;

        public void OnPickup()
        {
            //RPC destroy for everyone
            photonView.RPC(nameof(DestroyPickup), RpcTarget.AllBuffered);
        }

        [PunRPC]
        public void DestroyPickup()
        {
            //Only masterclient can destroy the object since it is instantiated as room object
            if (PhotonNetwork.IsMasterClient)
            {
                //Destroy object for everyone
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }
}