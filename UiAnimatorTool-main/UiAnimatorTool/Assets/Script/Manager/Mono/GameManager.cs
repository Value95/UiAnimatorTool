using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BaseManager
{
   private static GameManager instance;

   public static GameManager Instance
   {
      get
      {
         if (instance == null)
         {
            // 씬에 있는 GameManager 찾기
            instance = FindObjectOfType<GameManager>();

            // 없으면 새로 생성
            if (instance == null)
            {
               GameObject go = new GameObject("GameManager");
               instance = go.AddComponent<GameManager>();
            }

            // 씬 전환 시 파괴되지 않도록 설정
            DontDestroyOnLoad(instance.gameObject);
         }

         return instance;
      }
   }

   private void Awake()
   {
      GameManager.Instance.Prepare();
      SceneManager.Instance.Prepare();
      UIManager.Instance.Prepare();
   }

   private void Start()
   {
      GameManager.Instance.Run();
      SceneManager.Instance.Run();
      UIManager.Instance.Run();
   }


   public override void Prepare()
   {
      
   }

   public override void Run()
   {
      
   }
}
