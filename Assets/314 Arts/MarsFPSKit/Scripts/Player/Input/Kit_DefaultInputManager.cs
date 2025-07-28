using UnityEngine;

namespace MarsFPSKit
{
    public class DefaultInputData
    {

    }

    [CreateAssetMenu(menuName = "MarsFPSKit/Input Manager/Default")]
    /// <summary>
    /// This is the kit's default input manager
    /// </summary>
    public class Kit_DefaultInputManager : Kit_InputManagerBase
    {
        public Kit_InputSystem inputSystem
        {
            get
            {
                return Kit_InputSystem.instance;
            }
        }

        public override void InitializeServer(Kit_PlayerBehaviour pb)
        {
            DefaultInputData did = new DefaultInputData();
            pb.inputManagerData = did;
            pb.input.weaponSlotUses = new bool[inputSystem.weaponSelectButtons.Length];
        }

        public override void InitializeClient(Kit_PlayerBehaviour pb)
        {
            DefaultInputData did = new DefaultInputData();
            pb.inputManagerData = did;
            pb.input.weaponSlotUses = new bool[inputSystem.weaponSelectButtons.Length];
        }

        public override void WriteToPlayerInput(Kit_PlayerBehaviour pb)
        {
            if (pb.inputManagerData != null && pb.inputManagerData.GetType() == typeof(DefaultInputData))
            {
                DefaultInputData did = pb.inputManagerData as DefaultInputData;
                if (pb.enableInput)
                {
                    //Get all input
                    pb.input.hor = (float)System.Math.Round(inputSystem.movementHorizontal, 1);
                    pb.input.ver = (float)System.Math.Round(inputSystem.movementVertical, 1);
                    pb.input.crouch = inputSystem.crouchHold;
                    pb.input.sprint = inputSystem.runHold;
                    pb.input.jump = inputSystem.jump;
                    pb.input.interact = inputSystem.useHold;

                    pb.input.rmb = inputSystem.aimHold;
                    pb.input.reload = inputSystem.reloadHold;

                    //Changed  from pun to mirror: Mouse X and Mouse Y is now the desired look rotation
                    pb.input.mouseX += inputSystem.lookX * pb.weaponManager.CurrentSensitivity(pb);
                    pb.input.mouseY += inputSystem.lookY * pb.weaponManager.CurrentSensitivity(pb);
                    pb.input.mouseY = Mathf.Clamp(pb.input.mouseY, -90, 90);

                    pb.input.leanLeft = inputSystem.leanLeft;
                    pb.input.leanRight = inputSystem.leanRight;
                    pb.input.thirdPerson = inputSystem.changePerspective;
                    pb.input.flashlight = inputSystem.flashlight;
                    pb.input.laser = inputSystem.laser;

                    pb.input.nextWeapon = inputSystem.nextWeapon;
                    pb.input.previousWeapon = inputSystem.previousWeapon;

                    if (MarsScreen.lockCursor)
                    {
                        pb.input.lmb = inputSystem.fireHold;
                    }
                    else
                    {
                        pb.input.lmb = false;
                    }

                    if (pb.input.weaponSlotUses == null || pb.input.weaponSlotUses.Length != inputSystem.weaponSelectButtons.Length) pb.input.weaponSlotUses = new bool[inputSystem.weaponSelectButtons.Length];

                    for (int i = 0; i < inputSystem.weaponSelectButtons.Length; i++)
                    {
                        int id = i;
                        pb.input.weaponSlotUses[id] = inputSystem.weaponSelectButtons[id];
                    }
                }
                else
                {
                    //Get all input
                    pb.input.hor = 0f;
                    pb.input.ver = 0f;

                    pb.input.crouch = false;
                    pb.input.sprint = false;
                    pb.input.jump = false;
                    pb.input.interact = false;

                    pb.input.rmb = false;
                    pb.input.reload = false;

                    pb.input.leanLeft = false;
                    pb.input.leanRight = false;
                    pb.input.thirdPerson = false;
                    pb.input.flashlight = false;
                    pb.input.laser = false;

                    pb.input.lmb = false;


                    if (pb.input.weaponSlotUses == null || pb.input.weaponSlotUses.Length != inputSystem.weaponSelectButtons.Length) pb.input.weaponSlotUses = new bool[inputSystem.weaponSelectButtons.Length];

                    for (int i = 0; i < inputSystem.weaponSelectButtons.Length; i++)
                    {
                        int id = i;
                        pb.input.weaponSlotUses[id] = false;
                    }
                }

                //Set Camera (this is validated on the server)
                pb.input.clientCamPos = pb.playerCameraTransform.position;
                pb.input.clientCamForward = pb.playerCameraTransform.forward;
            }
        }
    }
}