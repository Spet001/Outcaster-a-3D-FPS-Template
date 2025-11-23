using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float radius = 0.01f;

    private float speed;
    private float damage;
    private LayerMask hitMask;
    private Vector3 direction;
    private float lifeTimer;

    public void Fire(Vector3 shootDirection, float damageAmount, float projectileSpeed, LayerMask mask)
    {
        direction = shootDirection.normalized;
        damage = damageAmount;
        speed = projectileSpeed;
        hitMask = mask;
        lifeTimer = lifetime;
        transform.forward = direction;
    }

    private void Update()
    {
        float distance = speed * Time.deltaTime;
        if (CheckCollision(distance))
        {
            return;
        }

        transform.position += direction * distance;
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private bool CheckCollision(float travelDistance)
    {
        if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, travelDistance, hitMask, QueryTriggerInteraction.Ignore))
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

            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
