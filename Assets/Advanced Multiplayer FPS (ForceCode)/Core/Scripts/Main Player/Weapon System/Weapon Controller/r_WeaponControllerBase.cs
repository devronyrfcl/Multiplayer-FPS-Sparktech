using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceCodeFPS
{
    #region Serializable Enums
    [System.Serializable] public enum r_ReloadType { MAG, SINGLE }
    [System.Serializable] public enum r_WeaponState { IDLE, RELOADING, FIRING }
    [System.Serializable] public enum r_WeaponAimState { HIP, AIMING }
    [System.Serializable] public enum FireMode { SINGLE, AUTOMATIC, BURST }
    [System.Serializable] public enum BulletType { RAYCAST, PHYSIC }
    [System.Serializable] public enum r_AnimationType { PLAY, SETTRIGGER, SETBOOL, SETFLOAT, SETINTEGER }
    #endregion

    #region Serializable classes
    [System.Serializable]
    public class r_WeaponInformationSetting
    {
        [Space(10)]
        public string m_WeaponName;
    }

    [System.Serializable]
    public class r_AnimationSettings
    {
        [Space(10)]
        public string m_FireAnimName;
        public string m_ReloadAnimName;
        public string m_EquipAnimName;
        public string m_UnequipAnimName;

        [Space(10)]
        public string m_WalkingBoolName;
        public string m_SprintingBoolName;

        [Space(10)]
        public float m_EquipAnimDuration;
        public float m_UnequipAnimDuration;

        [Space(10)]
        public string m_ReloadStartTriggerName = "Reload_Start";
        public string m_ReloadFinishTriggerName = "Reload_Finished";

        [Space(10)]
        [Range(0, 25)] public float m_AnimationTransitionSpeed;
    }

    [System.Serializable]
    public class r_FireModeSettings
    {
        [Header("Fire Mode")]
        public FireMode m_FireMode;

        [Header("Fire Rate")]
        public float m_FireRate;

        [Header("Burst Mode")]
        public int m_BurstModeCount;
        public float m_BurstModeDelay;
    }

    [System.Serializable]
    public class r_BulletSettings
    {
        [Header("Bullet Type")]
        public BulletType m_BulletType;

        [Header("Shot Fragments")]
        public int m_ShotFragments;

        [Header("Bullet Spread")]
        public float m_HipSpread;
        public float m_AimSpread;

        [Header("Raycast Distance")]
        public float m_ShootDistance;

        [Header("Rigidbody Impact Force")]
        public float m_bulletImpactForce;
        public float m_ragdollImpactForce;
    }

    [System.Serializable]
    public class r_ReloadSettings
    {
        [Header("Reload Type")]
        public r_ReloadType m_reloadType;

        [Header("Reload Settings")]
        public bool m_AutoReload;
        public float m_ReloadDuration;

        [Header("Warning Settings")]
        public int m_AmmoWarningOnBulletsLeft;

        [Header("Single Reload Settings")]
        public float m_StartReloadDuration;
    }

    [System.Serializable]
    public class r_Ammunation
    {
        public int m_Ammo;
        public int m_MaxAmmoClip;
    }

    [System.Serializable]
    public class r_BodyPartWeaponDamage
    {
        [Header("Body part Settings")]
        public r_BodyPartType m_bodyPartType;
        public float m_weaponDamage;
    }

    [System.Serializable]
    public class r_WeaponDamageEffectSettings
    {
        [Header("Damage impact")]
        public GameObject m_bodyImpact;
    }

    [System.Serializable]
    public class r_WeaponMotionSettings
    {
        [Header("Features")]
        public bool m_SprintFunction;
        public bool m_CrouchFunction;
        public bool m_BobFunction;

        [Header("Crouch Motion")]
        public Vector3 m_CrouchPosition;
        public Vector3 m_CrouchRotation;

        [Header("Sprint Motion")]
        public Vector3 m_SprintPosition;
        public Vector3 m_SprintRotation;

        [Header("Bob Motion")]
        public List<r_WeaponBobMotionSettings> m_WeaponBobSettings = new List<r_WeaponBobMotionSettings>();

        [Header("Motion settings")]
        public float m_CrouchSpeed;
        public float m_SprintSpeed;
    }

    [System.Serializable]
    public class r_WeaponBobMotionSettings
    {
        public r_MoveState m_MoveState;

        [Header("Weapon Bob Settings")]
        public float m_WeaponbobSpeed;
        public float m_WeaponbobAmount;
    }

    [System.Serializable]
    public class r_weaponAimSettings
    {
        public bool m_aimFunction;

        [Header("Aim Position")]
        public Vector3 m_hipPosition;
        public Vector3 m_hipRotation;

        public Vector3 m_aimPosition;
        public Vector3 m_aimRotation;

        [Header("Aim Settings")]
        public float m_aimSpeed;
        public float m_aimSpeedReturn;

        [Header("Aim FOV Settings")]
        public float m_aimFOV;
        public float m_aimFOVSpeed;

        [Header("Aim Sprint Settings")]
        public float m_AimResetAfterSprintingSpeed;
    }

    [System.Serializable]
    public class r_WeaponSwaySettings
    {
        public bool m_SwayFunction;

        [Header("Sway settings")]
        public float m_swayMultiplier;
        public float m_swayMultiplerAim;

        [Header("Sway smoothness")]
        public float m_swaySmoothness;
    }

    [System.Serializable]
    public class r_WeaponTiltSettings
    {
        public bool m_tiltFunction;

        [Header("Tilt settings")]
        public float m_tiltAngle;
        public float m_tiltSmoothness;
    }

    [System.Serializable]
    public class r_WeaponJumpLandSettings
    {
        public bool m_jumpLandFunction;

        [Header("Jump settings")]
        public Vector3 m_jumpPosition;
        public Vector3 m_jumpRotation;

        [Header("Land settings")]
        public Vector3 m_landPosition;
        public Vector3 m_landRotation;

        [Header("General settings")]
        public float m_effectDuration;
        public float m_effectDurationReturn;
    }

    [System.Serializable]
    public class r_WeaponLeanSettings
    {
        public bool m_LeanFunction;

        [Header("Lean Position")]
        public Vector3 m_LeanPositionLeft;
        public Vector3 m_LeanPositionRight;

        [Header("Lean Rotation")]
        public float m_LeanRotationAngle;

        [Header("Lean Settings")]
        public float m_LeanRotationAngleSpeed;
        public float m_LeanPositionAngleSpeed;
    }

    [System.Serializable]
    public class r_WeaponRecoilSettings
    {
        public bool m_recoilFunction;

        [Header("Recoil Euler")]
        public Vector3 m_recoilEuler;

        [Header("Recoil Settings")]
        public float m_recoilTime;
        public float m_recoilTimeReturn;
    }

    [System.Serializable]
    public class r_WeaponKickbackSettings
    {
        public bool m_KickbackFunction;

        [Header("Kickback Amounts")]
        public Vector3 m_KickBackPositionAmount;
        public Vector3 m_KickBackRotationAmount;

        [Header("Kickback Settings")]
        public float m_KickBackReturnSpeed;

        [Header("Kickback Aim Stabilization")]
        public float m_KickBackAimStabilizationMultiplier;
    }

    [System.Serializable]
    public class r_WeaponSoundSettings
    {
        [Space(10)]
        public List<AudioClip> m_AudioClips = new List<AudioClip>();

        [Space(10)]
        public string m_FireSoundName;
        public string m_ReloadSoundName;
        public string m_EquipSoundName;
        public string m_UnequipSoundName;
        public string m_BodyImpactSoundName;
        public string m_EnemyKilledSoundName;

        [Space(10)]
        [Range(0, 1)] public float m_FireSoundVolume;
        [Range(0, 1)] public float m_ReloadSoundVolume;
        [Range(0, 1)] public float m_EquipSoundVolume;
        [Range(0, 1)] public float m_UnequipSoundVolume;
        [Range(0, 1)] public float m_BodyImpactVolume;
        [Range(0, 1)] public float m_EnemyKilledVolume;
    }

    [System.Serializable]
    public class r_weaponFX
    {
        public bool m_muzzleflashFunction;
        public bool m_shellejectFunction;
        public bool m_BulletTrailFunction;

        [Header("Muzzle Settings")]
        public GameObject m_muzzleFlash;

        [Space(10)]
        public float m_muzzleLifetime;

        [Header("Shell Prefab")]
        public Rigidbody m_bulletShell;

        [Space(10)]
        public float m_shellLifeTime;
        public float m_shellAfterTime;

        [Space(10)]
        public float m_shellEjectForce;

        [Space(10)]
        public Vector3 m_shellStartRotation;
        public Vector3 m_shellRandomRotation;

        [Header("Bullet Trail Prefab")]
        public GameObject m_BulletTrailPrefab;
    }

    [System.Serializable]
    public class r_weaponScopeSettings
    {
        public bool m_scopeFunction;

        [Header("Scope Texture")]
        public Texture2D m_scopeTexture2D;

        [Header("Scope Settings")]
        public float m_scopeNearAimPosition;
    }

    [System.Serializable]
    public class r_weaponCrosshairSettings
    {
        public float m_CrosshairIncreaseSize;
    }
    #endregion

    [CreateAssetMenu(menuName = "FPS Template/Weapon System/Create Weapon Configuration", fileName = "Weapon Configuration")]
    public class r_WeaponControllerBase : ScriptableObject
    {
        #region Public variables
        [Space(10)] public r_WeaponInformationSetting m_WeaponInformationSettings;

        [Space(10)] public r_AnimationSettings m_AnimationSettings;

        [Space(10)] public r_FireModeSettings m_FireModeSettings;

        [Space(10)] public r_BulletSettings m_BulletSettings;

        [Space(10)] public r_WeaponRecoilSettings m_weaponRecoilSettings;

        [Space(10)] public r_WeaponKickbackSettings m_WeaponKickbackSettings;

        [Space(10)] public r_ReloadSettings m_ReloadSettings;

        [Space(10)] public r_WeaponMotionSettings m_WeaponMotionSettings;

        [Space(10)] public r_WeaponLeanSettings m_WeaponLeanSettings;

        [Space(10)] public r_WeaponSwaySettings m_weaponSwaySettings;

        [Space(10)] public r_WeaponTiltSettings m_weaponTiltSettings;

        [Space(10)] public r_WeaponJumpLandSettings m_WeaponJumpLandSettings;

        [Space(10)] public r_weaponAimSettings m_weaponAimSettings;

        [Space(10)] public r_weaponScopeSettings m_weaponScopeSettings;

        [Space(10)] public r_WeaponSoundSettings m_WeaponSound;

        [Space(10)] public r_weaponFX m_weaponFXSettings;

        [Space(10)] public r_weaponCrosshairSettings m_weaponCrosshairSettings;

        [Space(10)] public r_WeaponDamageEffectSettings m_weaponDamageEffectSettings;

        [Space(10)] public List<r_BodyPartWeaponDamage> m_WeaponDamageParts = new List<r_BodyPartWeaponDamage>();
        #endregion
    }
}