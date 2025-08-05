using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;

namespace ForceCodeFPS
{
    public class r_PlayerUI : MonoBehaviour
    {
        #region References
        public r_PlayerController m_PlayerController;
        #endregion

        #region Public variables
        [Header("UI Base Configuration")]
        public r_PlayerUIBase m_UIBase;

        [Header("Player HUD")]
        public GameObject m_PlayerHUD;

        [Header("Killmessage UI")]
        public Transform m_KillMessageContent;
        public GameObject m_KillMessagePrefab;

        [Header("Crosshair UI")]
        public RectTransform m_CrosshairRectTransform;
        public RawImage m_CrosshairCenterPointImage;

        [Header("Hitmarker UI")]
        public Image m_HitmarkerImage;

        [Header("Vitals UI")]
        public Text m_StaminaText;

        [Header("Bloody UI")]
        public Image m_BloodyScreenImage;
        public Image m_DamageIndicatorImage;

        [Space(10)]

        public Text m_HealthText;
        public Image m_HealthImage;

        [Header("Weapon UI")]
        public RawImage m_LocalWeaponPreview;
        public Text m_LocalWeaponName;
        public Text m_LocalWeaponAmmo;

        [Space(10)]

        public Text m_WeaponPickupText;
        public RawImage m_WeaponPickupImage;
        #endregion

        #region Private variables
        //Current bloodyscreen alpha
        [HideInInspector] public float m_CurrentBloodyAlpha;

        //Current damage indicator alpha
        [HideInInspector] public float m_CurrentIndicatorAlpha;

        //Current Hitmarker alpha
        [HideInInspector] public float m_CurrentHitmarkerTime;
        [HideInInspector] public float m_CurrentHitmarkerPosition;
        [HideInInspector] public float m_CurrentHitmarkerRotation;

        //Current Crosshair data
        [HideInInspector] public float m_CurrentCrosshairSize;
        [HideInInspector] public float m_CurrentCrosshairRotation;
        [HideInInspector] public float m_CurrentCrosshairShootMultiplier;

        //Last enemy position
        [HideInInspector] public Vector3 m_LastEnemyPosition;
        #endregion

        #region Functions
        private void Start() => SetDefaults();

        private void Update()
        {
            HandleHitmarker();
            HandleWeaponUI();
            HandleStaminaUI();
            HandleCrosshairState();

            if (this.m_UIBase.m_BloodyScreenFeature) HandleBloodyScreen();
            if (this.m_UIBase.m_DamageIndicatorFeature) HandleDamageIndicator();
        }
        #endregion

        #region Handling
        private void HandleBloodyScreen()
        {
            //If damage indicator is active
            if (this.m_CurrentBloodyAlpha > 0.0f)
            {
                //Decrease damage indicator image alpha, this makes it fade out
                this.m_CurrentBloodyAlpha -= Time.deltaTime;

                //Set damage indicator image alpha
                SetBloodyScreenAlpha(this.m_CurrentBloodyAlpha);
            }
        }

        private void HandleDamageIndicator()
        {
            //If bloody screen is active
            if (this.m_CurrentIndicatorAlpha > 0.0f)
            {
                //Decrease bloody screen image alpha, this makes it fade out
                this.m_CurrentIndicatorAlpha -= Time.deltaTime;

                //Set bloody screen image alpha
                SetDamageIndicatorAlpha(this.m_CurrentIndicatorAlpha);

                // Get the direction from the player to the damage indicator
                Vector3 _direction = new Vector3((m_LastEnemyPosition - this.m_PlayerController.transform.position).x, 0.0f, (this.m_LastEnemyPosition - this.m_PlayerController.transform.position).z).normalized;

                // Calculate the angle between the forward direction of the player and the damage indicator direction
                Vector3 _cross = Vector3.Cross(this.m_PlayerController.transform.forward, _direction);
                float _angle = Vector3.Angle(this.m_PlayerController.transform.forward, _direction) * Mathf.Sign(_cross.y);

                // Apply the scene rotation to the angle
                _angle += transform.rotation.eulerAngles.y;

                // Rotate the damage indicator around the Y-axis to face the direction of the damage
                this.m_DamageIndicatorImage.transform.eulerAngles = new Vector3(0, 0, -_angle);
            }
        }

        private void HandleHitmarker()
        {
            //If hitmarker is active
            if (this.m_CurrentHitmarkerTime > 0.0f)
            {
                //Decrease hitmarker times
                this.m_CurrentHitmarkerTime -= Time.deltaTime;

                //Check hitmarker rotation
                if (this.m_CurrentHitmarkerRotation != 0)
                {
                    //Rotate to zero
                    this.m_CurrentHitmarkerRotation = Mathf.Lerp(this.m_CurrentHitmarkerRotation, 0, this.m_UIBase.m_HitmarkerRotationReturnSpeed * Time.deltaTime);

                    //Set hitmarker rotation
                    this.m_HitmarkerImage.transform.rotation = Quaternion.Euler(0, 0, this.m_CurrentHitmarkerRotation);
                }
            }
            this.m_HitmarkerImage.gameObject.SetActive(this.m_CurrentHitmarkerTime > 0);
        }

