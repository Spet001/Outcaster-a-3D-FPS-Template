using System.Collections;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public enum FireMode
    {
        Hitscan,
        Projectile
    }

    [Header("General")]
    [SerializeField] private string weaponName = "Rifle";
    [SerializeField] private FireMode fireMode = FireMode.Hitscan;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;

    [Header("Hitscan")]
    [SerializeField] private float maxDistance = 100f;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Visuals")]
    [SerializeField] private Transform weaponModel;
    [SerializeField] private float recoilDistance = 0.1f;
    [SerializeField] private float recoilReturnSpeed = 12f;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private float muzzleFlashDuration = 0.05f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    private Camera ownerCamera;
    private float nextFireTime;
    private Vector3 baseLocalPosition;
    private Coroutine recoilRoutine;
    private Coroutine flashRoutine;

    public string WeaponName => weaponName;

    private void OnEnable()
    {
        CacheBasePose();
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
        }
    }

    public void BindCamera(Camera camera)
    {
        ownerCamera = camera;
    }

    public bool TryFire()
    {
        if (ownerCamera == null || Time.time < nextFireTime)
        {
            return false;
        }

        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);

        switch (fireMode)
        {
            case FireMode.Hitscan:
                FireHitscan();
                break;
            case FireMode.Projectile:
                FireProjectile();
                break;
        }

        PlayRecoil();
        TriggerMuzzleFlash();
        PlayShotAudio();
        return true;
    }

    private void FireHitscan()
    {
        Ray ray = new Ray(ownerCamera.transform.position, ownerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.ApplyDamage(damage, hit.point, hit.normal);
            }
            else
            {
                IDamageable parentDamageable = hit.collider.GetComponentInParent<IDamageable>();
                parentDamageable?.ApplyDamage(damage, hit.point, hit.normal);
            }
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"Weapon '{weaponName}' has no projectile prefab assigned.");
            return;
        }

        Transform spawn = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        Projectile projectileInstance = Instantiate(projectilePrefab, spawn.position, spawn.rotation);
        Vector3 direction = ownerCamera.transform.forward;
        projectileInstance.Fire(direction, damage, projectileSpeed, hitMask);
    }

    private void PlayRecoil()
    {
        Transform target = weaponModel != null ? weaponModel : transform;

        if (recoilRoutine != null)
        {
            StopCoroutine(recoilRoutine);
        }
        recoilRoutine = StartCoroutine(RecoilRoutine(target));
    }

    private IEnumerator RecoilRoutine(Transform target)
    {
        Vector3 startPos = baseLocalPosition;
        Vector3 recoiledPos = startPos + Vector3.back * recoilDistance;

        target.localPosition = recoiledPos;

        float t = 0f;
        while (t < 1f)
        {
            t += recoilReturnSpeed * Time.deltaTime;
            target.localPosition = Vector3.Lerp(recoiledPos, startPos, t);
            yield return null;
        }

        target.localPosition = startPos;
        recoilRoutine = null;
    }

    private void TriggerMuzzleFlash()
    {
        if (muzzleFlash == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(MuzzleFlashRoutine());
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashDuration);
        muzzleFlash.SetActive(false);
        flashRoutine = null;
    }

    private void PlayShotAudio()
    {
        if (audioSource == null || fireClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(fireClip);
    }

    private void CacheBasePose()
    {
        Transform target = weaponModel != null ? weaponModel : transform;
        baseLocalPosition = target.localPosition;
    }
}
