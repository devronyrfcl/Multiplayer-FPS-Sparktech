using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Rendering;

namespace ForceCodeFPS
{
    public class r_ThirdPersonWeapon : MonoBehaviourPun
    {
        #region Public variables
        [Header("Weapon Information")]
        public r_WeaponPickupBase m_WeaponData;

        [Header("Weapon IK transform")]
        public Transform m_LeftHandTransform;

        [Header("Weapon FX transform")]
        public Transform m_MuzzlePointTransform;
        public Transform m_ShellPointTransform;

        [Header("Weapon FX")]
        public GameObject m_MuzzleFlash;
        public Rigidbody m_BulletShell;

        [Header("Weapon Default Position/Rotation")]
        public Vector3 m_DefaultPosition;
        public Vector3 m_DefaultRotation;

        [Header("Animation Settings")]
        public float m_equipAnimationDuration;
        public float m_unequipAnimationDuration;

        [Header("Left Hand IK Settings")]
        public bool m_LeftHandIK;

        [Space(10)] public Renderer[] m_localRenderersOnlyShadows;
        #endregion

        #region Functions
        private void Start() => SetLocalRendererShadows(ShadowCastingMode.ShadowsOnly);
        #endregion

        #region Actions
        public void SetLocalRendererShadows(ShadowCastingMode _Mode)
        {
            if (this.m_localRenderersOnlyShadows.Length == 0) return;

            if (photonView.IsMine)
            {
                for (int i = 0; i < this.m_localRenderersOnlyShadows.Length; i++)
                {
                    //Disable weapon shadow for first person
                    if (this.m_localRenderersOnlyShadows[i]) this.m_localRenderersOnlyShadows[i].shadowCastingMode = _Mode;
                }
            }
        }
        #endregion
    }
}