using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Photon;
using Photon.Pun;
using System.Linq;

namespace ForceCodeFPS
{
    #region Serializable Classes
    [System.Serializable]
    public class r_ThirdPersonWeaponSlotItem
    {
        public GameObject m_ThirdPersonWeapon;

        [Header("Unique runtime ID")]
        [HideInInspector] public string m_UniqueID;
    }

    [System.Serializable]
    public class r_ThirdPersonWeaponSlot
    {
        [Header("Slot Information")]
        public r_WeaponItemType m_WeaponType;

        [Header("Slot Position")]
        public Transform m_WeaponSlot;

        [Space(10)] public List<r_ThirdPersonWeaponSlotItem> m_SlotWeapons;
    }
    #endregion

    public class r_ThirdPersonManager : MonoBehaviourPun
    {
        #region References
        public r_PlayerController m_PlayerController;
        #endregion

        #region Public Variables
        [Header("Character Animator")]
        public Animator m_ThirdPersonAnimator;

        [Header("Third Person Death Camera")]
        public r_ThirdPersonCamera m_ThirdPersonCamera;

        [Header("Spectate Holder Transform")]
        public Transform m_SpectateHolder;

        [Header("Weapon Manager")]
        public Transform m_WeaponParent;

        [Header("Animator Layer Settings")]
        public int m_WeaponAnimationLayer;

        //Weapon Slot Settings
        [Space(10)] public List<r_ThirdPersonWeaponSlot> m_WeaponSlots = new List<r_ThirdPersonWeaponSlot>();

        //Body Parts
        [Space(10)] public List<r_ThirdPersonBodyPart> m_BodyParts = new List<r_ThirdPersonBodyPart>();

        //Local Renderers
        [Space(10)] public Renderer[] m_localRenderersOnlyShadows;
        #endregion

        #region Private Variables
        //Current Weapon
        [HideInInspector, Space(10)] public r_WeaponManagerData m_CurrentWeapon;

        //Left Hand IK Weight
        [HideInInspector, Range(0, 1)] public float m_CurrentLeftHandWeight;

        //Current Lean Animator Angle
        [HideInInspector] public float m_AnimatorLeanAngle = 0;
        #endregion

        #region Functions
        private void Start() => Setup();

        private void Update()
        {
            if (this.m_PlayerController == null || this.m_ThirdPersonAnimator == null) return;

            HandleAnimator();
            HandleLeaning();
        }
        #endregion

        #region Handling
        private void HandleAnimator()
        {
            if (photonView.IsMine)
            {
                //Set jumping boolean
                this.m_ThirdPersonAnimator.SetBool("Jumping", !this.m_PlayerController.m_CharacterController.isGrounded);

                //Horizontal and vertical movement with smoothness
                this.m_ThirdPersonAnimator.SetFloat("MoveX", this.m_PlayerController.m_InputManager.GetHorizontal(), 0.1f, Time.deltaTime);
                this.m_ThirdPersonAnimator.SetFloat("MoveZ", this.m_PlayerController.m_InputManager.GetVertical(), 0.1f, Time.deltaTime);

                //Set other movement booleans
                this.m_ThirdPersonAnimator.SetBool("Sprinting", this.m_PlayerController.m_MoveState == r_MoveState.SPRINTING);
                this.m_ThirdPersonAnimator.SetBool("Crouching", this.m_PlayerController.m_MoveState == r_MoveState.CROUCHING);
                this.m_ThirdPersonAnimator.SetBool("Sliding", this.m_PlayerController.m_MoveState == r_MoveState.SLIDING);

                //Set weapon ID to play specific weapon animations
                this.m_ThirdPersonAnimator.SetInteger("weaponID", this.m_PlayerController.m_WeaponManager.m_CurrentWeaponIndex == 0 && this.m_PlayerController.m_WeaponManager.m_LocalWeapons.Count == 0 ? -1 : this.m_CurrentWeapon.m_WeaponData.m_WeaponID);

                //Set current left hand weight with lerping to give a smooth transition when move hand
                this.m_CurrentLeftHandWeight = Mathf.Lerp(this.m_CurrentLeftHandWeight, this.m_CurrentWeapon != null && !this.m_PlayerController.m_WeaponManager.m_ChangingWeapon && !this.m_PlayerController.m_WeaponManager.m_ReloadingWeapon ? 1 : 0, Time.deltaTime * 8f);
            }
        }

