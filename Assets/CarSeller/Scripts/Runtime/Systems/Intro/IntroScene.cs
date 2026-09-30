using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    private IEnumerator Start()
    {
        var handle = SceneManager.LoadSceneAsync(1);
        handle.allowSceneActivation = false;
        yield return new WaitForSeconds(2f);
        handle.allowSceneActivation = true;
    }
}