        private void HandleCrosshairState()
        {
            foreach (r_CrosshairState _State in this.m_UIBase.m_CrosshairStates)
            {
                if (_State.m_MoveState == this.m_PlayerController.m_MoveState)
                {
                    if (this.m_CurrentCrosshairSize != _State.m_CrosshairSize)
                    {
                        //Find crosshair information based on current move state
                        HandleCrosshair(_State.m_CrosshairSize, _State.m_CrosshairAdjustSpeed, _State.m_CrosshairRotation, _State.m_CrosshairRotationAdjustSpeed);
                    }
                }
            }

            //Return crosshair on shoot size
            this.m_CurrentCrosshairShootMultiplier = Mathf.Lerp(this.m_CurrentCrosshairShootMultiplier, 0, this.m_UIBase.m_CrosshairShootReturnSpeed);

            //Check crosshair state
            bool _crosshair_State = this.m_PlayerController.m_WeaponManager.m_CurrentWeapon == null ? false : (this.m_PlayerController.m_MoveState != r_MoveState.SPRINTING && this.m_PlayerController.m_PlayerCamera.m_CameraState != r_CameraState.AIMING);

            //Check if crosshair is the state it should be
            if (this.m_CrosshairRectTransform.gameObject.activeSelf != _crosshair_State)
            {
                //Crosshair activity based on movement and aiming
                SetCrosshairState(_crosshair_State);
            }
        }

        private void HandleCrosshair(float _CrosshairSize, float _CrosshairAdjustSpeed, float _CrosshairRotation, float _CrosshairRotationAdjustSpeed)
        {
            //Lerp current crosshair size to desired size
            this.m_CurrentCrosshairSize = Mathf.Lerp(this.m_CurrentCrosshairSize, _CrosshairSize, Time.deltaTime * _CrosshairAdjustSpeed);

            this.m_CurrentCrosshairSize += this.m_CurrentCrosshairShootMultiplier;

            //Set crsshair size
            this.m_CrosshairRectTransform.sizeDelta = new Vector2(this.m_CurrentCrosshairSize, this.m_CurrentCrosshairSize);

            if (this.m_CurrentCrosshairRotation != _CrosshairRotation)
            {
                //Lerp current crosshair rotation to desired rotation
                this.m_CurrentCrosshairRotation = Mathf.Lerp(this.m_CurrentCrosshairRotation, _CrosshairRotation, Time.deltaTime * _CrosshairRotationAdjustSpeed);

                //Apply crosshair rotation
                this.m_CrosshairRectTransform.localEulerAngles = new Vector3(0, 0, this.m_CurrentCrosshairRotation);
            }
        }

        private void HandleWeaponUI()
        {
            r_WeaponManagerData _current_weapon = this.m_PlayerController.m_WeaponManager.m_CurrentWeapon;

            if (_current_weapon != null && this.m_PlayerController.m_WeaponManager.m_LocalWeapons.Count > 0)
            {
                //Enable local weapon image
                if (!this.m_LocalWeaponPreview.enabled) this.m_LocalWeaponPreview.enabled = true;

                //Find weapon controller
                r_WeaponController _weapon = null;

                if (_current_weapon.m_WeaponObject_FP != null)
                {
                    //Find first person weapon controller
                    _weapon = _current_weapon.m_WeaponObject_FP;
                }

                if (_current_weapon.m_WeaponData != null)
                {
                    //Set current weapon name
                    this.m_LocalWeaponName.text = _current_weapon.m_WeaponData.m_WeaponName;

                    //Set current Weapon Image
                    this.m_LocalWeaponPreview.texture = _current_weapon.m_WeaponData.m_WeaponTexture;
                }

                if (_weapon != null)
                {
                    //Disable the center crosshair point
                    if (this.m_CrosshairCenterPointImage != null)
                        this.m_CrosshairCenterPointImage.gameObject.SetActive(_weapon.m_WeaponAimState != r_WeaponAimState.AIMING);

                    //Check text color because of low ammunation in mag
                    Color _mag_ammunation_color = _weapon.m_Ammunation.m_Ammo == 0 ? this.m_UIBase.m_EmptyAmmunationColor : (_weapon.m_Ammunation.m_Ammo <= _weapon.m_WeaponConfig.m_ReloadSettings.m_AmmoWarningOnBulletsLeft ? this.m_UIBase.m_WarningAmmunationColor : this.m_UIBase.m_DefaultAmmunationColor);
                    Color _total_ammunation_color = this.m_PlayerController.m_WeaponManager.m_TotalAmmunation == 0 ? this.m_UIBase.m_EmptyAmmunationColor : (this.m_PlayerController.m_WeaponManager.m_TotalAmmunation <= _weapon.m_WeaponConfig.m_ReloadSettings.m_AmmoWarningOnBulletsLeft ? this.m_UIBase.m_WarningAmmunationColor : this.m_UIBase.m_DefaultAmmunationColor);

                    //Color to hex
                    string _mag_hexadecimal_color = ColorUtility.ToHtmlStringRGBA(_mag_ammunation_color);
                    string _total_hexadecimal_color = ColorUtility.ToHtmlStringRGBA(_total_ammunation_color);

                    //Update ammunation text
                    this.m_LocalWeaponAmmo.text = $"<color={"#" + _mag_hexadecimal_color}>{_weapon.m_Ammunation.m_Ammo.ToString("000")}</color>" + " / " + $"<color={"#" + _total_hexadecimal_color}>{this.m_PlayerController.m_WeaponManager.m_TotalAmmunation.ToString("000")}</color>";
                }
            }
            else
            {
                //Clear weapon UI data
                this.m_LocalWeaponName.text = "-";
                this.m_LocalWeaponAmmo.text = "- / -";
                this.m_LocalWeaponPreview.enabled = false;
            }
        }

