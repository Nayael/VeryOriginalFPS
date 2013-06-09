using UnityEngine;

public interface IWeapon
{
    void Shoot(Vector3 direction);
    void Shoot(Vector3 position, Vector3 direction);
}