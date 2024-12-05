using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 2f;
    public float positionOffsetX = 7f;

    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool canRotateRight = true;
    private bool canRotateLeft = false;

    [Header("Audio")]
    public AudioClip rotateSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        targetRotation = transform.rotation;
        targetPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (canRotateLeft) RotateLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (canRotateRight) RotateRight();
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * rotationSpeed);
    }

    void RotateLeft()
    {
        if (rotateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(rotateSound, MusicManager.instance.SFXVolume);
        }
        targetRotation *= Quaternion.Euler(0, -180, 0);
        targetPosition += new Vector3(-positionOffsetX, 0, 0);

        canRotateLeft = false;
        canRotateRight = true;
    }

    void RotateRight()
    {
        if (rotateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(rotateSound, MusicManager.instance.SFXVolume);
        }
        targetRotation *= Quaternion.Euler(0, 180, 0);
        targetPosition += new Vector3(positionOffsetX, 0, 0);

        canRotateRight = false;
        canRotateLeft = true;
    }
}