        private void HandleStaminaUI() => this.m_StaminaText.text = Mathf.RoundToInt(this.m_PlayerController.m_stamina).ToString();
        #endregion

        #region Actions
        public void OnCrosshairShoot(float _crosshair_increase_size) => this.m_CurrentCrosshairShootMultiplier += _crosshair_increase_size;
        #endregion

        #region Set
        private void SetDefaults()
        {
            //Reset hitmarker
            this.m_HitmarkerImage.gameObject.SetActive(false);

            //Set bloody screen alpha on zero
            SetBloodyScreenAlpha(0);

            //Set damage indicator alpha on zero
            SetDamageIndicatorAlpha(0);

            //Set health text
            SetHealthText(this.m_PlayerController.m_PlayerHealth.m_Health);
        }

        public void SetBloodyScreen()
        {
            if (!this.m_UIBase.m_BloodyScreenFeature) return;

            //Set bloody screen alpha
            this.m_CurrentBloodyAlpha = this.m_UIBase.m_BloodyScreenTime;
        }

        public void SetDamageIndicator(string _senderName, Vector3 _enemyPosition)
        {
            if (!this.m_UIBase.m_DamageIndicatorFeature) return;
            if (_senderName == PhotonNetwork.LocalPlayer.NickName) return;

            //Set enemy position
            this.m_LastEnemyPosition = _enemyPosition;

            //Set damage indicator alpha color
            this.m_CurrentIndicatorAlpha = this.m_UIBase.m_DamageIndicatorTime;
        }

        public void SetHitmarker(bool _enemyKilled)
        {
            //Disable hitmarker
            this.m_HitmarkerImage.gameObject.SetActive(false);

            //Reset hitmarker time
            this.m_CurrentHitmarkerTime = this.m_UIBase.m_HitmarkerTime;

            //Set hitmarker color
            this.m_HitmarkerImage.color = _enemyKilled ? this.m_UIBase.m_HitmarkerKilledEnemyColor : this.m_UIBase.m_HitmarkerDefaultColor;

            //Random hitmarker rotation
            this.m_CurrentHitmarkerRotation = Random.Range(-this.m_UIBase.m_HitmarkerRandomRotation, this.m_UIBase.m_HitmarkerRandomRotation);
        }

        public void SetKillmessage(string _eliminatedName)
        {
            //Instantiate kill message on UI
            GameObject _killmessage = Instantiate(this.m_KillMessagePrefab, this.m_KillMessageContent);

            //Highlighting the elimated player name with a color
            string _string = ($"ELIMINATED <color=#ff0000ff>{ _eliminatedName } </color>").ToUpper();

            //Find the text and set the string
            _killmessage.GetComponent<Text>().text = _string;

            //Destroy after 2 seconds
            Destroy(_killmessage, 2f);
        }

        public void SetHealthText(float _health)
        {
            this.m_HealthImage.color = _health <= this.m_PlayerController.m_PlayerHealth.m_HealthBase.m_HealthWarning ? this.m_UIBase.m_WarningHealthColor : this.m_UIBase.m_DefaultHealthColor;
            this.m_HealthText.color = _health <= this.m_PlayerController.m_PlayerHealth.m_HealthBase.m_HealthWarning ? this.m_UIBase.m_WarningHealthColor : this.m_UIBase.m_DefaultHealthColor;

            this.m_HealthText.text = Mathf.RoundToInt(_health).ToString("000");
        }

        public void SetCrosshairState(bool _state)
        {
            if (this.m_CrosshairRectTransform.gameObject.activeSelf != _state)
                this.m_CrosshairRectTransform.gameObject.SetActive(_state);
        }

        private void SetBloodyScreenAlpha(float _alpha) => this.m_BloodyScreenImage.color = new Color(this.m_BloodyScreenImage.color.r, this.m_BloodyScreenImage.color.g, this.m_BloodyScreenImage.color.b, _alpha);
        private void SetDamageIndicatorAlpha(float _alpha) => this.m_DamageIndicatorImage.color = new Color(this.m_DamageIndicatorImage.color.r, this.m_DamageIndicatorImage.color.g, this.m_DamageIndicatorImage.color.b, _alpha);
        #endregion
    }
}