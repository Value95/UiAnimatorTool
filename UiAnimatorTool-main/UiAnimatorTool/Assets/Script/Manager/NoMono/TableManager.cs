using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : BaseManager
{
    private static TableManager _instance;

    public static TableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬에 있는 GameManager 찾기
                _instance = FindObjectOfType<TableManager>();

                // 없으면 새로 생성
                if (_instance == null)
                {
                    GameObject go = new GameObject("TableManager");
                    _instance = go.AddComponent<TableManager>();
                }

                // 씬 전환 시 파괴되지 않도록 설정
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }
    
    public override void Prepare()
    {
        throw new System.NotImplementedException();
    }

    public override void Run()
    {
        throw new System.NotImplementedException();
    }
}
