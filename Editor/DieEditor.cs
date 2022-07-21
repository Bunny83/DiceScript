#region License and Information
/*****
*
* DieEditor.cs
* 
* This is a custom editor for the "Die" class which allows easy editing
* of the side definitions (side normal and value).
* 
* 2017.05.17 - first version 
* 
* Copyright (c) 2017 Markus Göbel (Bunny83)
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to
* deal in the Software without restriction, including without limitation the
* rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
* sell copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
* IN THE SOFTWARE.
* 
*****/
#endregion License and Information
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Die))]
public class DieEditor : Editor
{
    Die m_Target;
    List<Die.DieSide> m_Sides;
    static bool m_EditMode = false;
    static int m_Selected = -1;
    static bool m_ShowHelp = false;
    static System.Type inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
    static string m_Help =@"
Left.Click in SceneView to update the current selected side normal
Right-Click in SceneView to add a new side
Note: if a side with the same normal already exists, the action is ignored
You can select sides in the inspector
Use ALT+ left drag to orbit. Keep in mind to center the view first (key ""F"")
";
    static bool EditMode
    {
        get { return m_EditMode; }
        set
        {
            if (m_EditMode != value)
            {
                m_EditMode = value;
                if (m_EditMode == false)
                    Tools.current = Tool.Move;
                SceneView.RepaintAll();
            }
            if (m_EditMode)
                Tools.current = Tool.None;
        }
    }
    void OnEnable()
    {
        
        m_Target = (Die)target;
        m_Sides = m_Target.sides;
        if (m_Sides == null)
            m_Sides = m_Target.sides = new List<Die.DieSide>();
    }
    void OnDisable()
    {
        EditMode = false;
    }
    public override void OnInspectorGUI()
    {
        EditMode = GUILayout.Toggle(EditMode, "Edit Sides", "Button");
        if (EditorApplication.isPlaying)
            GUILayout.Label("Current Value: " + m_Target.GetCurrentValue());
        if (EditMode)
        {
            GUILayout.Label("Sides: " + m_Sides.Count);
            Color old = GUI.color;
            for (int i = 0; i < m_Sides.Count; i++)
            {
                if (i == m_Selected)
                    GUI.color = Color.yellow;
                else
                    GUI.color = old;

                GUILayout.BeginHorizontal("box");
                GUILayout.Label(""+m_Sides[i].normal.ToString("F2"), GUILayout.Width(100));
                m_Sides[i].value = EditorGUILayout.IntField(m_Sides[i].value, GUILayout.MinWidth(20));
                if (GUILayout.Button("select", GUILayout.Width(45)))
                {
                    m_Selected = i;
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("flip", GUILayout.Width(30)))
                {
                    m_Sides[i].normal *= -1;
                    EditorUtility.SetDirty(m_Target);
                    SceneView.RepaintAll();
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    m_Sides.RemoveAt(i);
                    if (m_Selected >= m_Sides.Count)
                        m_Selected = m_Sides.Count - 1;
                    EditorUtility.SetDirty(m_Target);
                    SceneView.RepaintAll();
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndHorizontal();
            }
            GUI.color = old;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                m_Sides.Add(new Die.DieSide());
                EditorUtility.SetDirty(m_Target);
            }
            if (GUILayout.Button("flip all"))
            {
                for (int i = 0; i < m_Sides.Count; i++)
                    m_Sides[i].normal *= -1;
                EditorUtility.SetDirty(m_Target);
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();
            m_ShowHelp = GUILayout.Toggle(m_ShowHelp, "Show Help", "Button");
            if (m_ShowHelp)
            {
                GUILayout.TextArea(m_Help);
            }
        }
        else
        {
            m_Selected = -1;
            DrawDefaultInspector();
        }
    }

    int FindSide(Vector3 aNormal)
    {
        for (int i = 0; i < m_Sides.Count; i++)
        {
            if (Vector3.Angle(m_Sides[i].normal, aNormal) < 0.05f)
            {
                return i;
            }
        }
        return -1;
    }

    void OnSceneGUI()
    {
        if (!EditMode)
            return;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(id);
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        var trans = m_Target.transform;

        if (Physics.Raycast(ray, out hit) && hit.transform == trans && !e.alt)
        {
            Handles.color = Color.red;
            Handles.DrawLine(hit.point, hit.point + hit.normal);
            if (e.rawType == EventType.MouseDown)
            {
                var normal = trans.InverseTransformDirection(hit.normal);
                if (e.button == 0 && m_Selected >= 0 && m_Selected < m_Sides.Count && FindSide(normal) == -1)
                {
                    m_Sides[m_Selected].normal = normal;
                    EditorUtility.SetDirty(m_Target);
                }
                else if (e.button == 1)
                {
                    int side = FindSide(normal);
                    if (side == -1)
                    {
                        side = m_Sides.Count;
                        m_Sides.Add(new Die.DieSide());
                    }
                    m_Sides[side].normal = normal;
                    EditorUtility.SetDirty(m_Target);
                }
                else if (e.button == 2)
                {
                    int side = FindSide(normal);
                    if (side == -1)
                    {
                        return;
                    }
                    e.Use();
                    m_Selected = side;
                    SceneView.RepaintAll();
                    GUIUtility.ExitGUI();
                }
            }
        }

        for (int i = 0; i < m_Sides.Count; i++)
        {
            Handles.color = (i == m_Selected) ? Color.yellow : Color.cyan;
            var dir = trans.TransformDirection(m_Sides[i].normal);
            Handles.DrawLine(trans.position + dir * 0.1f, trans.position + dir);
        }
        if (Event.current.type == EventType.MouseMove)
            SceneView.RepaintAll();
    }
}
