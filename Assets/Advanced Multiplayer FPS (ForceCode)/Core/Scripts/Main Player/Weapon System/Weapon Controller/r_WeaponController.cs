using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Photon;
using Photon.Pun;

namespace ForceCodeFPS
{
    public class r_WeaponController : MonoBehaviour
    {
        #region References
        public r_PlayerController m_PlayerController
        {
            get => this.transform.GetComponentInParent<r_PlayerController>();
            set => this.m_PlayerController = value;
        }

        public r_PlayerCamera m_PlayerCamera
        {
            get => this.m_PlayerController != null ? this.m_PlayerController.m_PlayerCamera : this.transform.GetComponentInParent<r_PlayerCamera>();
            set => this.m_PlayerCamera = value;
        }

        public r_WeaponManager m_weaponManager
        {
            get => this.m_PlayerController != null ? this.m_PlayerController.m_WeaponManager : this.transform.GetComponentInParent<r_WeaponManager>();
            set => this.m_weaponManager = value;
        }

        public r_PlayerAudio m_PlayerAudio
        {
            get => this.m_PlayerController != null ? this.m_PlayerController.m_PlayerAudio : this.transform.GetComponentInParent<r_PlayerAudio>();
            set => this.m_PlayerAudio = value;
        }
        #endregion

        #region Public variables
        [Header("Weapon Base Configuration")]
        public r_WeaponControllerBase m_WeaponConfig;

        [Header("Weapon Animator")]
        public Animator m_Animator;

        [Header("Layer Mask")]
        public LayerMask m_LayerMask;

        [Header("Weapon motion transforms")]
        public Transform m_WeaponAimTransform;
        public Transform m_weaponSwayTransform;
        public Transform m_weaponTiltTransform;
        public Transform m_weaponCrouchTransform;
        public Transform m_weaponSprintTransform;
        public Transform m_weaponBobTransform;
        public Transform m_weaponLeanTransform;
        public Transform m_weaponJumpLandTransform;
        public Transform m_WeaponKickbackTransform;

        [Header("Weapon FX transforms")]
        public Transform m_muzzlePointTransform;
        public Transform m_shellPointTransform;

        [Space(10)] public Renderer[] m_weaponMeshRenderers;

        [Space(10)] public r_Ammunation m_Ammunation;
        #endregion

        #region Private variables
        //Current weapon capabilities
        [HideInInspector] public bool m_CanShoot;
        [HideInInspector] public bool m_CanReload;
        [HideInInspector] public bool m_canAim;

        //Current weapon states
        [HideInInspector] public r_WeaponState m_WeaponState;
        [HideInInspector] public r_WeaponAimState m_WeaponAimState;

        //Current fire modes
        [HideInInspector] public float m_FireCooldown;
        [HideInInspector] public int m_BurstModeCount;

        //Current recoil
        [HideInInspector] public Vector3 m_currentRecoil;
        [HideInInspector] public Vector3 m_targetRecoil;

        //Declared sway initial rotation
        [HideInInspector] public Quaternion m_initialSwayRotation;

        //Current scoped aiming
        [HideInInspector] public bool m_onAimingScope;

        //Check weapon jumping/landing effect
        [HideInInspector] public bool m_IsWeaponFallEffect;

        //Check current aim time
        [HideInInspector] public float m_CurrentAimTime;

        //Current headbob timer
        [HideInInspector] public float m_WeaponbobTimer;

        //Declared leaning initial rotation/position
        [HideInInspector] public Vector3 m_InitialLeanPosition;
        [HideInInspector] public Quaternion m_InitialLeanRotation;

        //Current leaning rotation angle
        [HideInInspector] public float m_CurrentLeanRotationAngle;

        //Current animator move state
        [HideInInspector] public float m_CurrentAnimatorMoveState;

        //Current reload bullet by bullet corountine
        [HideInInspector] public IEnumerator m_BulletByBulletReloadCorountine;
        #endregion

        #region Functions
        private void Start() => SetDefaults();

        private void OnDisable()
        {
            //Reset courountines
            StopAllCoroutines();

            //Reset current states
            this.m_WeaponState = r_WeaponState.IDLE;
            this.m_WeaponAimState = r_WeaponAimState.HIP;
        }

        private void Update()
        {
            HandleWeaponControl();
            HandleWeaponRecoil();
            HandleWeaponMotion();
            HandleWeaponAnimatorMotion();

            if (this.m_WeaponConfig.m_weaponAimSettings.m_aimFunction && this.m_WeaponAimTransform != null) HandleWeaponAim();
            if (this.m_WeaponConfig.m_weaponSwaySettings.m_SwayFunction && this.m_weaponSwayTransform != null) HandleWeaponSway();
            if (this.m_WeaponConfig.m_weaponTiltSettings.m_tiltFunction && this.m_weaponTiltTransform != null) HandleWeaponTilt();
            if (this.m_WeaponConfig.m_WeaponJumpLandSettings.m_jumpLandFunction && this.m_weaponJumpLandTransform != null) HandleWeaponJumpLand();
            if (this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickbackFunction && this.m_WeaponKickbackTransform != null) HandleKickback();
            if (this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanFunction && this.m_weaponLeanTransform != null) HandleWeaponLeaning();
        }
        #endregion

