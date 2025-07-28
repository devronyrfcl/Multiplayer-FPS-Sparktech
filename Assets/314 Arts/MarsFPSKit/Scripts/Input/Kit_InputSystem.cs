using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsFPSKit
{
    public class Kit_InputSystem : MonoBehaviour
    {
        public static Kit_InputSystem instance;

        public PlayerInput input;

        public string GetDisplayString(string actionName)
        {
            var action = input.actions.FindAction(actionName);

            if (action == null)
            {
                return actionName;
            }
            else
            {
                return GetDisplayString(action);
            }
        }

        public string GetDisplayString(InputAction action)
        {
            if (action == null) return "Invalid Action";
            return action.GetBindingDisplayString();
        }

        public bool pause;

        public void Pause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                pause = true;
            }
        }

        public bool chat;

        public void Chat(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                chat = true;
            }
        }

        public bool chatTeam;

        public void ChatTeam(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                chatTeam = true;
            }
        }

        public bool voiceChatPtt;

        public void VoiceChatPtt(InputAction.CallbackContext context)
        {
            voiceChatPtt = context.phase != InputActionPhase.Canceled;
        }

        public bool useHold;

        public void Use(InputAction.CallbackContext context)
        {
            useHold = context.phase != InputActionPhase.Canceled;
        }

        public bool[] weaponSelectButtons;

        public void WeaponSelectPrimary(InputAction.CallbackContext context)
        {
            weaponSelectButtons[0] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectSecondary(InputAction.CallbackContext context)
        {
            weaponSelectButtons[1] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectTertiary(InputAction.CallbackContext context)
        {
            weaponSelectButtons[2] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectFour(InputAction.CallbackContext context)
        {
            weaponSelectButtons[3] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectFive(InputAction.CallbackContext context)
        {
            weaponSelectButtons[4] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectSix(InputAction.CallbackContext context)
        {
            weaponSelectButtons[5] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectSeven(InputAction.CallbackContext context)
        {
            weaponSelectButtons[6] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectEight(InputAction.CallbackContext context)
        {
            weaponSelectButtons[7] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectNine(InputAction.CallbackContext context)
        {
            weaponSelectButtons[8] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectTen(InputAction.CallbackContext context)
        {
            weaponSelectButtons[9] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectEleven(InputAction.CallbackContext context)
        {
            weaponSelectButtons[10] = context.phase != InputActionPhase.Canceled;
        }

        public void WeaponSelectTwelve(InputAction.CallbackContext context)
        {
            weaponSelectButtons[11] = context.phase != InputActionPhase.Canceled;
        }

        public bool nextWeapon;

        public void NextWeapon(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                nextWeapon = true;
            }
        }

        public bool previousWeapon;

        public void PreviousWeapon(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                previousWeapon = true;
            }
        }

        public void ScrollWheel(InputAction.CallbackContext context)
        {
            var look = context.ReadValue<Vector2>();

            if (look.y < -0.2)
            {
                nextWeapon = true;
            }
            else if (look.y > 0.2f)
            {
                previousWeapon = true;
            }
        }

        public float lookX;
        public float lookY;

        public void Look(InputAction.CallbackContext context)
        {
            var look = context.ReadValue<Vector2>() / 10;
            lookX = look.x;
            lookY = look.y;
        }

        public float movementVertical;
        public float movementHorizontal;

        public void Move(InputAction.CallbackContext context)
        {
            var move = context.ReadValue<Vector2>();
            movementHorizontal = move.x;
            movementVertical = move.y;
        }

        public bool crouchHold;

        public void Crouch(InputAction.CallbackContext context)
        {
            crouchHold = context.phase != InputActionPhase.Canceled;
        }

        public bool runHold;

        public void Run(InputAction.CallbackContext context)
        {
            runHold = context.phase != InputActionPhase.Canceled;
        }

        public bool leanLeft;

        public void LeanLeft(InputAction.CallbackContext context)
        {
            leanLeft = context.phase != InputActionPhase.Canceled;
        }

        public bool leanRight;

        public void LeanRight(InputAction.CallbackContext context)
        {
            leanRight = context.phase != InputActionPhase.Canceled;
        }

        public bool jump;

        public void Jump(InputAction.CallbackContext context)
        {
            jump = context.phase != InputActionPhase.Canceled;
        }
        public bool fireHold;

        public void Fire(InputAction.CallbackContext context)
        {
            fireHold = context.phase != InputActionPhase.Canceled;
        }

        public bool reloadHold;

        public void Reload(InputAction.CallbackContext context)
        {
            reloadHold = context.phase != InputActionPhase.Canceled;
        }

        public bool flashlight;

        public void Flashlight(InputAction.CallbackContext context)
        {
            flashlight = context.phase != InputActionPhase.Canceled;
        }

        public bool laser;

        public void Laser(InputAction.CallbackContext context)
        {
            laser = context.phase != InputActionPhase.Canceled;
        }

        public bool changePerspective;

        public void ChangePerspective(InputAction.CallbackContext context)
        {
            changePerspective = context.phase != InputActionPhase.Canceled;
        }

        public bool melee;

        public void Melee(InputAction.CallbackContext context)
        {
            melee = context.phase != InputActionPhase.Canceled;
        }

        public bool aimHold;

        public void Aim(InputAction.CallbackContext context)
        {
            aimHold = context.phase != InputActionPhase.Canceled;
        }

        public bool scoreboard;

        public void Scoreboard(InputAction.CallbackContext context)
        {
            scoreboard = context.phase != InputActionPhase.Canceled;
        }

        public bool voteYes;

        public void VoteYes(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                voteYes = true;
            }
        }

        public bool voteNo;

        public void VoteNo(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                voteNo = true;
            }
        }

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                input.actions.FindActionMap("Player").Enable();
                input.actions.FindActionMap("UI").Enable();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            chat = false;
            chatTeam = false;
            pause = false;
            voteYes = false;
            voteNo = false;
            nextWeapon = false;
            previousWeapon = false;
        }
    }
}