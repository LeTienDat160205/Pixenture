using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 50f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //Left click
        {
            Shoot();
        }
    }

    void Shoot()
    {
        //Get mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Direction from us to mouse
        Vector3 shootDirection = (mousePosition - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(shootDirection.x, shootDirection.y) * bulletSpeed;
        SoundEffectManager.Play("Shoot");
        Destroy(bullet, 2f); // Destroy bullet after 2 seconds
    }
}