        #region Handling
        private void HandleWeaponControl()
        {
            //Check weapon capabilities
            this.m_CanShoot = Time.time > this.m_FireCooldown && this.m_PlayerController.m_MoveState != r_MoveState.SPRINTING && !this.m_Animator.GetCurrentAnimatorStateInfo(0).IsName(this.m_WeaponConfig.m_AnimationSettings.m_EquipAnimName) && !this.m_Animator.GetCurrentAnimatorStateInfo(0).IsName(this.m_WeaponConfig.m_AnimationSettings.m_UnequipAnimName) && this.m_Ammunation.m_Ammo > 0;
            this.m_CanReload = this.m_Ammunation.m_Ammo < this.m_Ammunation.m_MaxAmmoClip && this.m_WeaponState != r_WeaponState.RELOADING && !this.m_Animator.GetCurrentAnimatorStateInfo(0).IsName(this.m_WeaponConfig.m_AnimationSettings.m_EquipAnimName) && !this.m_Animator.GetCurrentAnimatorStateInfo(0).IsName(this.m_WeaponConfig.m_AnimationSettings.m_UnequipAnimName) && !this.m_Animator.GetCurrentAnimatorStateInfo(0).IsName(this.m_WeaponConfig.m_AnimationSettings.m_EquipAnimName) && !this.m_Animator.GetCurrentAnimatorStateInfo(0).IsName(this.m_WeaponConfig.m_AnimationSettings.m_FireAnimName);
            this.m_canAim = this.m_PlayerController.m_MoveState != r_MoveState.SPRINTING && this.m_CurrentAimTime <= 0;

            if (this.m_CanShoot)
            {
                switch (this.m_WeaponConfig.m_FireModeSettings.m_FireMode)
                {
                    //Check the weapon fire mode
                    case FireMode.SINGLE: if (this.m_PlayerController.m_InputManager.GetFireClick()) SingleFire(); break;
                    case FireMode.AUTOMATIC: if (this.m_PlayerController.m_InputManager.GetFireHold()) SingleFire(); break;
                    case FireMode.BURST: if (this.m_PlayerController.m_InputManager.GetFireClick()) StartCoroutine(BurstFire()); break;
                }
            }

            //Handle reload input
            if (this.m_CanReload && this.m_PlayerController.m_InputManager.GetReloadKey()) OnWeaponReload();

            //Handle Aiming Input
            this.m_WeaponAimState = this.m_PlayerController.m_InputManager.GetAimKey() && this.m_canAim ? this.m_WeaponAimState = r_WeaponAimState.AIMING : this.m_WeaponAimState = r_WeaponAimState.HIP;
        }

        private void HandleWeaponMotion()
        {
            //Crouch motion
            if (this.m_weaponCrouchTransform != null && this.m_WeaponConfig.m_WeaponMotionSettings.m_CrouchFunction)
            {
                //weapon crouch position and rotation
                Vector3 _crouch_position = (this.m_PlayerController.m_MoveState == r_MoveState.CROUCHING || this.m_PlayerController.m_MoveState == r_MoveState.SLIDING) ? this.m_WeaponConfig.m_WeaponMotionSettings.m_CrouchPosition : Vector3.zero;
                Vector3 _crouch_rotation = (this.m_PlayerController.m_MoveState == r_MoveState.CROUCHING || this.m_PlayerController.m_MoveState == r_MoveState.SLIDING) ? this.m_WeaponConfig.m_WeaponMotionSettings.m_CrouchRotation : Vector3.zero;

                //Apply weapon crouch position and rotation
                this.m_weaponCrouchTransform.localPosition = Vector3.Lerp(this.m_weaponCrouchTransform.localPosition, this.m_WeaponAimState == r_WeaponAimState.AIMING ? Vector3.zero : _crouch_position, this.m_WeaponConfig.m_WeaponMotionSettings.m_CrouchSpeed * Time.deltaTime);
                this.m_weaponCrouchTransform.localRotation = Quaternion.Lerp(this.m_weaponCrouchTransform.localRotation, this.m_WeaponAimState == r_WeaponAimState.AIMING ? Quaternion.identity : Quaternion.Euler(_crouch_rotation), this.m_WeaponConfig.m_WeaponMotionSettings.m_CrouchSpeed * Time.deltaTime);
            }

            //Sprint motion
            if (this.m_weaponSprintTransform != null && this.m_WeaponConfig.m_WeaponMotionSettings.m_SprintFunction)
            {
                //weapon crouch position and rotation
                Vector3 _sprint_position = this.m_PlayerController.m_MoveState == r_MoveState.SPRINTING ? this.m_WeaponConfig.m_WeaponMotionSettings.m_SprintPosition : Vector3.zero;
                Vector3 _sprint_rotation = this.m_PlayerController.m_MoveState == r_MoveState.SPRINTING ? this.m_WeaponConfig.m_WeaponMotionSettings.m_SprintRotation : Vector3.zero;

                //Apply weapon crouch position and rotation
                this.m_weaponSprintTransform.localPosition = Vector3.Lerp(this.m_weaponSprintTransform.localPosition, _sprint_position, this.m_WeaponConfig.m_WeaponMotionSettings.m_SprintSpeed * Time.deltaTime);
                this.m_weaponSprintTransform.localRotation = Quaternion.Lerp(this.m_weaponSprintTransform.localRotation, Quaternion.Euler(_sprint_rotation), this.m_WeaponConfig.m_WeaponMotionSettings.m_SprintSpeed * Time.deltaTime);
            }

            //Bob motion
            if (this.m_weaponBobTransform != null && this.m_WeaponConfig.m_WeaponMotionSettings.m_BobFunction)
            {
                //Check if we are moving to handle headbobbing
                if (Mathf.Abs(this.m_PlayerController.m_moveDirection.x) > 0.1f || Mathf.Abs(this.m_PlayerController.m_moveDirection.z) > 0.1f)
                {
                    //Return if we are not grounded
                    if (!this.m_PlayerController.m_CharacterController.isGrounded) return;

                    //Increase headbob timer while we are moving
                    this.m_WeaponbobTimer += Time.deltaTime * GetWeaponBobState(this.m_PlayerController.m_MoveState).m_WeaponbobSpeed;

                    //Calculate our desired headbob position
                    Vector3 _DesiredHeadbob = new Vector3(Mathf.Cos(this.m_WeaponbobTimer / 2) * GetWeaponBobState(this.m_PlayerController.m_MoveState).m_WeaponbobAmount * -1, Mathf.Sin(this.m_WeaponbobTimer) * GetWeaponBobState(this.m_PlayerController.m_MoveState).m_WeaponbobAmount, this.m_weaponBobTransform.localPosition.x);

                    //Lerp our headbob holder position to our desired headbob position
                    this.m_weaponBobTransform.localPosition = Vector3.Lerp(this.m_weaponBobTransform.localPosition, _DesiredHeadbob, Time.deltaTime * 8f);
                }
                else
                {
                    //If we are not moving, return our headbob holder position to zero
                    if (this.m_weaponBobTransform.localPosition != Vector3.zero)
                        this.m_weaponBobTransform.localPosition = Vector3.Lerp(this.m_weaponBobTransform.localPosition, Vector3.zero, Time.deltaTime * 8f);
                    else return;
                }
            }
        }

