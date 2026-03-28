using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum SceneType
    {
        Menu,
        CharSelectScene,
        Game
    }

    private static SceneType scene;

    public static void Load(SceneType targetScene)
    {
        scene = targetScene;

        SceneManager.LoadScene(scene.ToString());
    }

    public static void LoadNetwork(SceneType targetScene)
    {
        Debug.Log("start load");
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }


    public static void LoaderCallback()
    {
        SceneManager.LoadScene(scene.ToString());
    }
}
