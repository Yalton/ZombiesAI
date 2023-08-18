using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float fireRate = 1f;
    public float damage = 10f;
    public float range = 100f;
    public float recoilAmount = 5f; // Recoil angle
    public Transform shootPoint; // Where bullets originate from
    public LayerMask shootableLayer; // Layer of objects that can be shot
    public ParticleSystem muzzleFlash; // Reference to the muzzle flash effect

    private float nextFireTime = 0f;

    public void Shoot()
    {
        Debug.Log("Shooting Gun");
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;

            // Recoil
            transform.Rotate(-recoilAmount, 0f, 0f); // Rotates the gun up for recoil
            StartCoroutine(RecoilReset()); // Resets the recoil after a delay
            
            // Muzzle Flash
            muzzleFlash.Play();

            RaycastHit hit;
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, range, shootableLayer))
            {
                // You hit something on the "shootableLayer"

                // You can then apply damage or any other effects here.
                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }

    IEnumerator RecoilReset()
    {
        yield return new WaitForSeconds(0.1f); // Recoil reset delay
        transform.Rotate(recoilAmount, 0f, 0f); // Resets the recoil
    }
}