        private void HandleWeaponAnimatorMotion()
        {
            float _move_state = 0;

            if (this.m_PlayerController.m_MoveState == r_MoveState.IDLE || this.m_WeaponAimState == r_WeaponAimState.AIMING)
            {
                //0 = Idle
                _move_state = 0;
            }
            else
            {
                if (this.m_PlayerController.m_MoveState == r_MoveState.WALKING)
                {
                    //1 = Walking
                    _move_state = 1;
                }
                else if (this.m_PlayerController.m_MoveState == r_MoveState.SPRINTING)
                {
                    //2 = Sprinting
                    _move_state = 2;
                }
            }

            //Lerp movement state for animator
            this.m_CurrentAnimatorMoveState = Mathf.Lerp(this.m_CurrentAnimatorMoveState, _move_state, Time.deltaTime * this.m_WeaponConfig.m_AnimationSettings.m_AnimationTransitionSpeed);

            //Set move state animator
            PlayAnimation(r_AnimationType.SETFLOAT, "MoveState", this.m_CurrentAnimatorMoveState, false);
        }

        private void HandleWeaponAim()
        {
            //Set camera state to aiming
            this.m_PlayerCamera.m_CameraState = m_WeaponAimState == r_WeaponAimState.AIMING ? r_CameraState.AIMING : r_CameraState.HIP;

            //Set weapon camera data for aim settings
            SetWeaponCameraData();

            //Weapon aim position and rotation
            Vector3 _aimPosition = this.m_WeaponAimState == r_WeaponAimState.AIMING ? this.m_WeaponConfig.m_weaponAimSettings.m_aimPosition : this.m_WeaponConfig.m_weaponAimSettings.m_hipPosition;
            Vector3 _aimRotation = this.m_WeaponAimState == r_WeaponAimState.AIMING ? this.m_WeaponConfig.m_weaponAimSettings.m_aimRotation : this.m_WeaponConfig.m_weaponAimSettings.m_hipRotation;

            //Apply weapon aim position and rotation
            this.m_WeaponAimTransform.localPosition = Vector3.Lerp(this.m_WeaponAimTransform.localPosition, _aimPosition, this.m_WeaponConfig.m_weaponAimSettings.m_aimSpeed * Time.deltaTime);
            this.m_WeaponAimTransform.localRotation = Quaternion.Lerp(this.m_WeaponAimTransform.localRotation, Quaternion.Euler(_aimRotation), this.m_WeaponConfig.m_weaponAimSettings.m_aimSpeedReturn * Time.deltaTime);

            //Check when set the scope texture, based on if the current aim position is close to the target aim position
            if (this.m_WeaponConfig.m_weaponScopeSettings.m_scopeFunction)
            {
                this.m_onAimingScope = Vector3.Distance(this.m_WeaponAimTransform.localPosition, this.m_WeaponConfig.m_weaponAimSettings.m_aimPosition) < this.m_WeaponConfig.m_weaponScopeSettings.m_scopeNearAimPosition;

                //Handle hidden objects on aim
                SetHideMeshes(!this.m_onAimingScope);
            }

            //Handle aim controlable speed after sprinting
            this.m_CurrentAimTime = this.m_PlayerController.m_MoveState == r_MoveState.SPRINTING ? 1 : (this.m_CurrentAimTime > 0) ? this.m_CurrentAimTime -= Time.deltaTime * m_WeaponConfig.m_weaponAimSettings.m_AimResetAfterSprintingSpeed : 0;
        }

        private void HandleWeaponSway()
        {
            //Handle weapon sway multiplier
            float _swayMultiplier = this.m_WeaponAimState == r_WeaponAimState.AIMING ? this.m_WeaponConfig.m_weaponSwaySettings.m_swayMultiplerAim : this.m_WeaponConfig.m_weaponSwaySettings.m_swayMultiplier;

            //weapon Sway rotation
            Quaternion _rotationX = Quaternion.AngleAxis(-this.m_PlayerController.m_InputManager.GetMouseY() * _swayMultiplier, Vector3.right);
            Quaternion _rotationY = Quaternion.AngleAxis(this.m_PlayerController.m_InputManager.GetMouseX() * _swayMultiplier, Vector3.up);

            //Apply weapon sway
            this.m_weaponSwayTransform.localRotation = Quaternion.Slerp(this.m_weaponSwayTransform.localRotation, this.m_initialSwayRotation * (_rotationX * _rotationY), this.m_WeaponConfig.m_weaponSwaySettings.m_swaySmoothness * Time.deltaTime);
        }