        private void HandleLeaning()
        {
            //Player Camera configuration
            var _player_camera = this.m_PlayerController.m_PlayerCamera;

            //Current lean angle and speed
            float _animation_lean_angle = 0;
            float _animation_lean_speed = _player_camera.m_CameraBase.m_CameraLeanSettings.m_AnimatorLeanChangeSpeed;

            if (_player_camera.m_CameraRotationLeanAngle < 0)
            {
                //if lean angle is less than 0, player is leaning to left
                _animation_lean_angle = -_player_camera.m_CameraBase.m_CameraLeanSettings.m_MaxLeanAngleAnimator;
            }
            else if (_player_camera.m_CameraRotationLeanAngle > 0)
            {
                //if lean angle is greater than 0, player is leaning to right
                _animation_lean_angle = _player_camera.m_CameraBase.m_CameraLeanSettings.m_MaxLeanAngleAnimator;
            }
            else if (_player_camera.m_CameraRotationLeanAngle == 0)
            {
                //return animator lean angle to 0
                _animation_lean_angle = 0;
            }

            //Lerp animator lean angle
            this.m_AnimatorLeanAngle = Mathf.Lerp(this.m_AnimatorLeanAngle, _animation_lean_angle, Time.deltaTime * _animation_lean_speed);

            //Set animator lean angle
            this.m_ThirdPersonAnimator.SetFloat("Leaning", this.m_AnimatorLeanAngle);
        }
        #endregion

        #region Actions
        private void Setup()
        {
            //Disable character mesh and shadow only
            SetLocalRendererShadows(ShadowCastingMode.ShadowsOnly);

            //Deactivate Ragdoll
            DeActivateRagdoll();
        }

        public void ActivateRagdoll()
        {
            if (this.m_BodyParts == null) return;

            foreach (r_ThirdPersonBodyPart _BodyPart in this.m_BodyParts)
            {
                if (_BodyPart.m_BodyParts.m_BodyPartRigidbody != null && _BodyPart.m_BodyParts.m_BodyPartCollider != null)
                {
                    //set drag
                    _BodyPart.m_BodyParts.m_BodyPartRigidbody.drag = 0.5f;

                    //Set kinematic
                    _BodyPart.m_BodyParts.m_BodyPartRigidbody.isKinematic = false;

                    //Enable body part collider for first person
                    if (photonView.IsMine) _BodyPart.m_BodyParts.m_BodyPartCollider.enabled = true;
                }
            }
        }

        public void DeActivateRagdoll()
        {
            if (this.m_BodyParts == null) return;

            foreach (r_ThirdPersonBodyPart _BodyPart in this.m_BodyParts)
            {
                //Set kinematic
                if (_BodyPart.m_BodyParts.m_BodyPartRigidbody != null && _BodyPart.m_BodyParts.m_BodyPartCollider != null) _BodyPart.m_BodyParts.m_BodyPartRigidbody.isKinematic = true;

                //Disable body part collider for first person
                if (photonView.IsMine) _BodyPart.m_BodyParts.m_BodyPartCollider.enabled = false;
            }
        }

        public void ThirdPersonSuicide()
        {
            //Enable remote character mesh
            SetLocalRendererShadows(ShadowCastingMode.On);

            //Get components from third person character
            Component[] _Components = this.transform.GetComponents<Component>();

            //Remove components of third person character
            foreach (Component _Component in _Components)
            {
                if (_Component is Transform) continue;

                Destroy(_Component);
            }

            //Enable ragdoll
            this.ActivateRagdoll();
        }

        public void PlayAnimation(r_AnimationType _type, string _animationName, bool _setBool, int _layer)
        {
            switch (_type)
            {
                case r_AnimationType.PLAY: this.m_ThirdPersonAnimator.Play(_animationName, _layer, 0f); break;
                case r_AnimationType.SETTRIGGER: this.m_ThirdPersonAnimator.SetTrigger(_animationName); break;
                case r_AnimationType.SETBOOL: this.m_ThirdPersonAnimator.SetBool(_animationName, _setBool); break;
            }
        }

        public void OnWeaponFire() => photonView.RPC(nameof(OnWeaponFireEvent_RPC), RpcTarget.All);

        public void OnWeaponReload() => photonView.RPC(nameof(OnWeaponReloadEvent_RPC), RpcTarget.All);

        public void OnShellEject() => photonView.RPC(nameof(OnShellEjectEvent_RPC), RpcTarget.Others);
        #endregion

