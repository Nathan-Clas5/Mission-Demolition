using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;
    public AudioClip launchSound;
    public float launchSoundVolume = 1f;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;


    public AudioClip launchSFX;
    private LineRenderer bandLine;
    private AudioSource audioSource;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        audioSource = GetComponent<AudioSource>();
        if (launchPointTrans == null)
        {
            Debug.LogError("Slingshot needs a child named LaunchPoint.");
            enabled = false;
            return;
        }

        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        bandLine = GetComponent<LineRenderer>();
        if (bandLine == null)
        {
            Debug.LogError("Add a LineRenderer component to the Slingshot.");
        }
        else
        {
            bandLine.positionCount = 2;
            bandLine.enabled = false;
            bandLine.useWorldSpace = true;
        }

        audioSource = GetComponent<AudioSource>();
    }

    void OnMouseEnter()
    {
        if (launchPoint != null) launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        if (!aimingMode && launchPoint != null) launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        if (MissionDemolition.IsGameOver()) return;

        aimingMode = true;
        projectile = Instantiate(projectilePrefab);
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        if (bandLine != null) bandLine.enabled = true;
    }

    void Update()
    {
        if (!aimingMode || projectile == null) return;

        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        Vector3 mouseDelta = mousePos3D - launchPos;
        float maxMagnitude = GetComponent<SphereCollider>().radius;

        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta = mouseDelta.normalized * maxMagnitude;
        }

        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        if (bandLine != null)
        {
            bandLine.SetPosition(0, launchPos);
            bandLine.SetPosition(1, projPos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;

            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            if (launchSound != null)
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(launchSound, launchSoundVolume);
                else
                    AudioSource.PlayClipAtPoint(
                        launchSound, transform.position, launchSoundVolume);
            }

            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;

            if (projLinePrefab != null)
                Instantiate(projLinePrefab, projectile.transform);

            projectile = null;

            if (bandLine != null) bandLine.enabled = false;
            if (launchPoint != null) launchPoint.SetActive(false);

            MissionDemolition.SHOT_FIRED();
        }
    }
}