        private void HandleWeaponTilt()
        {
            //Handle weapon tilt
            Quaternion _tiltRotation = this.m_PlayerController.m_grounded ? Quaternion.Euler(0, 0, -this.m_PlayerController.m_InputManager.GetHorizontal() * this.m_WeaponConfig.m_weaponTiltSettings.m_tiltAngle) : Quaternion.identity;

            //Apply weapon tilt rotation
            this.m_weaponTiltTransform.localRotation = Quaternion.Slerp(this.m_weaponTiltTransform.localRotation, _tiltRotation, this.m_WeaponConfig.m_weaponTiltSettings.m_tiltSmoothness * Time.deltaTime);
        }

        private void HandleWeaponJumpLand()
        {
            //Return weapon jump land effect position and rotation
            this.m_weaponJumpLandTransform.localPosition = Vector3.Lerp(this.m_weaponJumpLandTransform.localPosition, Vector3.zero, Time.deltaTime * this.m_WeaponConfig.m_WeaponJumpLandSettings.m_effectDurationReturn);
            this.m_weaponJumpLandTransform.localRotation = Quaternion.Slerp(this.m_weaponJumpLandTransform.localRotation, Quaternion.identity, Time.deltaTime * this.m_WeaponConfig.m_WeaponJumpLandSettings.m_effectDurationReturn);
        }

        private void HandleWeaponLeaning()
        {
            if (this.m_PlayerController.m_InputManager.GetLeanLeftKey())
            {
                //Move lean angle to left
                this.m_CurrentLeanRotationAngle = Mathf.MoveTowardsAngle(this.m_CurrentLeanRotationAngle, this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanRotationAngle, this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanRotationAngleSpeed * Time.deltaTime);

                //Set camera lean position
                ChangeWeaponLeanPosition(this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanPositionLeft);
            }
            else if (this.m_PlayerController.m_InputManager.GetLeanRightKey())
            {
                //Move lean angle to right
                this.m_CurrentLeanRotationAngle = Mathf.MoveTowardsAngle(this.m_CurrentLeanRotationAngle, -this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanRotationAngle, this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanRotationAngleSpeed * Time.deltaTime);

                //Set camera lean position
                ChangeWeaponLeanPosition(this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanPositionRight);
            }
            else
            {
                //Move lean angle to zero
                this.m_CurrentLeanRotationAngle = Mathf.MoveTowardsAngle(this.m_CurrentLeanRotationAngle, 0, this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanRotationAngleSpeed * Time.deltaTime);

                //Set camera lean position to zero
                ChangeWeaponLeanPosition(Vector3.zero);
            }

            //Set lean rotation
            this.m_weaponLeanTransform.localEulerAngles = new Vector3(0, 0, this.m_InitialLeanRotation.z + this.m_CurrentLeanRotationAngle);
        }

        private void HandleWeaponRecoil()
        {
            //Lerp to zero to reset recoil
            this.m_targetRecoil = Vector3.Lerp(this.m_targetRecoil, Vector3.zero, Time.deltaTime * this.m_WeaponConfig.m_weaponRecoilSettings.m_recoilTimeReturn);

            //Lerp current recoil to target recoil
            this.m_currentRecoil = Vector3.Slerp(this.m_currentRecoil, this.m_targetRecoil, Time.fixedDeltaTime * this.m_WeaponConfig.m_weaponRecoilSettings.m_recoilTime);

            //Apply recoil rotation
            this.m_PlayerCamera.m_RecoilHolder.localRotation = Quaternion.Euler(this.m_currentRecoil);
        }

        private void HandleKickback()
        {
            this.m_WeaponKickbackTransform.localPosition = Vector3.Lerp(this.m_WeaponKickbackTransform.localPosition, Vector3.zero, Time.deltaTime * this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackReturnSpeed);
            this.m_WeaponKickbackTransform.localEulerAngles = Vector3.Lerp(this.m_WeaponKickbackTransform.localEulerAngles, Vector3.zero, this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackReturnSpeed);
        }
        #endregion

        #region Actions
        private void SetDefaults()
        {
            //Reset fire cooldown
            m_FireCooldown = 0;

            //Save sway origin local rotation
            this.m_initialSwayRotation = this.m_weaponSwayTransform.localRotation;

            //Save initial lean position
            this.m_InitialLeanPosition = this.m_weaponLeanTransform.localPosition;

            //Save initial lean rotation
            this.m_InitialLeanRotation = this.m_weaponLeanTransform.localRotation;

            //Disable first person weapon mesh shadows
            DisableMeshShadow();
        }

        public void EquipWeapon()
        {
            //Equip Animation
            PlayAnimation(r_AnimationType.PLAY, this.m_WeaponConfig.m_AnimationSettings.m_EquipAnimName, 0, false);

            //Equip Sound
            PlaySound(this.m_WeaponConfig.m_WeaponSound.m_EquipSoundName, this.m_WeaponConfig.m_WeaponSound.m_EquipSoundVolume, true);
        }

        public void UnEquipWeapon()
        {
            //Equip Animation
            PlayAnimation(r_AnimationType.PLAY, this.m_WeaponConfig.m_AnimationSettings.m_UnequipAnimName, 0, false);

            //Equip Sound
            PlaySound(this.m_WeaponConfig.m_WeaponSound.m_UnequipSoundName, this.m_WeaponConfig.m_WeaponSound.m_UnequipSoundVolume, true);
        }

