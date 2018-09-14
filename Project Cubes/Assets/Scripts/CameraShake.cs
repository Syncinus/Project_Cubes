using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {

    public static CameraShake instance;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        ShakeCamera(10f, 0.01f);
    }

    public IEnumerator Shake (float duration, float magnitude)
    {
        Vector3 originalPos = this.transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;


            this.transform.localPosition = new Vector3(this.transform.localPosition.x + x, this.transform.localPosition.y + y, this.transform.localPosition.z);
            yield return new WaitForEndOfFrame();
            this.transform.localPosition = originalPos;            

            elapsed += Time.deltaTime;

            yield return null;
        }

        this.transform.localPosition = originalPos;
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }


}
