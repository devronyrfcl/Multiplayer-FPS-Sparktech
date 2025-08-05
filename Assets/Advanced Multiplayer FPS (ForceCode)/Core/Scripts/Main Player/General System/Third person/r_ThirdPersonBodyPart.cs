using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceCodeFPS
{
    #region Serializable Enums
    [System.Serializable] public enum r_BodyPartType { Head, Body, Arm, Leg }
    #endregion

    #region Serializable Classes
    [System.Serializable]
    public class r_DamagableBodyPart
    {
        [Header("Body Part Type")]
        public r_BodyPartType m_BodyPartType;

        [Header("Body part collider/rigidbody")]
        [HideInInspector] public Collider m_BodyPartCollider;
        [HideInInspector] public Rigidbody m_BodyPartRigidbody;
    }
    #endregion

    public class r_ThirdPersonBodyPart : MonoBehaviour
    {
        #region Public variables
        [Header("Player Health")]
        public r_PlayerHealth m_PlayerHealth;

        [Header("Body Part")]
        public r_DamagableBodyPart m_BodyParts;
        #endregion

        #region Functions
        private void Awake() => FindBodyPartInfo();
        #endregion

        #region Get
        private void FindBodyPartInfo()
        {
            //Find rigidbody
            if (this.transform.GetComponent<Rigidbody>() != null)
            {
                this.m_BodyParts.m_BodyPartRigidbody = this.transform.GetComponent<Rigidbody>();
            }

            //Find collider
            if (this.transform.GetComponent<Collider>() != null)
            {
                this.m_BodyParts.m_BodyPartCollider = this.transform.GetComponent<Collider>();
            }
        }
        #endregion
    }
}