        private void SingleFire()
        {
            //Break reload corountine if reload type is bullet by bullet
            if (this.m_WeaponConfig.m_ReloadSettings.m_reloadType == r_ReloadType.SINGLE && this.m_BulletByBulletReloadCorountine != null)
            {
                //Stop corountine
                StopCoroutine(this.m_BulletByBulletReloadCorountine);

                //reset corountine
                this.m_BulletByBulletReloadCorountine = null;
            }

            //Set Weapon State
            this.m_WeaponState = r_WeaponState.FIRING;

            //Play fire animation
            PlayAnimation(r_AnimationType.PLAY, this.m_WeaponConfig.m_AnimationSettings.m_FireAnimName, 0, false);

            //Third person
            this.m_PlayerController.m_ThirdPersonManager.OnWeaponFire();

            //Increase crosshair size
            this.m_PlayerController.m_PlayerUI.OnCrosshairShoot(this.m_WeaponConfig.m_weaponCrosshairSettings.m_CrosshairIncreaseSize);

            //Recoil
            SetRecoil();

            if (this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickbackFunction) SetKickback();

            //Play fire sound
            PlaySound(this.m_WeaponConfig.m_WeaponSound.m_FireSoundName, this.m_WeaponConfig.m_WeaponSound.m_FireSoundVolume, true);

            //Muzzle flash
            SetMuzzleflash();

            //Eject shelll
            StartCoroutine(SetShellEject());

            //Take off ammo
            this.m_Ammunation.m_Ammo--;

            //Check auto reload
            if (this.m_WeaponConfig.m_ReloadSettings.m_AutoReload && this.m_Ammunation.m_Ammo == 0 && this.m_weaponManager.m_TotalAmmunation > 0)
                OnWeaponReload();

            //Reset fire rate cooldown
            this.m_FireCooldown = Time.time + this.m_WeaponConfig.m_FireModeSettings.m_FireRate;

            //Check how many shot fragments
            for (int i = 0; i < this.m_WeaponConfig.m_BulletSettings.m_ShotFragments; i++)
            {
                //Shoot with raycast or physic
                switch (this.m_WeaponConfig.m_BulletSettings.m_BulletType)
                {
                    case BulletType.RAYCAST: OnRaycastShoot(); break;
                    case BulletType.PHYSIC: OnPhysicShoot(); break;
                }
            }
        }

        private IEnumerator BurstFire()
        {
            //Aply burst delay
            yield return new WaitForSeconds(this.m_WeaponConfig.m_FireModeSettings.m_BurstModeDelay);

            if (this.m_BurstModeCount < this.m_WeaponConfig.m_FireModeSettings.m_BurstModeCount)
            {
                //Add burst mode count
                this.m_BurstModeCount++;

                //Fire and re-check the burst mode for next shot
                SingleFire();
                StartCoroutine(BurstFire());
            }
            else if (this.m_BurstModeCount >= this.m_WeaponConfig.m_FireModeSettings.m_BurstModeCount) this.m_BurstModeCount = 0;
        }

        private void OnRaycastShoot()
        {
            if (Physics.Raycast(this.m_PlayerCamera.m_PlayerCamera.transform.position, GetSpread() * Vector3.forward, out RaycastHit _hit, this.m_WeaponConfig.m_BulletSettings.m_ShootDistance, this.m_LayerMask.value))
            {
                r_ThirdPersonBodyPart _BodyPart = _hit.transform.GetComponent<r_ThirdPersonBodyPart>();

                SetBulletTrail(_hit.point);

                //Check if enemy hit
                if (_BodyPart != null)
                {
                    //On enemy body part hit
                    OnEnemyHit(_BodyPart, _hit);
                }
                else
                {
                    //On hit collider
                    if (this.m_PlayerController.m_PlayerAudio.GetBulletImpact(_hit.collider, _hit.point) != null)
                    {
                        //Instantiate bullet impact
                        GameObject _bulletImpact = (GameObject)PhotonNetwork.Instantiate("Impacts/" + this.m_PlayerController.m_PlayerAudio.GetBulletImpact(_hit.collider, _hit.point).name, _hit.point + (_hit.normal * 0.01f), Quaternion.LookRotation(_hit.normal));

                        //Bullet impact parent
                        _bulletImpact.transform.parent = _hit.collider.transform.root.parent;

                        //Destroy bullet impact
                        StartCoroutine(PhotonDestroy(_bulletImpact, 2f));
                    }

                    //Check rigidbody hit
                    if (_hit.transform.GetComponent<Rigidbody>() != null)
                    {
                        //Add force to rigidbody object
                        _hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * this.m_WeaponConfig.m_BulletSettings.m_bulletImpactForce, _hit.point);
                    }
                }
            }
        }

        private void OnPhysicShoot()
        {
            //NEXT UPDATE
        }

