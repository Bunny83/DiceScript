#region License and Information
/*****
*
* Die.cs
* 
* This is a general purpose dice script. It should be attached to a die object
* and allows you to define the different sides of the die in the inspector and
* SceneView. Each "side normal" should point upwards when the corresponding
* side should be "active". For a "d4" (tetrahedron) you may want to use the
* "flip" function on each normal after you defined the normals of the opposite
* sides / faces.
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

public class Die : MonoBehaviour
{
    [System.Serializable]
    public class DieSide
    {
        public int value;
        public Vector3 normal;
    }
    public List<DieSide> sides;
    public DieSide GetCurrentSide()
    {
        return GetCurrentSide(Vector3.up);
    }
    public DieSide GetCurrentSide(Vector3 upReference)
    {
        if (sides == null || sides.Count == 0)
            return null;
        var up = transform.InverseTransformDirection(upReference);
        DieSide side = null;
        float ang = float.MaxValue;
        for(int i = 0;i < sides.Count; i++)
        {
            float a = Vector3.Angle(sides[i].normal, up);
            if (a < ang)
            {
                ang = a;
                side = sides[i];
            }
        }
        return side;
    }
    public int GetCurrentValue()
    {
        return GetCurrentValue(Vector3.up);
    }

    public int GetCurrentValue(Vector3 upReference)
    {
        var side = GetCurrentSide(upReference);
        if (side != null)
            return side.value;
        return 0;
    }
}
