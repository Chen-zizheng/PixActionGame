using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // 这里填你的游戏场景名字（比如SampleScene，必须和你做游戏的场景名完全一致）
    public string gameSceneName = "SampleScene";

    // 开始游戏按钮的功能
    public void StartGame()
    {
        Debug.Log("开始游戏！");
        SceneManager.LoadScene(gameSceneName); // 切换到游戏场景
    }

    // 设置按钮的功能（后续可以扩展音量、分辨率设置）
    public void OpenSettings()
    {
        Debug.Log("打开设置面板");
    }

    // 退出游戏按钮的功能
    public void QuitGame()
    {
        Debug.Log("退出游戏！");
        // 编辑器里停止运行，打包游戏后会真正关闭程序
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}