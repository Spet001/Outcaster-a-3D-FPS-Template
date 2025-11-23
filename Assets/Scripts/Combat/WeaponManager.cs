using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponAnchor;
    [SerializeField] private List<WeaponBase> weapons = new List<WeaponBase>();
    [SerializeField] private int startingWeaponIndex;

    [Header("Input")]
    [SerializeField] private string fireButton = "Fire1";

    private int currentWeaponIndex = -1;

    private WeaponBase CurrentWeapon => currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count
        ? weapons[currentWeaponIndex]
        : null;

    private void Awake()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] == null)
            {
                continue;
            }

            weapons[i].gameObject.SetActive(false);
            weapons[i].BindCamera(playerCamera);
            SnapToAnchor(weapons[i].transform);
        }
    }

    private void Start()
    {
        EquipWeapon(startingWeaponIndex);
    }

    private void Update()
    {
        HandleFireInput();
        HandleSwitchInput();
    }

    private void HandleFireInput()
    {
        if (CurrentWeapon == null)
        {
            return;
        }

        if (Input.GetButton(fireButton))
        {
            CurrentWeapon.TryFire();
        }
    }

    private void HandleSwitchInput()
    {
        if (weapons.Count == 0)
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            CycleWeapon(1);
        }
        else if (scroll < 0f)
        {
            CycleWeapon(-1);
        }

        int maxSlots = Mathf.Min(weapons.Count, 9);
        for (int i = 0; i < maxSlots; i++)
        {
            KeyCode key = (KeyCode)((int)KeyCode.Alpha1 + i);
            if (Input.GetKeyDown(key))
            {
                EquipWeapon(i);
            }
        }
    }

    private void CycleWeapon(int step)
    {
        if (weapons.Count == 0)
        {
            return;
        }

        int nextIndex = (currentWeaponIndex + step) % weapons.Count;
        if (nextIndex < 0)
        {
            nextIndex += weapons.Count;
        }

        EquipWeapon(nextIndex);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
        {
            return;
        }

        if (currentWeaponIndex == index)
        {
            return;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] == null)
            {
                continue;
            }

            bool shouldEnable = i == index;
            weapons[i].gameObject.SetActive(shouldEnable);
            if (shouldEnable)
            {
                weapons[i].BindCamera(playerCamera);
            }
        }

        currentWeaponIndex = index;
    }

    private void SnapToAnchor(Transform weaponTransform)
    {
        if (weaponAnchor == null || weaponTransform == null)
        {
            return;
        }

        weaponTransform.SetParent(weaponAnchor, false);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;
    }
}
