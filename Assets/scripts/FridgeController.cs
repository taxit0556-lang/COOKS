using System.Collections;
using UnityEngine;

public class FridgeController : MonoBehaviour
{
    public static FridgeController Instance;

    public Transform fridgeViewPoint;
    public Camera playerCamera;
    public Player_Movement playerMovement;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private bool fridgeOpen;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenFridge()
    {
        if (fridgeOpen) return;

        playerMovement.canMove = false;
        fridgeOpen = true;

        originalPos = playerCamera.transform.position;
        originalRot = playerCamera.transform.rotation;

        StartCoroutine(MoveCamera(fridgeViewPoint.position, fridgeViewPoint.rotation));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseFridge()
    {
        if (!fridgeOpen) return;

        playerMovement.canMove = true;
        fridgeOpen = false;

        StartCoroutine(MoveCamera(originalPos, originalRot));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startPos = playerCamera.transform.position;
        Quaternion startRot = playerCamera.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            playerCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            playerCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        playerCamera.transform.position = targetPos;
        playerCamera.transform.rotation = targetRot;
    }

    public bool IsOpen()
    {
        return fridgeOpen;
    }

    private void Update()
    {
        if (fridgeOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseFridge();
        }
    }
}