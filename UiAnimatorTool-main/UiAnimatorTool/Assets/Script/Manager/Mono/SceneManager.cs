using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;

public class SceneManager : BaseManager
{
    private static SceneManager instance;

    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                // 씬에 있는 GameManager 찾기
                instance = FindObjectOfType<SceneManager>();

                // 없으면 새로 생성
                if (instance == null)
                {
                    GameObject go = new GameObject("SceenManager");
                    instance = go.AddComponent<SceneManager>();
                }

                // 씬 전환 시 파괴되지 않도록 설정
                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
    }

    public override void Prepare()
    {
    }

    public override void Run()
    {
    }

    public static IEnumerator LoadSceneAsync(string eSceneName, Action<bool> callBack)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(eSceneName);

        while (asyncLoad is { isDone: false })
        {
            callBack?.Invoke(true);

            yield return null;
        }
    }
}
