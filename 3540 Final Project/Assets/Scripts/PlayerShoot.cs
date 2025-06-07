using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerShooting : MonoBehaviour {

    [Header("Shot Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private int ammoSize = 8;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitLayers;

    [Header("References")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private TMP_Text ammoCountText;
    [SerializeField] private AudioClip gunShotSFX;
    [SerializeField] private AudioClip noBulletSFX;
    [SerializeField] private AudioClip reloadSFX;

    // Component references
    private AudioSource audioSource;
    private Transform firePoint;

    // Private variables
    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    private void Awake() {
        firePoint = Camera.main.transform;
    }

    private void Start() {
        currentAmmo = ammoSize;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R)) {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire) {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    private void Shoot() {
        if (currentAmmo <= 0) {
            PlayAudioClip(noBulletSFX);
            return;
        }

        currentAmmo--;

        UpdateAmmoCount();                                  // Update ammo text
        if (gunAnimator) gunAnimator.Play("Shoot", 0, 0f); // Start shooting animation
        PlayAudioClip(gunShotSFX);                          // Play shot SFX

        // Create and cast the ray
        Ray ray = new Ray(firePoint.position + firePoint.forward / 2, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, range, hitLayers)) {

            // Apply damage if the hit object has is an enemy
            if (hitInfo.collider.TryGetComponent<EnemyBehavior>(out var enemyBehavior)) {
                enemyBehavior.TakeDamage(damage);
            }

            // Spawn hit effect at the impact point
            if (hitEffectPrefab != null) {
                GameObject hitEffect = Instantiate(
                    hitEffectPrefab,
                    hitInfo.point,
                    Quaternion.LookRotation(hitInfo.normal)
                );
                Destroy(hitEffect, 2f); // Clean up effect after 2 seconds
            }
        }
    }

    private IEnumerator Reload() {
        isReloading = true;
        PlayAudioClip(reloadSFX);
        if (gunAnimator) gunAnimator.Play("Reload", 0, 0f);

        yield return new WaitForSeconds(reloadTime);

        isReloading = false;
        currentAmmo = ammoSize;
        UpdateAmmoCount();
    }

    private void PlayAudioClip(AudioClip audioClip) {
        if (!audioClip) return;

        audioSource.clip = audioClip;
        audioSource.Play();
    }

    private void UpdateAmmoCount() => ammoCountText.text = $"{currentAmmo}/{ammoSize}";
}
