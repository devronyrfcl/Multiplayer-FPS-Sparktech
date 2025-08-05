using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;

namespace ForceCodeFPS
{
    #region Serializable Enums
    [System.Serializable] public enum r_WeaponItemType { PRIMARY, SECONDARY, LETHAL, TACTICAL }
    #endregion

    #region Serializable Classes
    [System.Serializable]
    public class r_WeaponManagerData
    {
        [Header("Weapon Data")]
        public r_WeaponPickupBase m_WeaponData;

        [Header("Runtime Data")]
        [HideInInspector] public r_WeaponController m_WeaponObject_FP;
        [HideInInspector] public r_ThirdPersonWeapon m_WeaponObject_TP;

        [Header("Unique runtime ID")]
        [HideInInspector] public string m_UniqueID;

        public r_WeaponManagerData() => this.m_UniqueID = System.Guid.NewGuid().ToString();
    }
    #endregion

    public class r_WeaponManager : MonoBehaviourPun
    {
        #region References
        public r_PlayerController m_PlayerController;
        #endregion

        #region Public Variables
        [Header("Weapon Holder")]
        [SerializeField] private Transform m_WeaponParent;

        [Header("Camera Holder")]
        [SerializeField] private Camera m_Camera;

        [Space(10)]
        public List<r_WeaponManagerData> m_AllWeapons = new List<r_WeaponManagerData>();
        public List<r_WeaponManagerData> m_LocalWeapons = new List<r_WeaponManagerData>();

        [Header("Pickup Settings")]
        [SerializeField] private int m_WeaponSlots;
        [SerializeField] private float m_PickupDistance;

        [Header("Drop Settings")]
        [SerializeField] private float m_WeaponDropForce;

        [Header("Ammunation Settings")]
        public int m_TotalAmmunation;
        #endregion

        #region Private Variables
        [HideInInspector] public int m_CurrentWeaponIndex;

        //Current Weapon states
        [HideInInspector] public bool m_ChangingWeapon;
        [HideInInspector] public bool m_ReloadingWeapon;

        //Current weapon
        [HideInInspector] public r_WeaponManagerData m_CurrentWeapon = null;
        #endregion

        #region Functions
        private void Update()
        {
            if (!photonView.IsMine) return;

            HandleInputs();
            HandleStates();
        }
        #endregion

