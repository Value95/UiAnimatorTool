using UnityEditor;
using UnityEngine;

public static class BasicDraw
{
    public static GUIStyle labelStyle;
    
    public static void GuiStyleRefresh()
    {
        labelStyle = new GUIStyle(GUI.skin.label);
    }
    
    public static void DrawText(Rect pRect, float pValue, string pFormat = "")
    {
        GUILayout.BeginArea(pRect); // pRect 영역 내부에 레이블을 그리기 위해 BeginArea 호출
        GUILayout.Label((pValue).ToString(pFormat), labelStyle); // 레이블 그리기
        GUILayout.EndArea(); // BeginArea와 짝을 이루는 EndArea 호출
    }
    
    public static void DrawText(Rect pRect, string pString, TextAnchor pTextAnchor = TextAnchor.MiddleCenter)
    {
        GUILayout.BeginArea(pRect); // pRect 영역 내부에 레이블을 그리기 위해 BeginArea 호출
        labelStyle.alignment = pTextAnchor;
        GUILayout.Label(pString, labelStyle); // 레이블 그리기
        GUILayout.EndArea(); // BeginArea와 짝을 이루는 EndArea 호출
    }
    
    public static void BackGroundColor(Color pColor)
    {
        labelStyle.normal.textColor = pColor;
    }

    public static void DrawRect(Rect pRect, Color pColor)
    {
        GUI.backgroundColor = pColor;
        DrawRect(pRect);
        GUI.backgroundColor = Color.white;
    }
    
    public static void DrawRect(Rect pRect)
    {
        GUILayout.BeginArea(pRect); // pRect 영역 내부에 레이블을 그리기 위해 BeginArea 호출
        GUILayout.Box("", GUILayout.Width(pRect.width), GUILayout.Height(pRect.height)); // 빈 상자를 그려서 Rect를 표현
        GUILayout.EndArea(); // BeginArea와 짝을 이루는 EndArea 호출
    }

    public static void DrawLine(Vector3 pStart, Vector3 pEnd)
    {
        Handles.DrawLine(pStart, pEnd);   
    }

    public static bool DrawButton(Rect pRect, string pText, Texture2D pTexture = null)
    {
        GUILayout.BeginArea(pRect); // 버튼을 그릴 영역 설정
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.background = pTexture;

        bool lbool = GUILayout.Button(pText, buttonStyle);
        
        GUILayout.EndArea();
        
        return lbool;
    }

    public static float DrawSlider(Rect pRect, float pValue, float pLeftValue, float pRightValue)
    {
        return  EditorGUI.Slider(pRect, pValue, pLeftValue, pRightValue);
    }

    public static float VerticalSlider(Rect pRect, float pValue, float pLeftValue, float pRightValue)
    {
        pValue = GUI.VerticalSlider(pRect, pValue, pLeftValue, pRightValue);

        return pValue;
    }

    public static void DrawSlider(Rect pRect, ref float pValue, float pLeftValue, float pRightValue)
    {
        pValue =  EditorGUI.Slider(pRect, pValue, pLeftValue, pRightValue);
    }

    public static void DrawMinMaxSlider (Rect pRect,ref float pMinValue,ref float pMaxValue,float pMinLimit,float pMaxLimit)
    {
        EditorGUI.MinMaxSlider(pRect, ref pMinValue, ref pMaxValue, pMinLimit, pMaxLimit);
    }
}
