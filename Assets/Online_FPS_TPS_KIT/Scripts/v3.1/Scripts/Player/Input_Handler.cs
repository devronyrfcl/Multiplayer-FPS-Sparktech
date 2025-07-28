using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Input_Handler : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private WeaponPickup weaponPickUp;
    [SerializeField] private BodySlope_Handler bodySlope_Handler;
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [SerializeField] private BodyTiltInSprint bodyTiltInSprint;

        private void Start()
    {
        weaponController.activeID = 1;
        weaponController.animator.Play("GunPickUp", 1);
    }

    void Update()
    {
        TryShoot();

        bodySlope_Handler.setInput(-Input.GetAxisRaw("Slope"));

        bodyTiltInSprint.SetMouseXMove(Input.GetAxis("Mouse X"));

        if (Input.GetKeyDown(KeyCode.Alpha1))
            weaponController.ToChange(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            weaponController.ToChange(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            weaponController.ToChange(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            weaponController.ToChange(4);


        if (Input.GetKeyDown(KeyCode.F) && weaponPickUp != null)
        {
            weaponPickUp.PickupCheck();
        }

        if (Input.GetMouseButtonDown(1))
        {
            cameraSwitcher.AimViewChange();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            cameraSwitcher.ViewChange();
        }
    }


    void TryShoot()
    {
        bool singleshoot = weaponController.GETCurrentWeapon.singleShoot;
        if (singleshoot && Input.GetMouseButtonDown(0))
        {
            weaponController.StartShoot();
        }
        else if (!singleshoot && Input.GetMouseButton(0))
        {
            weaponController.StartShoot();
        }
    }
}
