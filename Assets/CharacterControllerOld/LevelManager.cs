using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    int currentSceneIndex = 0;

    public void GoToNextLevel() {

#if UNITY_EDITOR
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
#endif

        currentSceneIndex++;
        SceneManager.LoadScene(currentSceneIndex);
    }
}
