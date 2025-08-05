using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ForceCodeFPS
{
    #region Serializable Classes
    [System.Serializable]
    public class r_CrosshairState
    {
        public r_MoveState m_MoveState;

        [Header("Crosshair Configuration")]
        public float m_CrosshairSize;
        public float m_CrosshairAdjustSpeed;

        [Header("Crosshair Rotation")]
        public float m_CrosshairRotation;
        public float m_CrosshairRotationAdjustSpeed;
    }
    #endregion

    [CreateAssetMenu(menuName = "FPS Template/Player System/Create Player UI Configuration", fileName = "Player UI Configuration")]
    public class r_PlayerUIBase : ScriptableObject
    {
        #region Public variables

        [Header("Features")]
        public bool m_BloodyScreenFeature;
        public bool m_DamageIndicatorFeature;
        public bool m_HitmarkerFeature;
        public bool m_CrosshairFeature;

        [Header("Bloodyscreen Settings")]
        public float m_BloodyScreenTime;

        [Header("Damage Indicator Settings")]
        public float m_DamageIndicatorTime;

        [Header("Hitmarker Settings")]
        public float m_HitmarkerTime;

        [Header("Hitmarker Color Settings")]
        public Color m_HitmarkerDefaultColor;
        public Color m_HitmarkerKilledEnemyColor;

        [Header("Hitmarker Effect Settings")]
        public float m_HitmarkerRandomRotation;
        public float m_HitmarkerRotationReturnSpeed;

        [Header("Ammunition UI Settings")]
        public Color m_DefaultAmmunationColor;
        public Color m_WarningAmmunationColor;
        public Color m_EmptyAmmunationColor;

        [Header("Health UI Settings")]
        public Color m_DefaultHealthColor;
        public Color m_WarningHealthColor;

        [Header("Health UI Settings")]
        public float m_CrosshairShootReturnSpeed;

        [Space(10)] public List<r_CrosshairState> m_CrosshairStates = new List<r_CrosshairState>();
        #endregion
    }
}