using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceCodeFPS
{
    [CreateAssetMenu(menuName = "FPS Template/Weapon System/Create Weapon Pickup Configuration", fileName = "Weapon pickup Configuration")]
    public class r_WeaponPickupBase : ScriptableObject
    {
        #region Public variables
        [Header("Weapon Type")]
        public r_WeaponItemType m_WeaponType;

        [Header("Weapon Information")]
        public string m_WeaponName;

        [Header("Weapon Identification")]
        public int m_WeaponID;

        [Header("Weapon UI")]
        public Texture2D m_WeaponTexture;

        [Header("Weapon Prefabs")]
        public r_WeaponController m_Weapon_FP_Prefab;
        public r_ThirdPersonWeapon m_Weapon_TP_Prefab;
        public GameObject m_Weapon_Pickup_Prefab;
        #endregion
    }
}