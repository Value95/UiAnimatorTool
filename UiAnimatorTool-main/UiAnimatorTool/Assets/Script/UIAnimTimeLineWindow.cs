using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class UIAnimTimeLineWindow : DrawAnimTimeLine
    {
        public enum AnimState {None, Playing, Pause, ReSet}
        
        [SerializeField]
        private UiAnimator _UiAnimator;
        
        [SerializeField]
        private Rect _contentRect;
        [SerializeField]
        private Rect _buttonRect;
        
        public void CreateWindow(UiAnimator pUiAnimator)
        {
            _UiAnimator = pUiAnimator;
            _Init();
        
            titleContent = new GUIContent("UI Anim Time Line Window");
            minSize = new Vector2(1000, 500);
            Show();
        }
    
        private void _Init()
        {
            _buttonRect = new Rect(0, 0, 20, 20);
            
            _FirstDataRefresh();
            base.Init();
        }

        private void _FirstDataRefresh()
        {
            if (_UiAnimator is not null)
            {
                drawViewMin = _UiAnimator.drawViewMin;
                drawViewMax = _UiAnimator.drawViewMax;
                
                DataRefresh();
            }
        }
        
        public void DataRefresh()
        {
            if (_UiAnimator is not null)
            {
                animTime = _UiAnimator.animTime;
            }
        }

        protected override void ContentsUpdate(Rect pLeftRect, Event pMouseEvent)
        {
            _contentRect = pLeftRect;
            _contentRect.height = 20;
            
            DrawStateButton();
            
            // Grup size만큼 내려가서 그리게
            _contentRect = _DrawClipGroup(pLeftRect, _contentRect, pMouseEvent);
            
            // ADD Group
            var lAddRect = Rect.MinMaxRect(50,_contentRect.y,200,_contentRect.y + 50);
            DrawAddClipGroup(lAddRect);
            
            // Game Scene Update
            if(!Application.isPlaying)
                _GameSceneUpdate();
            
            
            _UiAnimator.StateUpdate();
            _PlayingTime();   
        }

        private void _PlayingTime()
        {
            if (_UiAnimator.animState == AnimState.Playing)
            {
                currentTime = _UiAnimator.curTime;
                this.Repaint();
            }
        }
        
        private void DrawStateButton()
        {
            Rect lStateButon = _buttonRect;
            lStateButon.x = 160;
            lStateButon.y += 40;

            switch (_UiAnimator.animState)
            {
                case AnimState.None:
                {
                    if (BasicDraw.DrawButton(lStateButon, "▶"))
                    {
                        currentTime = 0.0f;
                        _UiAnimator.ChanageState(AnimState.Playing);
                    }
                }
                    break;
                case AnimState.Playing:
                {
                    if (BasicDraw.DrawButton(lStateButon, "‖"))
                    {
                        _UiAnimator.ChanageState(AnimState.Pause);
                    }

                    lStateButon.x += lStateButon.width * 1.5f;
                    if (BasicDraw.DrawButton(lStateButon, "≪"))
                    {
                        currentTime = 0;
                        _UiAnimator.ChanageState(AnimState.ReSet);
                    }
                }
                    break;
                case AnimState.Pause:
                {
                    if (BasicDraw.DrawButton(lStateButon, "▶"))
                    {
                        currentTime = 0.0f;
                        _UiAnimator.ChanageState(AnimState.Playing);
                    }

                    lStateButon.x += lStateButon.width * 1.5f;
                    if (BasicDraw.DrawButton(lStateButon, "≪"))
                    {
                        currentTime = 0;
                        _UiAnimator.ChanageState(AnimState.ReSet);
                    }
                }
                    break;
                case AnimState.ReSet:
                {
                    // 버튼없음
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // 타임라인 클릭하여 계수찾아서
        private void _GameSceneUpdate()
        {
            _UiAnimator.Play(currentTime + drawViewMin);
        }
        
        private Rect _DrawClipGroup(Rect pLeftRect, Rect pContentRect, Event pMouseEvent)
        {
            int lCount = _UiAnimator.GetAnimationClipGroupCount();
            for (int i = 0; i < lCount; ++i)
            {
                // pLeftRect의 Y 시작점을 넘겨준다.
                var lastDrawnPosition = DrawClipContent(i, pContentRect, pMouseEvent);
                if (!lastDrawnPosition.Item1)
                    break;

                pContentRect.y = lastDrawnPosition.Item2;
            }

            return pContentRect;
        }
        
        private (bool, float) DrawClipContent(int pIndex, Rect pLeftRect, Event pMouseEvent)
        {
            AnimationClipGroup lAnimationClipGroup = _UiAnimator.GetAnimationClipGroup(pIndex);

            if (lAnimationClipGroup.clipGroups is null)
                return (false, 0);
            
            int lClipGroupCount = lAnimationClipGroup.clipGroups.Count;

            //* Clip영역 Rect 
            Rect lBackGroundRect = pLeftRect;

            if (lAnimationClipGroup.isToogle)
                lBackGroundRect.height = (lBackGroundRect.height * (lClipGroupCount + 2)) + 10 + (lClipGroupCount * 5);

            BasicDraw.DrawRect(lBackGroundRect);
            //*/

            //* 해당 그룹의 이름
            Rect lGroupNameRect = pLeftRect;
            lGroupNameRect.x += 20;
            
            BasicDraw.GuiStyleRefresh();
            BasicDraw.DrawText(lGroupNameRect, lAnimationClipGroup.groupName, TextAnchor.MiddleLeft);
            //*/

            //*  on off
            Rect lOnOffRect = _buttonRect;
            lOnOffRect.x = pLeftRect.x;
            lOnOffRect.y = pLeftRect.y;

            DrawToggleLabel(lOnOffRect, lAnimationClipGroup.isToogle ? "↑" : "↓", ref lAnimationClipGroup.isToogle);
            //*/
            
            //* Clip Group 삭제
            Rect lClipRemoveAtRect = _buttonRect;
            lClipRemoveAtRect.x += pLeftRect.width - pLeftRect.x;
            lClipRemoveAtRect.y = pLeftRect.y;
            
            if (BasicDraw.DrawButton(lClipRemoveAtRect, "-", Texture2D.blackTexture))
            {
                lAnimationClipGroup.IsObjNullCheck();
                _UiAnimator.AnimationClipGroupsRemoveAt(pIndex);
                return (false, 0.0f);
            }
            //*/
            
            // Clip Contents
            if (lAnimationClipGroup.isToogle)
            {
                //+ Clip Group 기능추가
                if (lAnimationClipGroup.obj is not null)
                {
                    Rect lClipAddRect = _buttonRect;
                    lClipAddRect.x += pLeftRect.width;
                    lClipAddRect.y = pLeftRect.y;
                    
                    if (BasicDraw.DrawButton(lClipAddRect, "+", Texture2D.blackTexture))
                    {
                        DrawAddClipGroup(lAnimationClipGroup);
                    }
                }
                pLeftRect.y += pLeftRect.height + 10;
                //*/
                
                // clip내용 표시
                
                //* GameObject 추가
                Rect lGameObjectRect = pLeftRect;
                
                lAnimationClipGroup.obj = (GameObject)EditorGUI.ObjectField(lGameObjectRect, "GameObject",
                    lAnimationClipGroup.obj, typeof(GameObject), true);
                pLeftRect.y += pLeftRect.height + 5;
                //*/

                //* Clip
                foreach (var clips in lAnimationClipGroup.clipGroups)
                {
                    //* Clip 삭제 버튼
                    Rect lClipDeleteRect = _buttonRect;
                    lClipDeleteRect.x = pLeftRect.x;
                    lClipDeleteRect.y = pLeftRect.y;
                    if (BasicDraw.DrawButton(lClipDeleteRect, "-"))
                    {
                        lAnimationClipGroup.clipGroups.Remove(clips);
                        break;
                    }
                    //*/
                    
                    //* Clip 이름표시
                    Rect lClipNameRect = pLeftRect;
                    lClipNameRect.x += 20;
                    BasicDraw.DrawText(lClipNameRect, clips.clipType.ToString(), TextAnchor.MiddleLeft);
                    //*/
                    
                    //* Clip 표시
                    List<Rect> lClipRectList = new List<Rect>();

                    Clip lSelectClip = _UiAnimator.drawClip;
                    
                    int lClipCount = clips.clipList.Count;
                    for (int clipIndex = 0; clipIndex < lClipCount; ++clipIndex)
                    {
                        Rect lClipRect = pLeftRect;
                        float lStartTime = drawViewMin / drawTime;
                        lClipRect.x = TimeLineValueToPosition((clips.clipList[clipIndex].startTime / drawTime) - lStartTime);
                        lClipRect.width = TimeLineValueToPosition((clips.clipList[clipIndex].endTime / drawTime) - lStartTime) - lClipRect.x;

                        if (lClipRect.x < timeLineRect.x)
                        {
                            lClipRect.width -= timeLineRect.x - lClipRect.x;
                            lClipRect.x = timeLineRect.x;
                        }
                        
                        if(lSelectClip == clips.clipList[clipIndex])
                            BasicDraw.DrawRect(lClipRect, Color.magenta);
                        else
                            BasicDraw.DrawRect(lClipRect, Color.cyan);
                        
                        lClipRectList.Add(lClipRect);
                    }
                    //*/
                    
                    //* Clip TimeLine 추가 or 삭제
                    Rect lTimeLineClickRect = new Rect(timeLineRect.x, pLeftRect.y, timeLineRect.width, pLeftRect.height);
                    
                    if(lSelectClip is not null && lSelectClip.eClipType == clips.clipType)
                        BasicDraw.DrawRect(lTimeLineClickRect, Color.black);
                    else
                        BasicDraw.DrawRect(lTimeLineClickRect);

                    if (pMouseEvent.type == EventType.ContextClick &&
                        lTimeLineClickRect.Contains(pMouseEvent.mousePosition))
                    {
                        bool lIsDelete = false;
                        GenericMenu lMenu = new GenericMenu();

                        int lClipRectCount = lClipRectList.Count;
                        for (int clipRectIndex = 0; clipRectIndex < lClipRectCount; ++clipRectIndex)
                        {
                            if (lClipRectList[clipRectIndex].Contains(pMouseEvent.mousePosition))
                            {
                                lMenu.AddItem(new GUIContent("Delete Clip"), false,
                                    () =>
                                    {
                                        if (clips.clipList[clipRectIndex] == _UiAnimator.drawClip)
                                            _UiAnimator.SetDrawClip(null);

                                        clips.clipList.RemoveAt(clipRectIndex);
                                    });

                                lIsDelete = true;
                                break;
                            }
                        }

                        if (!lIsDelete)
                        {
                            float lTimeLineValue = PositionToTimeLineValue(pMouseEvent.mousePosition.x);
                            float lStartTime = drawViewMin + (lTimeLineValue * drawTime);
                            lMenu.AddItem(new GUIContent("Add Clip"), false,
                                () =>
                                {
                                    float lEndTime = Mathf.Min(lStartTime + (0.1f * drawViewMax), drawViewMax);
                                    
                                    
                                    
                                    
                                    lAnimationClipGroup.AddClip(_UiAnimator, clips.clipType, lStartTime, lEndTime);
                                });
                        }


                        lMenu.ShowAsContext();
                    }
                    //*/
                    
                    //* 클립 셋팅 
                    int lClipRectCount1 = lClipRectList.Count;
                    for (int clipRectIndex = 0; clipRectIndex < lClipRectCount1; ++clipRectIndex)
                    {
                        if(pMouseEvent.type == EventType.MouseUp && pMouseEvent.button == 0)
                        {
                            if (lClipRectList[clipRectIndex].Contains(pMouseEvent.mousePosition))
                            {
                                // 클립 선택
                                _UiAnimator.SetDrawClip(clips.clipList[clipRectIndex]);
                            }
                        }
                    }
                    //*/
                   
                    pLeftRect.y += pLeftRect.height + 5;
                }
                //*/
            }
            else
            {
                pLeftRect.y += pLeftRect.height + 10;
            }
            

            return (true , pLeftRect.y + 10.0f); // 자기자신의 마지막 y좌표
        }
        
        // ClipGroup 추가
        private void DrawAddClipGroup(Rect pAddRect)
        {
            GUI.color = Color.white;
            if (BasicDraw.DrawButton(pAddRect, "Add Actor Group"))
            {
                _UiAnimator.AddAnimClipGroup();
            }
        }
        
        // 클립의 기능추가
        private void DrawAddClipGroup(AnimationClipGroup pAnimationClipGroup)
        {
            // 팝업 표시
            GenericMenu menu = new GenericMenu();

            foreach (Clip.ClipType clipTag in Enum.GetValues(typeof(Clip.ClipType)))
            {
                if (!pAnimationClipGroup.ClipGroupContainsKey(clipTag))
                {
                    menu.AddItem(new GUIContent(clipTag.ToString()), false, () =>
                    {
                        pAnimationClipGroup.AddClipGroup(clipTag);
                    });
                }
            }

            menu.ShowAsContext();
        }

        public void DrawToggleLabel(Rect rect, string pText, ref bool isToggled)
        {
            if (BasicDraw.DrawButton(rect, pText))
            {
                isToggled = !isToggled;
            }
        }

    }