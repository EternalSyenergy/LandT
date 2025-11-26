using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiLocator : MonoBehaviour
{

    public bool ishead;
    public float speed = 5f;
    public float rotationSpeed = 5f;
    public Transform PlayerclickPos;
  public  float scaleSpeed = .2f;

    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }
    private void LateUpdate()
    {

        PositionHandler();
    }
    void PositionHandler()
    {
        if (ishead)
        {
            // Move toward the target position
            transform.position = Vector3.MoveTowards(
                transform.position,
                GameManager.Instance.PlayerData.playerHeadUi.position,
                speed * Time.deltaTime
            );

            // Get target rotation but restrict to Y axis only
            Quaternion targetRotation = GameManager.Instance.PlayerData.playerHeadUi.rotation;
            Vector3 targetEuler = targetRotation.eulerAngles;

            // Keep only Y axis
            //targetEuler = new Vector3(0f, targetEuler.y, 0f);

            // Convert back to Quaternion
            targetRotation = Quaternion.Euler(targetEuler);

            // Rotate smoothly toward the Y-axis target rotation
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
    public void updateFarUi()
    {
        // Match position directly
        transform.position = GameManager.Instance.PlayerData.farUi.position;

        // Get the target's Y-axis rotation only
        Quaternion targetRotation = GameManager.Instance.PlayerData.farUi.rotation;
        Vector3 targetEuler = targetRotation.eulerAngles;

        // Keep only Y axis
        targetEuler = new Vector3(0f, targetEuler.y, 0f);

        // Apply the Y-only rotation
        transform.rotation = Quaternion.Euler(targetEuler);
    }


    private IEnumerator ScaleUIRoutine()
    {
        float scale = 0f;
        float targetScale = 0.05f;

        // Start small
        transform.localScale = Vector3.zero;

        // Gradually scale up
        while (scale < targetScale)
        {
            scale += Time.deltaTime * scaleSpeed;
            transform.localScale = new Vector3(scale, scale, scale);
            yield return null; // Wait for next frame
        }

        // Ensure final scale is exact
        transform.localScale = new Vector3(targetScale, targetScale, targetScale);
    }
    private void OnEnable()
    {
        StartCoroutine(ScaleUIRoutine());
    }

  


}