        private void OnEnemyHit(r_ThirdPersonBodyPart _enemy, RaycastHit _hit)
        {
            if (_enemy.m_PlayerHealth != null)
            {
                if (!_enemy.m_PlayerHealth.m_IsDeath)
                {
                    if (_enemy.m_PlayerHealth.m_Health > 0)
                    {
                        //Take off damage from enemy player
                        _enemy.m_PlayerHealth.DecreaseHealth(PhotonNetwork.LocalPlayer.NickName, GetBodyPartWeaponDamage(_enemy.m_BodyParts.m_BodyPartType), this.m_PlayerController.transform.position, this.m_PlayerController.m_PlayerHealth.m_Health, this.m_WeaponConfig.m_WeaponInformationSettings.m_WeaponName);
                    }

                    //Check if the enemy is eliminated
                    if (_enemy.m_PlayerHealth.m_Health <= 0)
                    {
                        //play body impact effect sound
                        PlaySound(this.m_WeaponConfig.m_WeaponSound.m_EnemyKilledSoundName, this.m_WeaponConfig.m_WeaponSound.m_EnemyKilledVolume, false);

                        this.m_PlayerController.m_PlayerUI.SetHitmarker(true);
                        this.m_PlayerController.m_PlayerUI.SetKillmessage(_enemy.m_PlayerHealth.m_PlayerController.m_PhotonView.name);

                        r_KillfeedManager.instance.AddKillfeed(PhotonNetwork.LocalPlayer, this.m_WeaponConfig.m_WeaponInformationSettings.m_WeaponName, _enemy.m_PlayerHealth.m_PlayerController.m_PhotonView.Owner);

                        if (this.m_PlayerController.m_PhotonView.IsMine)
                        {
                            //Enemy killed adding kill
                            r_PlayerProperties.WriteInt(PhotonNetwork.LocalPlayer, r_PlayerProperties.KillsPropertyKey, (int)r_PlayerProperties.GetPlayerKills(PhotonNetwork.LocalPlayer) + 1);
                        }
                    }
                    else
                    {
                        this.m_PlayerController.m_PlayerUI.SetHitmarker(false);
                    }
                }
            }

            //Add force to ragdoll
            _enemy.m_BodyParts.m_BodyPartRigidbody.AddForceAtPosition(-_hit.transform.forward * this.m_WeaponConfig.m_BulletSettings.m_ragdollImpactForce, _hit.point);

            //Enemy damage bullet impact
            if (this.m_WeaponConfig.m_weaponDamageEffectSettings.m_bodyImpact != null)
            {
                //Instantiate damage effect, for example: blood
                GameObject _damageEffect = (GameObject)PhotonNetwork.Instantiate("Impacts/" + this.m_WeaponConfig.m_weaponDamageEffectSettings.m_bodyImpact.name, _hit.point, Quaternion.LookRotation(_hit.normal));

                //Clean up damage effect
                StartCoroutine(PhotonDestroy(_damageEffect, 4f));
            }

            //play body impact effect sound
            PlaySound(this.m_WeaponConfig.m_WeaponSound.m_BodyImpactSoundName, this.m_WeaponConfig.m_WeaponSound.m_BodyImpactVolume, false);
        }

        private void OnWeaponReload()
        {
            switch (this.m_WeaponConfig.m_ReloadSettings.m_reloadType)
            {
                //Magazine reload
                case r_ReloadType.MAG: StartCoroutine(ReloadMagazine()); break;

                //Bullet by bullet reload for with example shotgun
                case r_ReloadType.SINGLE: this.m_BulletByBulletReloadCorountine = ReloadBulletByBullet(); StartCoroutine(this.m_BulletByBulletReloadCorountine); break;
            }

            //Third Person Reload
            this.m_PlayerController.m_ThirdPersonManager.OnWeaponReload();
        }

        private IEnumerator ReloadMagazine()
        {
            //Set Weapon State
            this.m_WeaponState = r_WeaponState.RELOADING;

            //Set reload animation trigger
            PlayAnimation(r_AnimationType.SETTRIGGER, this.m_WeaponConfig.m_AnimationSettings.m_ReloadAnimName, 0, false);

            //Play fire sound
            PlaySound(this.m_WeaponConfig.m_WeaponSound.m_ReloadSoundName, this.m_WeaponConfig.m_WeaponSound.m_ReloadSoundVolume, true);

            //Empty ammo clip and add it to total ammo
            this.m_weaponManager.m_TotalAmmunation += this.m_Ammunation.m_Ammo;
            this.m_Ammunation.m_Ammo = 0;

            yield return new WaitForSeconds(this.m_WeaponConfig.m_ReloadSettings.m_ReloadDuration);

            if (this.m_Ammunation.m_MaxAmmoClip >= this.m_weaponManager.m_TotalAmmunation)
            {
                //Fill ammo clip with rest of our total ammo
                this.m_Ammunation.m_Ammo = this.m_weaponManager.m_TotalAmmunation;
                this.m_weaponManager.m_TotalAmmunation -= this.m_Ammunation.m_Ammo;
            }
            else
            {
                //Full ammo clip and take off total ammo
                this.m_Ammunation.m_Ammo = this.m_Ammunation.m_MaxAmmoClip;
                this.m_weaponManager.m_TotalAmmunation -= this.m_Ammunation.m_MaxAmmoClip;
            }

            //Reset weapon state
            this.m_WeaponState = r_WeaponState.IDLE;
        }

