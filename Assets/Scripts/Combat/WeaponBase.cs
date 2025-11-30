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
    [SerializeField] private bool showBulletTracer = true;
    [SerializeField] private float tracerWidth = 0.02f;
    [SerializeField] private float tracerDuration = 0.1f;
    [SerializeField] private Color tracerColor = Color.yellow;
    [SerializeField] private Material tracerMaterial;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Visuals")]
    [SerializeField] private Transform weaponModel;
    [SerializeField] private float recoilDistance = 0.1f;
    [SerializeField] private float recoilReturnSpeed = 12f;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float muzzleFlashDuration = 0.05f;
    [SerializeField] private float muzzleFlashScale = 0.1f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    private Camera ownerCamera;
    private float nextFireTime;
    private Vector3 baseLocalPosition;
    private Coroutine recoilRoutine;
    private Coroutine flashRoutine;
    private GameObject muzzleFlashInstance;

    public string WeaponName => weaponName;

    private void Awake()
    {
        CacheBasePose();
        EnsureMuzzleFlashInstance();
    }

    private void OnEnable()
    {
        CacheBasePose();
        EnsureMuzzleFlashInstance();
        if (muzzleFlashInstance != null)
        {
            muzzleFlashInstance.SetActive(false);
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
        Transform firePoint = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        Ray ray = new Ray(ownerCamera.transform.position, ownerCamera.transform.forward);
        Vector3 hitPoint;
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            
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
        else
        {
            hitPoint = ray.origin + ray.direction * maxDistance;
        }
        
        if (showBulletTracer)
        {
            DrawBulletTracer(firePoint.position, hitPoint);
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
        if (muzzleFlashInstance == null)
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
        muzzleFlashInstance.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashDuration);
        muzzleFlashInstance.SetActive(false);
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

    private void EnsureMuzzleFlashInstance()
    {
        if (muzzleFlashPrefab == null)
        {
            muzzleFlashInstance = null;
            return;
        }

        if (muzzleFlashInstance == null)
        {
            Transform anchor = GetMuzzleFlashAnchor();

            if (muzzleFlashPrefab.scene.IsValid())
            {
                muzzleFlashInstance = muzzleFlashPrefab;
            }
            else
            {
                muzzleFlashInstance = Instantiate(muzzleFlashPrefab, anchor);
            }
        }

        Transform currentAnchor = GetMuzzleFlashAnchor();
        muzzleFlashInstance.transform.SetParent(currentAnchor, false);
        muzzleFlashInstance.transform.localPosition = Vector3.zero;
        muzzleFlashInstance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        muzzleFlashInstance.transform.localScale = Vector3.one * muzzleFlashScale;
        muzzleFlashInstance.SetActive(false);
        
        // Adiciona billboard se não tiver
        if (muzzleFlashInstance.GetComponent<BillboardSprite>() == null)
        {
            BillboardSprite billboard = muzzleFlashInstance.AddComponent<BillboardSprite>();
            billboard.SetCamera(ownerCamera);
        }
    }

    private Transform GetMuzzleFlashAnchor()
    {
        if (projectileSpawnPoint != null)
        {
            return projectileSpawnPoint;
        }

        return transform;
    }

    private void DrawBulletTracer(Vector3 start, Vector3 end)
    {
        GameObject tracerObj = new GameObject("BulletTracer");
        LineRenderer line = tracerObj.AddComponent<LineRenderer>();
        
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = tracerWidth;
        line.endWidth = tracerWidth;
        line.startColor = tracerColor;
        line.endColor = tracerColor;
        
        if (tracerMaterial != null)
        {
            line.material = tracerMaterial;
        }
        else
        {
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.material.color = tracerColor;
        }
        
        StartCoroutine(FadeOutTracer(line, tracerObj));
    }

    private IEnumerator FadeOutTracer(LineRenderer line, GameObject tracerObj)
    {
        float elapsed = 0f;
        Color startColor = line.startColor;
        Color endColor = line.endColor;
        
        while (elapsed < tracerDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / tracerDuration);
            
            line.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            line.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);
            
            yield return null;
        }
        
        Destroy(tracerObj);
    }
}
