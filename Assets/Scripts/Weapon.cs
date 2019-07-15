using UnityEngine.UI;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    // Weapon Specification
    public string weaponName; // AKM
    public int bulletsPerMag; // 30
    public int bulletsTotal; // 1500
    public int currentBullets; // 0
    public float range; // 100
    public float fireRate; // 0.1

    // Parameters
    private float fireTimer;
    private bool isReloading = false;

    // References
    public Transform shootPoint;
    private Animator anim;
    public ParticleSystem muzzleFlash;
    public Text bulletsText;
    public Transform bulletCasingPoint;

    //Sound
    public AudioClip shootSound;
    public AudioSource audioSource;
    public AudioClip reloadSound;

    // Prefabs
    public GameObject hitSparkPrefab;
    public GameObject hitHolePrefab;
    public GameObject bulletCasing;

    // Weapon Specification
    public Vector3 aimPosition;
    private Vector3 originalPosition;

    // Recoil
    public Transform camRecoil;
    public Vector3 recoilKickback;
    public float recoilAmount;
    private float originalRecoil;

    // private bool isReloading
    private bool isAiming;

    // Weapon Specification
    public float accuracy;
    private float originalAccuracy;


    // Use this for initialization
    private void Start()
    {
        currentBullets = bulletsPerMag;
        originalPosition = transform.localPosition;
        anim = GetComponent<Animator>();

        bulletsText.text = currentBullets + " / " + bulletsTotal;

        originalAccuracy = accuracy;
        originalRecoil = recoilAmount;

    }

    // Update is called once per frame
    private void Update()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        isReloading = info.IsName("Reload");

        RecoilBack();
        AimDownSights();

        if (Input.GetButton("Fire1"))
        {
            if (currentBullets > 0)
            {
                Fire();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DoReload();
        }


        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }

    private void Fire()
    {
        if (fireTimer < fireRate)
        {
            return;
        }
        Debug.Log("Shot Fired!");
        RaycastHit hit;

        if(Physics.Raycast(shootPoint.position, shootPoint.transform.forward + Random.onUnitSphere * accuracy, out hit, range))
        {
            GameObject hitSpark = Instantiate(hitSparkPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            Destroy(hitSpark, 0.5f); // Destroying automatically
            GameObject hitHole = Instantiate(hitHolePrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            Destroy(hitHole, 5f); // Destroying automatically

        }

        currentBullets--;
        bulletsText.text = currentBullets + " / " + bulletsTotal;

        fireTimer = 0.0f;
        audioSource.PlayOneShot(shootSound);
        anim.CrossFadeInFixedTime("Fire", 0.01f); // fire animation

        muzzleFlash.Play();
        Recoil();
        BulletEffect();

    }

    private void DoReload()
    {
        if (!isReloading && currentBullets < bulletsPerMag && bulletsTotal > 0)
        {
            anim.CrossFadeInFixedTime("Reload", 0.01f); // Reloading
            audioSource.PlayOneShot(reloadSound);
        }

    }

    public void Reload()
    {
        int bulletsToReload = bulletsPerMag - currentBullets;
        if (bulletsToReload > bulletsTotal)
        {
            bulletsToReload = bulletsTotal;
        }
        currentBullets += bulletsToReload;
        bulletsTotal -= bulletsToReload;
        bulletsText.text = currentBullets + " / " + bulletsTotal;
    }

    private void AimDownSights()
    {
        if (Input.GetButton("Fire2") && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * 8f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 40f, Time.deltaTime * 8f);
            isAiming = true;
            accuracy = originalAccuracy / 2f;
            recoilAmount = originalRecoil / 2f;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60f, Time.deltaTime * 8f);
            isAiming = false;
            accuracy = originalAccuracy;
            recoilAmount = originalRecoil;
        }
    }

    private void Recoil()
    {
        Vector3 recoilVector = new Vector3(Random.Range(-recoilKickback.x, recoilKickback.x), recoilKickback.y, recoilKickback.z);
        Vector3 recoilCamVector = new Vector3(-recoilVector.y * 400f, recoilVector.x * 200f, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + recoilVector, recoilAmount / 2f); // position recoil
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.Euler(camRecoil.localEulerAngles + recoilCamVector), recoilAmount); // cam recoil
    }

    // back to original position
    private void RecoilBack()
    {
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.identity, Time.deltaTime * 2f);
    }

    private void BulletEffect()
    {
        Quaternion randomQuaternion = new Quaternion(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f), 1);
        GameObject casing = Instantiate(bulletCasing, bulletCasingPoint);
        casing.transform.localRotation = randomQuaternion;
        casing.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(Random.Range(50f, 100f), Random.Range(50f, 100f), Random.Range(-30f, 30f)));
        Destroy(casing, 1f);
    }

}