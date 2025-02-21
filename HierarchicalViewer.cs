using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using static TreeEditor.TreeEditorHelper;

#if UNITY_EDITOR
using UnityEditor;
#endif


/************************************************************************
 *    해당 컴포넌트가 부착된 게임 오브젝트의 계층구조를 표시하고, 각 본들의
 *    트랜스폼을 씬에서 직관적으로 조작할 수 있도록 해주는 컴포넌트입니다...
 * *****/
public sealed class HierarchicalViewer : MonoBehaviour
{
    #region EDITOR_EXTENSION
#if UNITY_EDITOR
    [CustomEditor(typeof(HierarchicalViewer))]
    private sealed class HierarchicalViewerEditor : Editor
    {
        //=============================================
        //////            Fields..                /////
        ///============================================
        private const  int            BONE_QUAD_SIZE      = 12;
        private const  float          BONE_QUAD_HALF_SIZE = (BONE_QUAD_SIZE * .5f);
        private static readonly Color BONE_QUAD_COLOR     = new Color(.8f, .8f, .8f);
        private static readonly Color GRAB_QUAD_COLOR     = new Color(0f, 1f, 0f);

        private static GUIStyle NodeButtonStyle, GrabNodeButtonStyle, NameTxtStyle;

        /**selection....*/
        private Transform          _showTargetTr;
        private Transform          _selection = null;
        private SerializedProperty _showTarget;
        private SerializedProperty _lineColor;
        private Tool               _toolType = Tool.Move;



        //============================================================
        ///////         Magic and Override methods...          ///////
        ////==========================================================
        private void OnSceneGUI()
        {
            #region Omit
            HierarchicalViewer curTarget = (target as HierarchicalViewer);
            if (curTarget == null) return;


            /*************************************************
             *   모든 본들을 표시한다....   
             * ****/
            GUI_Initialize();

            Handles.BeginGUI();
            {
                Transform rootTr       = (_showTargetTr != null ? _showTargetTr : curTarget.transform);
                Vector2   targetGUIPos = HandleUtility.WorldToGUIPoint(rootTr.position);
                GUI_ShowTargetChildrens(rootTr, targetGUIPos);
            }
            Handles.EndGUI();


            /********************************************************
             *    선택된 본이 있다면 트랜스폼을 수정할 수 있도록 한다...
             * ******/
            if (_selection!=null){

                /*해당 오브젝트가 비활성화 되었다면 스킵한다...**/
                Tools.current = Tool.None;
                using(var scope = new EditorGUI.ChangeCheckScope())
                {
                    Vector3    newPos   = _selection.position;
                    Vector3    guiPos   = HandleUtility.WorldToGUIPointWithDepth(newPos);
                    Vector3    newScale = _selection.localScale; 
                    Quaternion newQuat  = _selection.rotation;


                    /********************************************************
                     *    특정 키가 입력되었다면, 키에 대응되는 도구로 변경한다..
                     * ******/
                    Event curr = Event.current;
                    if(curr.type==EventType.KeyDown)
                    {
                        if (curr.keyCode==KeyCode.W)     _toolType = Tool.Move;
                        else if(curr.keyCode==KeyCode.E) _toolType = Tool.Rotate;
                        else if(curr.keyCode==KeyCode.R) _toolType = Tool.Scale;
                    }


                    /*********************************************************
                     *    현재 선택된 두고에 따라 트랜스폼 변경을 적용한다...
                     * ******/
                    switch(_toolType){

                            /**이동 도구일 경우...**/
                            case (Tool.Move):
                            {
                                newPos = Handles.PositionHandle(_selection.position, newQuat);
                                break;
                            }

                            /**회전 도구일 경우...*/
                            case (Tool.Rotate):
                            {
                                newQuat = Handles.RotationHandle(_selection.rotation, newPos);
                                break;
                            }

                            /**축적 도구일 경우...*/
                            case (Tool.Scale):
                            {
                                newScale = Handles.ScaleHandle(_selection.localScale, newPos, newQuat);
                                break;
                            }
                    }


                    /**************************************************
                     *   값이 바뀌었다면 갱신한다...
                     * ******/
                    if (scope.changed){
                        Undo.RecordObject(_selection, $"Changed transform of {_selection.name}.");
                        _selection.position   = newPos;
                        _selection.rotation   = newQuat;
                        _selection.localScale = newScale;
                    }

                    Handles.BeginGUI();
                    {
                        if(guiPos.z>=.1f)
                        GUI_ShowBoneTransform(guiPos, _selection);
                    }
                    Handles.EndGUI();
                }
            }

            #endregion
        }

        private void OnEnable()
        {
            GUI_Initialize();
            _selection = null;
        }