        private IEnumerator ReloadBulletByBullet()
        {
            bool _reload_started = true;

            //Start bullet by bullet reload first person
            PlayAnimation(r_AnimationType.SETTRIGGER, this.m_WeaponConfig.m_AnimationSettings.m_ReloadStartTriggerName, 0, _reload_started);

            //Start bullet by bullet reload third person
            this.m_PlayerController.m_ThirdPersonManager.PlayAnimation(r_AnimationType.SETTRIGGER, this.m_WeaponConfig.m_AnimationSettings.m_ReloadStartTriggerName, false, this.m_PlayerController.m_ThirdPersonManager.m_WeaponAnimationLayer);

            yield return new WaitForSeconds(this.m_WeaponConfig.m_ReloadSettings.m_StartReloadDuration);

            while (this.m_Ammunation.m_Ammo < this.m_Ammunation.m_MaxAmmoClip && this.m_weaponManager.m_TotalAmmunation > 0)
            {
                //Set Weapon State
                this.m_WeaponState = r_WeaponState.RELOADING;

                //Set reload animation trigger First Person
                PlayAnimation(r_AnimationType.SETTRIGGER, this.m_WeaponConfig.m_AnimationSettings.m_ReloadAnimName, 0, false);

                //Set reload animation trigger Third Person
                this.m_PlayerController.m_ThirdPersonManager.PlayAnimation(r_AnimationType.SETTRIGGER, this.m_WeaponConfig.m_AnimationSettings.m_ReloadAnimName, false, this.m_PlayerController.m_ThirdPersonManager.m_WeaponAnimationLayer);

                yield return new WaitForSeconds(this.m_WeaponConfig.m_ReloadSettings.m_ReloadDuration);

                //Play fire sound
                PlaySound(this.m_WeaponConfig.m_WeaponSound.m_ReloadSoundName, this.m_WeaponConfig.m_WeaponSound.m_ReloadSoundVolume, true);

                //Add ammo
                this.m_Ammunation.m_Ammo++;

                //Decrease ammo
                this.m_weaponManager.m_TotalAmmunation--;
            }

            //Finish reload
            if (this.m_Ammunation.m_Ammo >= this.m_Ammunation.m_MaxAmmoClip)
            {
                //First Person
                PlayAnimation(r_AnimationType.SETTRIGGER, this.m_WeaponConfig.m_AnimationSettings.m_ReloadFinishTriggerName, 0, _reload_started);

                //Third person
                this.m_PlayerController.m_ThirdPersonManager.PlayAnimation(r_AnimationType.SETTRIGGER, this.m_WeaponConfig.m_AnimationSettings.m_ReloadFinishTriggerName, false, this.m_PlayerController.m_ThirdPersonManager.m_WeaponAnimationLayer);
            }

            //Reset weapon state
            this.m_WeaponState = r_WeaponState.IDLE;

            //Reset bullet by bullet corountine
            this.m_BulletByBulletReloadCorountine = null;
        }

        private void SetMuzzleflash()
        {
            // Instantiates a bullet shell.
            GameObject _muzzle = Instantiate(this.m_WeaponConfig.m_weaponFXSettings.m_muzzleFlash, this.m_muzzlePointTransform.position, this.m_muzzlePointTransform.rotation);

            //Destroy muzzle
            Destroy(_muzzle, this.m_WeaponConfig.m_weaponFXSettings.m_muzzleLifetime);
        }

        private IEnumerator SetShellEject()
        {
            yield return new WaitForSeconds(this.m_WeaponConfig.m_weaponFXSettings.m_shellAfterTime);

            //Instantiates a bullet shell
            Rigidbody _shell = (Rigidbody)Instantiate(this.m_WeaponConfig.m_weaponFXSettings.m_bulletShell, this.m_shellPointTransform.position, this.m_shellPointTransform.rotation);

            //Apply bullet shell force
            _shell.velocity = _shell.transform.forward * this.m_WeaponConfig.m_weaponFXSettings.m_shellEjectForce;

            //Random bullet shell rotation
            Vector3 _shellRotation = new Vector3(Random.Range(-this.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.x, this.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.x), Random.Range(-this.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.y, this.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.y), Random.Range(-this.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.z, this.m_WeaponConfig.m_weaponFXSettings.m_shellRandomRotation.z));

            //Apply bullet shell rotation
            _shell.transform.localRotation = Quaternion.Euler(this.m_WeaponConfig.m_weaponFXSettings.m_shellStartRotation + _shellRotation);

            //Destroy after 1 second.
            Destroy(_shell, this.m_WeaponConfig.m_weaponFXSettings.m_shellLifeTime);

            //Shell Eject Third Person
            this.m_PlayerController.m_ThirdPersonManager.OnShellEject();
        }

        private void SetBulletTrail(Vector3 _hitPoint)
        {
            GameObject _trail = Instantiate(this.m_WeaponConfig.m_weaponFXSettings.m_BulletTrailPrefab, this.m_muzzlePointTransform.position, Quaternion.identity);

            LineRenderer _line = _trail.GetComponent<LineRenderer>();

            if (_line != null)
            {
                _line.SetPosition(0, this.m_muzzlePointTransform.position);
                _line.SetPosition(1, _hitPoint);

                Destroy(_trail, 1f);
            }
        }

        public void onWeaponFall(bool _onJump)
        {
            //Start corountine weapon fall effect if the function is enabled
            if (this.m_WeaponConfig.m_WeaponJumpLandSettings.m_jumpLandFunction)
            {
                if (!this.m_IsWeaponFallEffect)
                    StartCoroutine(OnWeaponFallEffect(_onJump));
            }
        }

        private IEnumerator OnWeaponFallEffect(bool _onJump)
        {
            this.m_IsWeaponFallEffect = true;

            //Start time on zero
            float _time = 0.0f;

            //Declare start position and rotation
            Vector3 _startPosition = this.m_weaponJumpLandTransform.localPosition;
            Quaternion _startRotation = this.m_weaponJumpLandTransform.localRotation;

            while (_time < this.m_WeaponConfig.m_WeaponJumpLandSettings.m_effectDuration)
            {
                //Increase time with delta time
                _time += Time.deltaTime;

                //Check jump or land position and rotation
                Vector3 _endPosition = _onJump ? _startPosition + this.m_WeaponConfig.m_WeaponJumpLandSettings.m_jumpPosition : _startPosition + this.m_WeaponConfig.m_WeaponJumpLandSettings.m_landPosition;
                Quaternion _endRotation = _onJump ? _startRotation * Quaternion.Euler(this.m_WeaponConfig.m_WeaponJumpLandSettings.m_jumpRotation) : _startRotation * Quaternion.Euler(this.m_WeaponConfig.m_WeaponJumpLandSettings.m_landRotation);

                //Apply position and rotation
                this.m_weaponJumpLandTransform.localPosition = Vector3.Lerp(this.m_weaponJumpLandTransform.localPosition, _endPosition, _time);
                this.m_weaponJumpLandTransform.localRotation = Quaternion.Slerp(this.m_weaponJumpLandTransform.localRotation, _endRotation, _time);

                yield return null;
            }

            this.m_IsWeaponFallEffect = false;
        }

