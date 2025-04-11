using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : BaseManager
{
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                // 씬에 있는 GameManager 찾기
                instance = FindObjectOfType<UIManager>();

                // 없으면 새로 생성
                if (instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    instance = go.AddComponent<UIManager>();
                }

                // 씬 전환 시 파괴되지 않도록 설정
                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
    }

    [SerializeField] private GameObject windowParent;
    [SerializeField] private GameObject popupParent;
    
    
    private Queue<BaseWindow> windowHistory;
    private Queue<BasePopup> popupHistory;

    public BaseWindow topWindow { get; private set; }
    public BasePopup topPopup { get; private set; }
    
    public override void Prepare()
    {
    }
    
    public override void Run()
    {
        windowHistory = new Queue<BaseWindow>();
        popupHistory = new Queue<BasePopup>();
    }
    


    public BaseWindow ShowWindow<T>(object[] param = null) where T : BaseWindow, new()
    {
        T window = new T();
        GameObject windowPrefab = Resources.Load<GameObject>(window.Path());
        GameObject.Instantiate(windowPrefab, windowParent.transform);
        
        if (windowPrefab == null)
        {
            Debug.Log("windowPrefab is Null");
            return null;
        }

        window = windowPrefab.GetComponent<T>();
        
        if (window == null)
        {
            Debug.Log("window is Null");
            return null;
        }

        window.AnimTrigger("In", () =>
        {
            window.Open(param);
        });
        
        windowHistory.Enqueue(window);
        return window;
    }

    public void CloseWindow()
    {
        BaseWindow window = windowHistory.Dequeue();

        if (window == null)
        {
            Debug.Log("window is Null");
            return;
        }

        window.Close();    
        window.AnimTrigger("Out", () =>
        {
            window.CloseAnim();
        });
    }
}
