using UnityEngine;
using System.Collections;

public class hitStopScript : MonoBehaviour
{
    public float stopDuration;
    private bool stopped;

    public void enableHitStop()
    {
        if (stopped) return;
        StartCoroutine(doHitStop());
    }

    IEnumerator doHitStop()
    {
        stopped = true;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(stopDuration);
        Time.timeScale = 1f;
        stopped = false;
    }
}