        #region Handling
        private void HandleInputs()
        {
            //Pickup detection
            if (Physics.Raycast(this.m_Camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out RaycastHit _hit, this.m_PickupDistance))
            {
                r_WeaponPickup _weapon_pickup = _hit.transform.GetComponent<r_WeaponPickup>();

                if (_weapon_pickup != null)
                {
                    if (!this.m_PlayerController.m_PlayerUI.m_WeaponPickupImage.gameObject.activeSelf) this.m_PlayerController.m_PlayerUI.m_WeaponPickupImage.gameObject.SetActive(true);

                    this.m_PlayerController.m_PlayerUI.m_WeaponPickupText.text = $"Hold [F] to pickup [{_weapon_pickup.m_WeaponPickupData.m_WeaponName}]";
                    this.m_PlayerController.m_PlayerUI.m_WeaponPickupImage.texture = _weapon_pickup.m_WeaponPickupData.m_WeaponTexture;

                    if (this.m_PlayerController.m_InputManager.WeaponPickKey())
                    {
                        //Validate pickup
                        OnValidatePickup(_weapon_pickup.m_WeaponPickupData.m_WeaponID);

                        //On Pickup
                        _weapon_pickup.OnPickup();
                    }
                }
                else
                {
                    this.m_PlayerController.m_PlayerUI.m_WeaponPickupText.text = string.Empty;
                    this.m_PlayerController.m_PlayerUI.m_WeaponPickupImage.texture = null;

                    if (this.m_PlayerController.m_PlayerUI.m_WeaponPickupImage.gameObject.activeSelf) this.m_PlayerController.m_PlayerUI.m_WeaponPickupImage.gameObject.SetActive(false);
                }
            }

            //Drop detection
            if (this.m_PlayerController.m_InputManager.WeaponDropKey() && this.m_LocalWeapons.Count > 0 && !this.m_ChangingWeapon)
            {
                OnDrop(PhotonNetwork.LocalPlayer.ActorNumber, this.m_CurrentWeaponIndex, false, false);
            }

            //Change Weapon detection
            if (this.m_LocalWeapons.Count > 0 && !this.m_ChangingWeapon)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) && CanChangeWeapon(GetWeaponIndexKeyCode(KeyCode.Alpha1))) OnChange(GetWeaponIndexKeyCode(KeyCode.Alpha1));
                if (Input.GetKeyDown(KeyCode.Alpha2) && CanChangeWeapon(GetWeaponIndexKeyCode(KeyCode.Alpha2))) OnChange(GetWeaponIndexKeyCode(KeyCode.Alpha2));
                if (Input.GetKeyDown(KeyCode.Alpha3) && CanChangeWeapon(GetWeaponIndexKeyCode(KeyCode.Alpha3))) OnChange(GetWeaponIndexKeyCode(KeyCode.Alpha3));
                if (Input.GetKeyDown(KeyCode.Alpha4) && CanChangeWeapon(GetWeaponIndexKeyCode(KeyCode.Alpha4))) OnChange(GetWeaponIndexKeyCode(KeyCode.Alpha4));
                if (Input.GetKeyDown(KeyCode.Alpha5) && CanChangeWeapon(GetWeaponIndexKeyCode(KeyCode.Alpha5))) OnChange(GetWeaponIndexKeyCode(KeyCode.Alpha5));
            }
        }

        private void HandleStates()
        {
            //Check if any weapon in local weapons inventory is reloading
            this.m_ReloadingWeapon = GetWeaponReloadState();
        }
        #endregion

        #region Actions
        public void OnLoadoutSelect(int[] _weapon_indexes)
        {
            if (photonView.IsMine)
            {
                List<r_WeaponManagerData> _weapons = new List<r_WeaponManagerData>();

                //Give weapon
                for (int i = 0; i < _weapon_indexes.Length; i++)
                {
                    r_WeaponManagerData _weapon = FindWeaponByID(_weapon_indexes[i]);

                    if (_weapon != null)
                    {
                        //add weapon
                        _weapons.Add(_weapon);
                    }
                }

                //Sort loadout based on primary, secondary, etc..
                _weapons.OrderBy(x => x.m_WeaponData.m_WeaponType).ToList();

                //Add weapons to inventory
                foreach (r_WeaponManagerData _inventory_weapon in _weapons)
                {
                    //Validate pickup
                    OnPickup(_inventory_weapon.m_WeaponData.m_WeaponID);
                }
            }
        }

        private void OnValidatePickup(int _weapon_id)
        {
            if (this.m_LocalWeapons.Count >= this.m_WeaponSlots)
            {
                //Swap on weapon slot limit
                OnSwap(_weapon_id);
            }
            else
            {
                //Pickup on weapon slot limit free
                OnPickup(_weapon_id);
            }
        }

        private void OnCreateWeapon(int _weapon_index)
        {
            //Create first person weapon instantiate
            this.m_LocalWeapons[_weapon_index].m_WeaponObject_FP = Instantiate(this.m_LocalWeapons[_weapon_index].m_WeaponData.m_Weapon_FP_Prefab, this.m_WeaponParent);

            //Instantiate third person weapon on local player but for everyone
            this.m_LocalWeapons[_weapon_index].m_WeaponObject_TP = Instantiate(this.m_LocalWeapons[_weapon_index].m_WeaponData.m_Weapon_TP_Prefab, this.m_PlayerController.m_ThirdPersonManager.m_WeaponParent);
        }

        private IEnumerator OnChangeCoroutine(int _previous_weapon_index, int _next_weapon_index)
        {
            r_WeaponManagerData _previous_weapon = this.m_LocalWeapons[_previous_weapon_index];
            r_WeaponManagerData _next_weapon = this.m_LocalWeapons[_next_weapon_index];

            this.m_ChangingWeapon = true;

            if (_previous_weapon != null)
            {
                //Unequip first person weapon
                _previous_weapon.m_WeaponObject_FP.UnEquipWeapon();

                //Unequip third person weapon
                this.m_PlayerController.m_ThirdPersonManager.OnUnequipWeapon(_previous_weapon.m_UniqueID, _previous_weapon.m_WeaponData.m_Weapon_TP_Prefab.m_unequipAnimationDuration);

                yield return new WaitForSeconds(_previous_weapon.m_WeaponObject_FP.m_WeaponConfig.m_AnimationSettings.m_UnequipAnimDuration);

                OnChangeWeaponMesh(_previous_weapon_index, false);
            }

            if (_next_weapon != null)
            {
                OnChangeWeaponMesh(_next_weapon_index, true);

                //Equip first person weapon
                _next_weapon.m_WeaponObject_FP.EquipWeapon();

                //Save next weapon index
                OnChangeWeaponIndex(_next_weapon_index);

                //Equip third person weapon
                this.m_PlayerController.m_ThirdPersonManager.OnEquipWeapon(_next_weapon.m_UniqueID, _next_weapon.m_WeaponData.m_Weapon_TP_Prefab.m_equipAnimationDuration);

                yield return new WaitForSeconds(_next_weapon.m_WeaponObject_FP.m_WeaponConfig.m_AnimationSettings.m_EquipAnimDuration);
            }

            this.m_ChangingWeapon = false;
        }

        public void OnDropAllWeapons()
        {
            int _weapon_index = this.m_LocalWeapons.Count - 1;

            if (this.m_LocalWeapons.Count > 0)
            {
                foreach (r_WeaponManagerData _weapon in this.m_LocalWeapons)
                {
                    //Drop weapon
                    OnDrop(PhotonNetwork.LocalPlayer.ActorNumber, _weapon_index, false, true);

                    //Destroy weapon
                    OnDestroyWeapon(_weapon_index);

                    //Count weapon index
                    _weapon_index--;
                }
            }
        }

        private void OnDrop(int _sender_actor_id, int _weapon_index, bool _drop_on_swap, bool _on_die)
        {
            if (photonView == null) return;

            if (photonView.IsMine)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    //Check if correct weapon to drop
                    if (this.m_LocalWeapons[_weapon_index] != null)
                    {
                        //Instantiate droppable weapon
                        GameObject _weapon_drop = PhotonNetwork.InstantiateRoomObject("Weapons/Pickup/" + this.m_LocalWeapons[_weapon_index].m_WeaponData.m_Weapon_Pickup_Prefab.name, this.m_Camera.transform.position, this.m_Camera.transform.rotation);

                        //Find rigidbody
                        Rigidbody _weapon_rigidbody = _weapon_drop.GetComponent<Rigidbody>();

                        //Adding force and torque to make throw effect
                        _weapon_rigidbody.isKinematic = false;

                        _weapon_rigidbody.AddForce(this.m_Camera.transform.forward * this.m_WeaponDropForce, ForceMode.Impulse);
                        _weapon_rigidbody.AddTorque(new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100)));
                    }
                }
                else
                {
                    photonView.RPC(nameof(OnDropEvent_RPC), RpcTarget.MasterClient, _sender_actor_id, _weapon_index, _drop_on_swap, _on_die);
                }

                if (photonView != null && !_on_die) photonView.RPC(nameof(OnAfterDropEvent_RPC), RpcTarget.AllBuffered, _sender_actor_id, _weapon_index, _drop_on_swap, _on_die);
            }
        }

        private void OnPickup(int _weapon_id) => photonView.RPC(nameof(OnPickupEvent_RPC), RpcTarget.AllBuffered, _weapon_id);
        private void OnSwap(int _weapon_id) => photonView.RPC(nameof(OnSwapEvent_RPC), RpcTarget.AllBuffered, _weapon_id);
        private void OnChange(int _next_weapon_index) => photonView.RPC(nameof(OnChangeEvent_RPC), RpcTarget.AllBuffered, _next_weapon_index);
        #endregion

        #region Network Events
        [PunRPC]
        private void OnPickupEvent_RPC(int _weapon_id)
        {
            if (FindWeaponByID(_weapon_id) != null)
            {
                if (this.m_LocalWeapons.Count > 0)
                {
                    //Disable current weapon
                    OnChangeWeaponMesh(this.m_CurrentWeaponIndex, false);

                    //Handle in third person
                    this.m_PlayerController.m_ThirdPersonManager.OnUnequipWeapon(this.m_LocalWeapons[this.m_CurrentWeaponIndex].m_UniqueID, 0);
                }

                int _desired_weapon_index = this.m_CurrentWeaponIndex;

                if (this.m_LocalWeapons.Count == 0) _desired_weapon_index = 0; else _desired_weapon_index++;

                //Create new weapon item
                r_WeaponManagerData _new_weapon = new r_WeaponManagerData { m_WeaponData = FindWeaponByID(_weapon_id).m_WeaponData };

                //Register new weapon
                this.m_LocalWeapons.Add(_new_weapon);

                //Set current weapon Index
                OnChangeWeaponIndex(_desired_weapon_index);

                //Create new weapon object
                OnCreateWeapon(this.m_CurrentWeaponIndex);

                //Handle Mesh
                OnChangeWeaponMesh(this.m_CurrentWeaponIndex, true);

                //Third person
                this.m_PlayerController.m_ThirdPersonManager.OnEquipWeapon(_new_weapon.m_UniqueID, _new_weapon.m_WeaponData.m_Weapon_TP_Prefab.m_equipAnimationDuration);
            }
        }

        [PunRPC]
        private void OnSwapEvent_RPC(int _weapon_id)
        {
            if (FindWeaponByID(_weapon_id) != null)
            {
                //Drop current weapon
                OnDrop(PhotonNetwork.LocalPlayer.ActorNumber, this.m_CurrentWeaponIndex, true, false);

                //Replace current weapon
                this.m_LocalWeapons[this.m_CurrentWeaponIndex] = new r_WeaponManagerData { m_WeaponData = FindWeaponByID(_weapon_id).m_WeaponData };

                //Create new weapon
                OnCreateWeapon(this.m_CurrentWeaponIndex);

                //Set current weapon index
                OnChangeWeaponIndex(this.m_CurrentWeaponIndex);
            }
        }

        [PunRPC]
        private void OnDropEvent_RPC(int _sender_actor_id, int _weapon_index, bool _drop_on_swap, bool _on_die)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //Check if correct weapon to drop
                if (this.m_LocalWeapons[_weapon_index] != null)
                {
                    //Instantiate droppable weapon
                    GameObject _weapon_drop = PhotonNetwork.InstantiateRoomObject("Weapons/Pickup/" + this.m_LocalWeapons[_weapon_index].m_WeaponData.m_Weapon_Pickup_Prefab.name, this.m_Camera.transform.position, this.m_Camera.transform.rotation);

                    //Find rigidbody
                    Rigidbody _weapon_rigidbody = _weapon_drop.GetComponent<Rigidbody>();

                    //Adding force and torque to make throw effect
                    _weapon_rigidbody.isKinematic = false;

                    _weapon_rigidbody.AddForce(this.m_Camera.transform.forward * this.m_WeaponDropForce, ForceMode.Impulse);
                    _weapon_rigidbody.AddTorque(new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100)));

                    Debug.Log("On Masterclient networked drop");
                }
            }
        }

        [PunRPC]
        private void OnAfterDropEvent_RPC(int _sender_actor_id, int _weapon_index, bool _drop_on_swap, bool _on_die)
        {
            //Check if correct weapon to drop
            if (this.m_LocalWeapons[_weapon_index] != null)
            {
                if (!_on_die)
                {
                    //Destroy current weapon
                    OnDestroyWeapon(this.m_CurrentWeaponIndex);

                    if (!_drop_on_swap)
                    {
                        //Remove current weapon if contains
                        if (this.m_LocalWeapons.Contains(this.m_CurrentWeapon)) this.m_LocalWeapons.Remove(m_CurrentWeapon);

                        if (this.m_LocalWeapons.Count > 0)
                        {
                            //Set weapon index
                            int _desired_weapon_index = this.m_CurrentWeaponIndex;

                            //Check previous weapon index of local weapons
                            if (_desired_weapon_index != 0) _desired_weapon_index--;

                            //Set current weapon index
                            OnChangeWeaponIndex(_desired_weapon_index);

                            //Set previous weapon
                            OnChangeWeaponMesh(this.m_CurrentWeaponIndex, true);

                            //Equip weapon
                            this.m_PlayerController.m_ThirdPersonManager.OnEquipWeapon(this.m_LocalWeapons[this.m_CurrentWeaponIndex].m_UniqueID, this.m_LocalWeapons[this.m_CurrentWeaponIndex].m_WeaponData.m_Weapon_TP_Prefab.m_equipAnimationDuration);
                        }

                        //Set current weapon index
                        OnChangeWeaponIndex(this.m_CurrentWeaponIndex);
                    }
                }
            }
        }

        [PunRPC]
        private void OnChangeEvent_RPC(int _next_weapon_index)
        {
            //Save previous weapon index
            int _previous_weapon_index = this.m_CurrentWeaponIndex;

            //Change weapon with coroutine for animations
            StartCoroutine(OnChangeCoroutine(_previous_weapon_index, _next_weapon_index));
        }
        #endregion

        #region Get
        private int GetWeaponIndexKeyCode(KeyCode _input)
        {
            int _weapon_index = 999;

            switch (_input)
            {
                case KeyCode.Alpha1: _weapon_index = 0; break;
                case KeyCode.Alpha2: _weapon_index = 1; break;
                case KeyCode.Alpha3: _weapon_index = 2; break;
                case KeyCode.Alpha4: _weapon_index = 3; break;
                case KeyCode.Alpha5: _weapon_index = 4; break;
            }

            return _weapon_index;
        }

        private bool GetWeaponReloadState()
        {
            bool _reloading = false;

            foreach (r_WeaponManagerData _weapon in this.m_LocalWeapons)
            {
                if (_weapon.m_WeaponObject_FP.m_WeaponState == r_WeaponState.RELOADING)
                    _reloading = true;
            }
            return _reloading;
        }

        private bool CanChangeWeapon(int _next_weapon_index) => this.m_LocalWeapons.Count - 1 >= _next_weapon_index && this.m_LocalWeapons[_next_weapon_index] != null && _next_weapon_index != this.m_CurrentWeaponIndex;

        public r_WeaponManagerData FindWeaponByID(int _weapon_id) => this.m_AllWeapons.Find(x => x.m_WeaponData.m_WeaponID == _weapon_id);
        public r_WeaponManagerData FindWeaponByName(string _weapon_name) => this.m_LocalWeapons.Find(x => x.m_WeaponData.m_WeaponName == _weapon_name);
        #endregion

        #region Set
        private void OnChangeWeaponIndex(int _weapon_index)
        {
            //Set current weapon index
            this.m_CurrentWeaponIndex = _weapon_index;

            //Set current weapon -> weapon manager
            this.m_CurrentWeapon = this.m_LocalWeapons.Count == 0 ? this.m_CurrentWeapon = null : this.m_LocalWeapons[m_CurrentWeaponIndex];

            //Set current weapon -> Third person manager
            this.m_PlayerController.m_ThirdPersonManager.m_CurrentWeapon = this.m_LocalWeapons.Count == 0 ? null : this.m_LocalWeapons[m_CurrentWeaponIndex];

        }

        private void OnChangeWeaponMesh(int _weapon_index, bool _state)
        {
            if (this.m_LocalWeapons[_weapon_index].m_WeaponObject_FP != null)
            {
                //Set first person weapon
                this.m_LocalWeapons[_weapon_index].m_WeaponObject_FP.gameObject.SetActive(_state);
            }
        }

        public void OnDestroyWeapon(int _weapon_index)
        {
            if (this.m_LocalWeapons[_weapon_index].m_WeaponObject_FP != null)
            {
                //Destroy local instantiated weapon object
                Destroy(this.m_LocalWeapons[_weapon_index].m_WeaponObject_FP.gameObject);
            }

            if (this.m_LocalWeapons[_weapon_index].m_WeaponObject_TP != null)
            {
                //Destroy Photon Object
                Destroy(this.m_LocalWeapons[_weapon_index].m_WeaponObject_TP.gameObject);
            }
        }

        public void OnWeaponFallMotion(bool _fall)
        {
            if (this.m_CurrentWeapon != null)
            {
                if (this.m_CurrentWeapon.m_WeaponObject_FP != null)
                {
                    //Apply weapon fall effect on character jump or land
                    this.m_CurrentWeapon.m_WeaponObject_FP.onWeaponFall(_fall);
                }
            }
        }
        #endregion
    }
}