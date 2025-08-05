using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

namespace ForceCodeFPS
{
    public class r_ThirdPersonCamera : MonoBehaviourPunCallbacks
    {
        #region Public variables
        [Header("Third Person Camera")]
        public Camera m_ThirdPersonCamera;

        [Header("Third Person Camera Settings")]
        public float m_DeathCameraTime;
        #endregion

        #region Private variables
        //Runtime data
        [HideInInspector] public float m_CameraTime;

        //Last Attacker
        [HideInInspector] public string m_LastAttackerName;
        [HideInInspector] public string m_ReceiverName;
        [HideInInspector] public float m_LastAttackerHealth;
        [HideInInspector] public string m_LastAttackerWeapon;
        #endregion

        #region Functions
        private void Update()
        {
            if (this.m_ThirdPersonCamera.gameObject.activeSelf)
                HandleDeathCamera();
        }
        #endregion

        #region Handling
        public void HandleDeathCamera()
        {
            if (this.m_CameraTime <= 0)
            {
                //Disable camera
                this.m_ThirdPersonCamera.gameObject.SetActive(false);

                if (m_LastAttackerName == PhotonNetwork.LocalPlayer.NickName)
                {
                    //Reset game settings to activate scene camera
                    r_InGameManager.Instance.ResetLocalGameSettings();
                }
                else
                {
                    //Spectate enemy
                    if (m_SpectatorController.instance)
                        m_SpectatorController.instance.CallEventOnDie(this.m_LastAttackerName, this.transform.name, this.m_LastAttackerHealth, this.m_LastAttackerWeapon);
                }

                //Destroy
                Destroy(this);
            }
            else if (this.m_CameraTime > 0)
            {
                //Decrease timer
                this.m_CameraTime -= Time.deltaTime;
            }
        }
        #endregion

        #region Actions
        public void SetDeathCamera(string _senderName, string _receiverName, float _senderHealth, string _senderWeaponName)
        {
            //Save data
            this.m_LastAttackerName = _senderName;
            this.m_LastAttackerHealth = _senderHealth;
            this.m_LastAttackerWeapon = _senderWeaponName;
            this.m_ReceiverName = _receiverName;

            //Enable death camera
            this.m_ThirdPersonCamera.gameObject.SetActive(true);

            //Reset death camera time
            this.m_CameraTime = this.m_DeathCameraTime;
        }
        #endregion
    }
}