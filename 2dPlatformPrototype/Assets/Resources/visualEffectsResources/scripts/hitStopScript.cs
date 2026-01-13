using UnityEngine;
using System.Collections;

public class hitStopScript : MonoBehaviour
{
    private bool stopped;

    public void enableHitStop(float duration)
    {
        if (stopped) return;
        StartCoroutine(doHitStop(duration));
    }

    IEnumerator doHitStop(float duration)
    {
        stopped = true;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        stopped = false;
    }
}