        private void SetRecoil()
        {
            //Add recoil
            this.m_targetRecoil += new Vector3(-this.m_WeaponConfig.m_weaponRecoilSettings.m_recoilEuler.x, Random.Range(-this.m_WeaponConfig.m_weaponRecoilSettings.m_recoilEuler.y, this.m_WeaponConfig.m_weaponRecoilSettings.m_recoilEuler.y), Random.Range(-this.m_WeaponConfig.m_weaponRecoilSettings.m_recoilEuler.z, this.m_WeaponConfig.m_weaponRecoilSettings.m_recoilEuler.y));
        }

        private void SetKickback()
        {
            if (this.m_WeaponKickbackTransform == null) return;

            this.m_WeaponKickbackTransform.localPosition = this.m_WeaponKickbackTransform.localPosition + (this.m_WeaponAimState == r_WeaponAimState.AIMING ? (this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackPositionAmount * this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackAimStabilizationMultiplier) : this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackPositionAmount);
            this.m_WeaponKickbackTransform.localEulerAngles = this.m_WeaponKickbackTransform.localEulerAngles + (this.m_WeaponAimState == r_WeaponAimState.AIMING ? (this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackRotationAmount * this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackAimStabilizationMultiplier) : this.m_WeaponConfig.m_WeaponKickbackSettings.m_KickBackRotationAmount);
        }

        private void PlayAnimation(r_AnimationType _type, string _animationName, float _value, bool _setBool)
        {
            switch (_type)
            {
                case r_AnimationType.PLAY: this.m_Animator.Play(_animationName, 0, 0f); break;
                case r_AnimationType.SETTRIGGER: this.m_Animator.SetTrigger(_animationName); break;
                case r_AnimationType.SETBOOL: this.m_Animator.SetBool(_animationName, _setBool); break;
                case r_AnimationType.SETFLOAT: this.m_Animator.SetFloat(_animationName, _value); break;
            }
        }

        private void PlaySound(string _audioClipName, float _audioVolume, bool _networked)
        {
            if (this.m_PlayerController.m_PhotonView.IsMine)
            {
                //Play audio over network
                this.m_PlayerAudio.OnWeaponAudioPlay(this.m_WeaponConfig.m_WeaponInformationSettings.m_WeaponName, _audioClipName, _audioVolume, _networked);
            }
        }

        private void ChangeWeaponLeanPosition(Vector3 _lean_position)
        {
            //Lerp to lean position
            this.m_weaponLeanTransform.localPosition = Vector3.Lerp(this.m_weaponLeanTransform.localPosition, this.m_InitialLeanPosition + _lean_position, this.m_WeaponConfig.m_WeaponLeanSettings.m_LeanPositionAngleSpeed * Time.deltaTime);
        }

        private void SetHideMeshes(bool _state)
        {
            if (this.m_weaponMeshRenderers.Length == 0) return;

            //Hide or make objects visible
            foreach (Renderer _mesh in this.m_weaponMeshRenderers) _mesh.gameObject.SetActive(_state);
        }

        private void DisableMeshShadow()
        {
            if (this.m_weaponMeshRenderers.Length == 0) return;

            //Hide or make objects visible
            foreach (Renderer _mesh in this.m_weaponMeshRenderers) _mesh.shadowCastingMode = ShadowCastingMode.Off;
        }

        private void SetWeaponCameraData()
        {
            //Save player camera data
            this.m_PlayerCamera.m_WeaponCameraFOV = this.m_WeaponConfig.m_weaponAimSettings.m_aimFOV;
            this.m_PlayerCamera.m_WeaponCameraFOVSpeed = this.m_WeaponConfig.m_weaponAimSettings.m_aimFOVSpeed;
        }

        private IEnumerator PhotonDestroy(GameObject _object, float _time)
        {
            if (_object == null) yield return true;

            yield return new WaitForSeconds(_time);

            PhotonNetwork.Destroy(_object);
        }
        #endregion

        #region Get
        private Quaternion GetSpread()
        {
            //Check min spread and max spread based on aiming and hip
            float _MinSpread = this.m_WeaponAimState == r_WeaponAimState.AIMING ? -this.m_WeaponConfig.m_BulletSettings.m_AimSpread : -this.m_WeaponConfig.m_BulletSettings.m_HipSpread;
            float _MaxSpread = this.m_WeaponAimState == r_WeaponAimState.AIMING ? this.m_WeaponConfig.m_BulletSettings.m_AimSpread : this.m_WeaponConfig.m_BulletSettings.m_HipSpread;

            //Apply and return the min/max spread
            return Quaternion.RotateTowards(Quaternion.LookRotation(this.m_PlayerCamera.m_PlayerCamera.transform.forward), Random.rotation, Random.Range(_MinSpread, _MaxSpread));
        }

        private r_WeaponBobMotionSettings GetWeaponBobState(r_MoveState _move_state) => this.m_WeaponConfig.m_WeaponMotionSettings.m_WeaponBobSettings.Find(x => x.m_MoveState == _move_state);

        private float GetBodyPartWeaponDamage(r_BodyPartType _bodyPart) => this.m_WeaponConfig.m_WeaponDamageParts.Find(x => x.m_bodyPartType == _bodyPart).m_weaponDamage;
        #endregion

        #region Custom
        private void OnGUI()
        {
            if (this.m_onAimingScope)
            {
                //Draw scope texture on full screen
                GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), this.m_WeaponConfig.m_weaponScopeSettings.m_scopeTexture2D);
            }
        }
        #endregion
    }
}