        #region Network Events
        [PunRPC]
        private void OnWeaponFireEvent_RPC()
        {
            r_WeaponController _weapon_FP = this.m_CurrentWeapon.m_WeaponObject_FP;
            r_ThirdPersonWeapon _weapon_TP = this.m_CurrentWeapon.m_WeaponObject_TP;

            if (_weapon_FP != null && _weapon_TP != null)
            {
                //Muzzle flash only for other players except ourself
                if (!photonView.IsMine)
                {
                    // Instantiates a bullet shell.
                    GameObject _muzzle = Instantiate(_weapon_TP.m_MuzzleFlash, _weapon_TP.m_MuzzlePointTransform.position, _weapon_TP.m_MuzzlePointTransform.rotation);

                    //Destroy muzzle
                    Destroy(_muzzle, _weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_muzzleLifetime + 0.2f);
                }

                //Play fire animation
                PlayAnimation(r_AnimationType.PLAY, _weapon_FP.m_WeaponConfig.m_AnimationSettings.m_FireAnimName, false, this.m_WeaponAnimationLayer);
            }
        }

        [PunRPC]
        private void OnWeaponReloadEvent_RPC()
        {
            //Play reload animation on current animation
            PlayAnimation(r_AnimationType.SETTRIGGER, "Reload", false, this.m_WeaponAnimationLayer);
        }

        [PunRPC]
        public void OnShellEjectEvent_RPC()
        {
            r_WeaponController _weapon_FP = this.m_CurrentWeapon.m_WeaponObject_FP;
            r_ThirdPersonWeapon _weapon_TP = this.m_CurrentWeapon.m_WeaponObject_TP;

            if (_weapon_FP != null && _weapon_TP != null)
            {
                //Instantiates a bullet shell
                Rigidbody _shell = (Rigidbody)Instantiate(_weapon_TP.m_BulletShell, _weapon_TP.m_ShellPointTransform.position, _weapon_TP.m_ShellPointTransform.rotation);

                //Apply bullet shell force
                _shell.velocity = _shell.transform.forward * _weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellEjectForce;

                //Random bullet shell rotation
                Vector3 _shellRotation = new Vector3(Random.Range(-_weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.x, _weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.x), Random.Range(-_weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.y, _weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.y), Random.Range(-_weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.z, _weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.z));

                //Apply bullet shell rotation
                _shell.transform.localRotation = Quaternion.Euler(_weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellStartRotation + _shellRotation);

                //Destroy after 1 second.
                Destroy(_shell, _weapon_FP.m_WeaponConfig.m_weaponFXSettings.m_shellLifeTime);
            }
        }

        public void OnEquipWeapon(string _unique_weapon_id, float _equip_duration) => StartCoroutine(OnEquipWeaponCorountine(_unique_weapon_id, _equip_duration));

        public IEnumerator OnEquipWeaponCorountine(string _unique_weapon_id, float _equip_duration)
        {
            yield return new WaitForSeconds(_equip_duration);

            //Declare local weapon inventory
            var _local_weapons = this.m_PlayerController.m_WeaponManager.m_LocalWeapons;

            //Declare local weapon
            var _local_weapon = _local_weapons.Find(x => x.m_UniqueID == _unique_weapon_id);

            //Break corountine if local weapon doesn't exist
            if (_local_weapon == null || _local_weapon.m_WeaponObject_FP == null || _local_weapon.m_WeaponObject_TP == null) yield break;

            //Play animation
            PlayAnimation(r_AnimationType.PLAY, _local_weapon.m_WeaponData.m_Weapon_FP_Prefab.m_WeaponConfig.m_AnimationSettings.m_EquipAnimName, false, this.m_WeaponAnimationLayer);

            //Enable the third person weapon object
            _local_weapon.m_WeaponObject_TP.gameObject.SetActive(true);

            //Set parent
            _local_weapon.m_WeaponObject_TP.transform.parent = this.m_WeaponParent;

            //Set position and rotation
            _local_weapon.m_WeaponObject_TP.transform.localPosition = _local_weapon.m_WeaponData.m_Weapon_TP_Prefab.m_DefaultPosition;
            _local_weapon.m_WeaponObject_TP.transform.localRotation = Quaternion.Euler(_local_weapon.m_WeaponData.m_Weapon_TP_Prefab.m_DefaultRotation);

            //Find slot by weapon type
            r_ThirdPersonWeaponSlot _slot = FindWeaponSlotByType(_local_weapon.m_WeaponData.m_WeaponType);

            if (_slot != null)
            {
                //Find weapon object in slot
                r_ThirdPersonWeaponSlotItem _weapon_in_slot = _slot.m_SlotWeapons.Find(x => x.m_UniqueID == _unique_weapon_id);

                if (_weapon_in_slot != null)
                {
                    //Remove weapon object from slot list
                    _slot.m_SlotWeapons.Remove(_weapon_in_slot);
                }
            }
        }

