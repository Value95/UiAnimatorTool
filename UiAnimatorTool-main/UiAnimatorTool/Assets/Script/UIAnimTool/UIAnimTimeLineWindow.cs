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

        private float contentsY;

        private float topY;
        
        public void CreateWindow(UiAnimator pUiAnimator)
        {
            _UiAnimator = pUiAnimator;
            _Init();
        
            titleContent = new GUIContent("UI Anim Time Line Window");
            minSize = new Vector2(1000, 500);
            Show();
        }

        public void CloseWindow()
        {
            _UiAnimator.SetDrawClip(null);
            this.Close();
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
            topY = pLeftRect.y;

            _contentRect = pLeftRect;
            _contentRect.height = 20;

            DrawStateButton();

            _contentRect.y -= contentsY;

            // Grup size만큼 내려가서 그리게
            _contentRect = _DrawClipGroup(_contentRect, pMouseEvent);

            // ADD Group
            var lAddRect = Rect.MinMaxRect(50, _contentRect.y, 200, _contentRect.y + 50);
            DrawAddClipGroup(lAddRect);

            // ClipGroup 위치 조종
            Rect lContentsRect = pLeftRect;
            lContentsRect.x = 10;
            lContentsRect.height = pLeftRect.height - pLeftRect.y - 50;

            float lSliderMaxValue = (_contentRect.y + contentsY - 50) - lContentsRect.height;
            
            if(lSliderMaxValue >= 0)
                _ContentsGroupSlider(lContentsRect, ref contentsY, 3.0f, 0, lSliderMaxValue);
            else
                contentsY = 0;

            // Game Scene Update
            if (!Application.isPlaying)
                _GameSceneUpdate();
            
            _UiAnimator.StateUpdate();
            _PlayingTime();
        }

        private void _ContentsGroupSlider(Rect area, ref float value, float increment, float pMinValue, float pMaxValue)
        {
            Event e = Event.current;

            // 특정 영역 안에 마우스 커서가 있는지 확인
            if (area.Contains(e.mousePosition))
            {
                // 마우스 휠 이벤트 처리
                if (e.type == EventType.ScrollWheel)
                {
                    float scrollDelta = e.delta.y;

                    // 마우스 휠 스크롤에 따라 float 값 증가 또는 감소
                    if (scrollDelta > 0)
                    {
                        value = Mathf.Min(value + increment, pMaxValue);
                    }
                    else if (scrollDelta < 0)
                    {
                        value = Mathf.Max(value - increment, pMinValue);
                    }

                    // 값 변경을 즉시 적용하기 위해 Repaint 호출
                    Repaint();

                    // 이벤트 사용 처리
                    e.Use();
                }
            }
            
            value = BasicDraw.VerticalSlider(area, value, 0, pMaxValue);
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

        private Rect _DrawClipGroup(Rect pContentRect, Event pMouseEvent)
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
            bool lIsDraw = false;
            
            AnimatorClipGroup lAnimatorClipGroup = _UiAnimator.GetAnimationClipGroup(pIndex);

            if (lAnimatorClipGroup.clipGroups is null)
                return (false, 0);
            
            int lClipGroupCount = lAnimatorClipGroup.clipGroups.Count;

            //* Clip영역 Rect 
            Rect lBackGroundRect = pLeftRect;

            if (lAnimatorClipGroup.isToogle)
                lBackGroundRect.height = (lBackGroundRect.height * (lClipGroupCount + 2)) + 10 + (lClipGroupCount * 5);

            lIsDraw = topY <= lBackGroundRect.y + lBackGroundRect.height;
            if (lIsDraw)
            {
                if (topY >= lBackGroundRect.y)
                {
                    lBackGroundRect.height -= topY - lBackGroundRect.y;
                    lBackGroundRect.y = topY;
                }

                BasicDraw.DrawRect(lBackGroundRect);
            }
            //*/

            //* 해당 그룹의 이름
            Rect lGroupNameRect = pLeftRect;
            lGroupNameRect.x += 20;

            lIsDraw = topY <= lGroupNameRect.y + lGroupNameRect.height;
            if (lIsDraw)
            {
                BasicDraw.GuiStyleRefresh();
                BasicDraw.DrawText(lGroupNameRect, lAnimatorClipGroup.groupName, TextAnchor.MiddleLeft);
            }
            //*/

            //*  on off
            Rect lOnOffRect = _buttonRect;
            lOnOffRect.x = pLeftRect.x;
            lOnOffRect.y = pLeftRect.y;

            lIsDraw = topY <= lOnOffRect.y +lOnOffRect.height;
            if(lIsDraw)
                DrawToggleLabel(lOnOffRect, lAnimatorClipGroup.isToogle ? "↑" : "↓", ref lAnimatorClipGroup.isToogle);
            //*/
            
            //* Clip Group 삭제
            Rect lClipRemoveAtRect = _buttonRect;
            lClipRemoveAtRect.x += pLeftRect.width - pLeftRect.x + 10;
            lClipRemoveAtRect.y = pLeftRect.y;
            
            lIsDraw = topY <= lClipRemoveAtRect.y + lClipRemoveAtRect.height;
            if (lIsDraw && BasicDraw.DrawButton(lClipRemoveAtRect, "-", Texture2D.blackTexture))
            {
                lAnimatorClipGroup.IsObjNullCheck();
                _UiAnimator.AnimationClipGroupsRemoveAt(pIndex);
                return (false, 0.0f);
            }
            //*/
            
            // Clip Contents
            if (lAnimatorClipGroup.isToogle)
            {
                //+ Clip Group 기능추가
                if (lAnimatorClipGroup.obj is not null)
                {
                    Rect lClipAddRect = _buttonRect;
                    lClipAddRect.x += pLeftRect.width;
                    lClipAddRect.y = pLeftRect.y;
                    
                    lIsDraw = topY <= lClipAddRect.y + lClipAddRect.height;
                    if (lIsDraw && BasicDraw.DrawButton(lClipAddRect, "+", Texture2D.blackTexture))
                    {
                        DrawAddClipGroup(lAnimatorClipGroup);
                    }
                }
                pLeftRect.y += pLeftRect.height + 10;
                //*/
                
                // clip내용 표시
                
                //* GameObject 추가
                Rect lGameObjectRect = pLeftRect;
                
                lIsDraw = topY <= lGameObjectRect.y + lGameObjectRect.height;
                if (lIsDraw)
                {
                    lAnimatorClipGroup.obj = (GameObject)EditorGUI.ObjectField(lGameObjectRect, "GameObject",
                        lAnimatorClipGroup.obj, typeof(GameObject), true);
                }

                pLeftRect.y += pLeftRect.height + 5;
                //*/

                //* Clip
                foreach (var clips in lAnimatorClipGroup.clipGroups)
                {
                    //* Clip 삭제 버튼
                    Rect lClipDeleteRect = _buttonRect;
                    lClipDeleteRect.x = pLeftRect.x;
                    lClipDeleteRect.y = pLeftRect.y;
                    
                    lIsDraw = topY <= lClipDeleteRect.y + lClipDeleteRect.height;
                    if (lIsDraw && BasicDraw.DrawButton(lClipDeleteRect, "-"))
                    {
                        lAnimatorClipGroup.clipGroups.Remove(clips);
                        break;
                    }
                    //*/
                    
                    //* Clip 이름표시
                    Rect lClipNameRect = pLeftRect;
                    lClipNameRect.x += 20;
                    
                    lIsDraw = topY <= lClipNameRect.y + lClipNameRect.height;
                    if(lIsDraw)
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
                            lClipRect.width = Mathf.Max(0, lClipRect.width - (timeLineRect.x - lClipRect.x));
                            lClipRect.x = timeLineRect.x;
                        }

                        lIsDraw = topY <= lClipRect.y  +lClipRect.height;
                        if (lIsDraw)
                        {
                            if (lSelectClip == clips.clipList[clipIndex])
                                BasicDraw.DrawRect(lClipRect, Color.magenta);
                            else
                                BasicDraw.DrawRect(lClipRect, Color.cyan);
                            
                        lClipRectList.Add(lClipRect);
                        }

                    }
                    //*/
                    
                    //* Clip TimeLine 추가 or 삭제
                    Rect lTimeLineClickRect = new Rect(timeLineRect.x, pLeftRect.y, timeLineRect.width, pLeftRect.height);

                    lIsDraw = topY <= lTimeLineClickRect.y + lTimeLineClickRect.height;
                    if (lIsDraw)
                    {
                        if (lSelectClip is not null && lSelectClip.eClipType == clips.clipType)
                            BasicDraw.DrawRect(lTimeLineClickRect, Color.black);
                        else
                            BasicDraw.DrawRect(lTimeLineClickRect);
                    }

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
                                    
                                    
                                    
                                    
                                    lAnimatorClipGroup.AddClip(_UiAnimator, clips.clipType, lStartTime, lEndTime);
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
        private void DrawAddClipGroup(AnimatorClipGroup pAnimatorClipGroup)
        {
            // 팝업 표시
            GenericMenu menu = new GenericMenu();

            foreach (Clip.ClipType clipTag in Enum.GetValues(typeof(Clip.ClipType)))
            {
                if (!pAnimatorClipGroup.ClipGroupContainsKey(clipTag))
                {
                    menu.AddItem(new GUIContent(clipTag.ToString()), false, () =>
                    {
                        pAnimatorClipGroup.AddClipGroup(clipTag);
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