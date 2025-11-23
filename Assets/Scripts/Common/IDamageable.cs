using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}