        public override void OnInspectorGUI()
        {
            #region Omit
            EditorGUILayout.HelpBox("If the Root Transform is not valid, it will be displayed based on the Transform to which the component is attached.", MessageType.Info);

            using (var scope = new EditorGUI.ChangeCheckScope()){
                UnityEngine.Object trRet    = EditorGUILayout.ObjectField("Root Transform", _showTarget.objectReferenceValue, typeof(Transform), true);
                Color              colorRet = EditorGUILayout.ColorField("Line Color", _lineColor.colorValue);

                if (scope.changed){
                    _showTarget.objectReferenceValue = trRet;
                    _lineColor.colorValue            = colorRet;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            #endregion
        }



        ///================================================
        ////////           GUI methods...           ///////
        /////==============================================
        private void GUI_Initialize()
        {
            #region Omit
            /*************************************************
             *    각종 프로퍼티와 스타일들을 초기화한다....
             * ******/

            _lineColor  = serializedObject.FindProperty("LineColor");
            _showTarget = serializedObject.FindProperty("ShowTarget");
            if(_showTarget!=null){
                _showTargetTr = (_showTarget.objectReferenceValue as Transform);
            }

            /**스타일을 초기화한다...*/
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, BONE_QUAD_COLOR);
            t.Apply();

            Texture2D t2 = new Texture2D(1, 1);
            t2.SetPixel(0, 0, GRAB_QUAD_COLOR);
            t2.Apply();

            NodeButtonStyle = new GUIStyle();
            NodeButtonStyle.normal.background = t;

            GrabNodeButtonStyle = new GUIStyle();
            GrabNodeButtonStyle.normal.background = t2;

            try{
                NameTxtStyle = new GUIStyle(EditorStyles.textField);
                NameTxtStyle.alignment = TextAnchor.MiddleCenter;
            }
            catch (Exception e) { }

            #endregion
        }

        private void GUI_ShowBoneTransform(Vector2 pos, Transform selection)
        {
            #region Omit
            Rect fieldRect = new Rect(
                   (pos - new Vector2(150f, -100f)),
                   new Vector3(300f, 40f)
            );

            Rect nameRect = new Rect(
                   (pos - new Vector2(100f, -130f)),
                   new Vector3(200f, 20f)
            );

            /**선택한 본의 이름을 출력한다...*/
            EditorGUI.TextField(nameRect, "", selection.name, NameTxtStyle);


            /********************************************************
             *   현재 선택된 도구에 따라서 적절한 GUI를 출력한다...
             * ******/

            /**이동 도구일 경우...*/
            if (_toolType==Tool.Move){
                EditorGUI.Vector3Field(fieldRect, "", _selection.position);
                return;
            }

            /**회전 도구일 경우...*/
            else if (_toolType == Tool.Rotate){
                EditorGUI.Vector3Field(fieldRect, "", _selection.localEulerAngles);
                return;
            }

            /**스케일 도구일 경우...*/
            else if (_toolType == Tool.Scale){
                EditorGUI.Vector3Field(fieldRect, "", _selection.localScale);
                return;
            }

            #endregion
        }

        private void GUI_ShowTargetChildrens(Transform tr, Vector2 prevHandlePos, bool isSelectionChild=false)
        {
            #region Omit
            int childCount        = tr.childCount;
            Vector3 trPos         = tr.position;
            Vector3 trGUIPos      = HandleUtility.WorldToGUIPointWithDepth(trPos);
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.current);

            /***************************************************
             *   tr의 모든 자식들을 GUI로 표시한다....
             * *****/
            for (int i=0; i<childCount; i++){

                Transform child       = tr.GetChild(i);
                GUIStyle  nodeStyle   = NodeButtonStyle;
                Vector3   childGUIPos = HandleUtility.WorldToGUIPointWithDepth(child.position);

                /**해당 자식이 선택된 노드인가?*/
                if(_selection==child)
                {
                    nodeStyle        = GrabNodeButtonStyle;
                    isSelectionChild = true;
                }

                /*********************************************************
                 *    본이 카메라에서 컬링되지 않았을 경우에만 표시한다...
                 * *****/
                Bounds pointBounds = new Bounds(child.position, Vector3.zero);

                if (GeometryUtility.TestPlanesAABB(frustumPlanes, pointBounds))
                {
                    Handles.color = (isSelectionChild && _selection != child ? GRAB_QUAD_COLOR : _lineColor.colorValue);
                    Handles.DrawAAPolyLine(10f, trGUIPos, childGUIPos);

                    Handles.color = Color.black;
                    Handles.DrawAAPolyLine(7f, trGUIPos, childGUIPos);

                    /**해당 버튼을 누르면 트랜스폼을 편집할 수 있도록 한다...*/
                    if (GUI_ShowBoneButton(childGUIPos, nodeStyle)){
                        _selection = child;
                    }
                }

                /**child가 자식을 가지고 있다면, 계층구조를 모조리 표시한다...*/
                if(child.childCount>0)
                {
                    GUI_ShowTargetChildrens(child, childGUIPos, isSelectionChild);
                }

                /**해당 자식이 선택된 노드인가?*/
                if (_selection == child){
                    isSelectionChild = false;
                }
            }
            #endregion
        }

        private bool GUI_ShowBoneButton(Vector2 pos, GUIStyle style)
        {
            #region Omit
            Rect  btnRect = new Rect(
                (pos - new Vector2( BONE_QUAD_HALF_SIZE, BONE_QUAD_HALF_SIZE)),
                new Vector3(BONE_QUAD_SIZE, BONE_QUAD_SIZE)
            );

            return GUI.Button(btnRect, GUIContent.none, style);
            #endregion
        }

    }
#endif
    #endregion 

    [SerializeField] public Transform ShowTarget;
    [SerializeField] public Color     LineColor = Color.black;
}