        public void OnUnequipWeapon(string _unique_weapon_id, float _unequip_duration) => StartCoroutine(OnUnequipWeaponCorountine(_unique_weapon_id, _unequip_duration));

        public IEnumerator OnUnequipWeaponCorountine(string _unique_weapon_id, float _unequip_duration)
        {
            //Declare local weapon inventory
            var _local_weapons = this.m_PlayerController.m_WeaponManager.m_LocalWeapons;

            //Declare local weapon
            var _local_weapon = _local_weapons.Find(x => x.m_UniqueID == _unique_weapon_id);

            //Break corountine if local weapon doesn't exist
            if (_local_weapon == null || _local_weapon.m_WeaponObject_FP == null || _local_weapon.m_WeaponObject_TP == null) yield break;

            //Play animation
            PlayAnimation(r_AnimationType.PLAY, _local_weapon.m_WeaponObject_FP.m_WeaponConfig.m_AnimationSettings.m_UnequipAnimName, false, this.m_WeaponAnimationLayer);

            yield return new WaitForSeconds(_unequip_duration);

            //Break corountine if local weapon doesn't exist
            if (_local_weapon == null || _local_weapon.m_WeaponObject_FP == null || _local_weapon.m_WeaponObject_TP == null) yield break;

            //Check Weapon Type
            r_ThirdPersonWeaponSlot _slot = FindWeaponSlotByType(_local_weapon.m_WeaponData.m_WeaponType);

            if (_slot != null)
            {
                //Create new slot item
                r_ThirdPersonWeaponSlotItem _slot_item = new r_ThirdPersonWeaponSlotItem { m_ThirdPersonWeapon = _local_weapon.m_WeaponObject_TP.gameObject, m_UniqueID = _unique_weapon_id };

                //Add weapon to slot
                _slot.m_SlotWeapons.Add(_slot_item);

                //Set parent
                _local_weapon.m_WeaponObject_TP.transform.parent = _slot.m_WeaponSlot;

                //Set position and rotation
                _local_weapon.m_WeaponObject_TP.transform.localPosition = Vector3.zero;
                _local_weapon.m_WeaponObject_TP.transform.localRotation = Quaternion.identity;
            }
        }
        #endregion

        #region Get
        private r_ThirdPersonWeaponSlot FindWeaponSlotByType(r_WeaponItemType _type) => this.m_WeaponSlots.Find(x => x.m_WeaponType == _type);
        #endregion

        #region Set
        public void SetLocalRendererShadows(ShadowCastingMode _Mode)
        {
            if (this.m_localRenderersOnlyShadows.Length == 0) return;

            if (photonView.IsMine)
            {
                for (int i = 0; i < this.m_localRenderersOnlyShadows.Length; i++)
                {
                    if (this.m_localRenderersOnlyShadows[i])
                        this.m_localRenderersOnlyShadows[i].shadowCastingMode = _Mode;
                }
            }
        }
        #endregion

        #region Custom
        private void OnAnimatorIK()
        {
            if (this.m_ThirdPersonAnimator != null)
            {
                if (this.m_CurrentWeapon != null && this.m_CurrentWeapon.m_WeaponObject_TP != null)
                {
                    r_ThirdPersonWeapon _weapon = this.m_CurrentWeapon.m_WeaponObject_TP;

                    if (_weapon != null)
                    {
                        //Left hand IK with position and rotation offset
                        this.m_ThirdPersonAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _weapon.m_LeftHandIK ? this.m_CurrentLeftHandWeight : 0);
                        this.m_ThirdPersonAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _weapon.m_LeftHandIK ? this.m_CurrentLeftHandWeight : 0);

                        this.m_ThirdPersonAnimator.SetIKPosition(AvatarIKGoal.LeftHand, _weapon.m_LeftHandTransform.position);
                        this.m_ThirdPersonAnimator.SetIKRotation(AvatarIKGoal.LeftHand, _weapon.m_LeftHandTransform.rotation);
                    }
                }
                else
                {
                    //Left hand IK with position and rotation offset
                    this.m_ThirdPersonAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, this.m_CurrentLeftHandWeight);
                    this.m_ThirdPersonAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, this.m_CurrentLeftHandWeight);
                }
            }
        }
        #endregion
    }
}