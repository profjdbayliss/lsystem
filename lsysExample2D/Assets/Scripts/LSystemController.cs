using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Diagnostics;

public class LSystemController : MonoBehaviour {

    
    Hashtable ruleHash = new Hashtable(100);

	public float initial_length = 2;
	public float initial_radius = 1.0f;
    List<byte> start; //new StringBuilder("");
                      //StringBuilder lang = new StringBuilder("");
    List<byte> lang;
	GameObject contents;
	float angleToUse = 45f;
	public int iterations = 4;
    int drawnThings = 0;

	// for drawing lines
	public float lineWidth = 1.0f;
    Mesh lineMesh;
    List<Color> colors;
    List<Vector3> vertices;
    List<int> indices;
    public Material lineMaterial;
    MeshRenderer renderer;
    MeshFilter filter;

    void Start () {

        Stopwatch watch = new Stopwatch();

        // create the object to draw with some default values for the mesh and rendering
        contents = GameObject.CreatePrimitive(PrimitiveType.Cube);
        contents.transform.position = new Vector3(0, 0f, 0);
        filter = (MeshFilter)contents.GetComponent("MeshFilter");
        renderer = (MeshRenderer)filter.GetComponent<MeshRenderer>();
        renderer.material = lineMaterial;
        lineMesh = new Mesh();

        watch.Start();
        //variables : 0, 1
        //constants: [, ]
        //axiom  : 0
        //rules  : (1 → 11), (0 → 1[0]0)
        // Second example LSystem from 
        // http://en.wikipedia.org/wiki/L-system
        start = new List<byte>(100);
        //start.Add(0);
        //lang = start;
        ////new StringBuilder("0");
        //byte[] firstRule = new byte[] { 1, 1 };
        //ruleHash.Add((byte)1, firstRule);
        //byte[] secondRule = new byte[] { 1, 6, 0, 9, 0 };
        //ruleHash.Add((byte)0, secondRule);
        //angleToUse = 45f;
        //run(iterations);
        //watch.Stop();
        //UnityEngine.Debug.Log("Time for generation took: " + watch.ElapsedMilliseconds);
        //watch.Reset();
        //watch.Start();
        //colors = new List<Color>(lang.Count);
        //vertices = new List<Vector3>(lang.Count);
        //indices = new List<int>(lang.Count);
        //display2();



        // Weed type plant example from: 
        // http://en.wikipedia.org/wiki/L-system
        // rules: X = 0, F = 1, 
        // [ = 6, ] = 9
        // + = 2, - = 4
        //start = new StringBuilder("X");
        start.Add(0);
        //ruleHash.Add("X", "F-[[X]+X]+F[+FX]-X");
        byte[] firstRule = new byte[] { 1, 4, 6, 6, 0, 9, 2, 0, 9, 2, 1, 6, 2, 1, 0, 9, 4, 0 };
        ruleHash.Add((byte)0, firstRule);
        //ruleHash.Add("F", "FF");
        byte[] secondRule = new byte[] { 1, 1 };
        ruleHash.Add((byte)1, secondRule);     
        
        angleToUse = 25f;
        run(iterations);
        watch.Stop();
        UnityEngine.Debug.Log("Time for generation took: " + watch.ElapsedMilliseconds);
        UnityEngine.Debug.Log("Size of lang is: " + lang.Count);
        watch.Reset();
        watch.Start();
        colors = new List<Color>(lang.Count);
        vertices = new List<Vector3>(lang.Count);
        indices = new List<int>(lang.Count);
        display3();


        watch.Stop();
        UnityEngine.Debug.Log("Time for display took: " + watch.ElapsedMilliseconds);
        UnityEngine.Debug.Log("Count of vertices in list: " + vertices.Count);

    }

    // Get a rule from a given letter that's in our array
    byte[] getRule( byte[] input) {		
		if (ruleHash.ContainsKey(input[0]))
        {
            return (byte[])ruleHash[input[0]];

        }
		return input;
	}
	
	// Run the lsystem iterations number of times on the start axiom.
	void run(int iterations) {
    	List<byte> buffer1 = start;
        List<byte> buffer2 = new List<byte>(100);
        List<byte> currentList = buffer1;
        List<byte> newList = buffer2;
        byte[] singleByte = new byte[] { 0 };
        int currentCount = 0;

        for (int i = 0; i < iterations; i++) {
            currentCount = currentList.Count;
        	for (int j = 0; j < currentCount; j++) {
                singleByte[0] = currentList[j];
                byte[] buff = getRule(singleByte );
                newList.AddRange(buff);
        	}
            List<byte> tmp = currentList;
            currentList = newList;
            tmp.Clear();
            newList = tmp;
    	}
        
        lang = currentList;
        //for (int i = 0; i < start.Count; i++)
        //{
        //    UnityEngine.Debug.Log(lang[i]);
        //}

    }


    // The display routine for the weed type plant above
    void display3() {

        // to push and pop location and angles
        Stack<float> positions = new Stack<float>(10);
        Stack<float> angles = new Stack<float>(10);

        // current location and angle
        float angle = 0f;
        Vector3 position = new Vector3(0, 0, 0);
        float posy = 0.0f;
        float posx = 0.0f;

        // location and rotation to draw towards
        Vector3 newPosition;
        Vector2 rotated;

        // start at 0,0,0
        // Apply all the drawing rules to the lsystem string
        for (int i = 0; i < lang.Count; i++)
        {
            byte buff = lang[i];
            switch (buff)
            {
                case 1:
                    // draw a line 
                    posy += initial_length;
                    newPosition = new Vector3(position.x, posy, 0);
                    rotated = rotate(position, new Vector3(position.x, posy, 0), angle);
                    newPosition = new Vector3(rotated.x, rotated.y, 0);
                    addLineToMesh(lineMesh, position, newPosition, Color.green);
                    // set up for the next draw
                    position = newPosition;
                    posx = newPosition.x;
                    posy = newPosition.y;
                    break;
                case 2:
                    // Turn right 25
                    angle += angleToUse;
                    break;
                case 4:
                    // Turn left 25
                    angle -= angleToUse;
                    break;
                case 6:
                    //[: push position and angle
                    positions.Push(posy);
                    positions.Push(posx);
                    float currentAngle = angle;
                    angles.Push(currentAngle);
                    break;
                case 9:
                    //]: pop position and angle
                    posx = positions.Pop();
                    posy = positions.Pop();
                    position = new Vector3(posx, posy, 0);
                    angle = angles.Pop();
                    break;
                default: break;
            }

            // after we recreate the mesh we need to assign it to the original object
            filter.mesh = lineMesh;

        }

    }

    // Display routine for 2d examples in the main program
    void display2()
    {

        // to push and pop location and angle
        Stack<float> positions = new Stack<float>();
        Stack<float> angles = new Stack<float>();

        // current angle and position
        float angle = 0f;
        Vector3 position = new Vector3(0, 0, 0);
        float posy = 0.0f;
        float posx = 0.0f;

        // positions to draw towards
        Vector3 newPosition;
        Vector2 rotated;

        // start at 0,0,0        

        // Apply the drawing rules to the string given to us
        for (int i = 0; i < lang.Count; i++)
        {
            byte buff = lang[i];
            switch (buff)
            {
                case 0:
                    // draw a line ending in a leaf
                    posy += initial_length;
                    newPosition = new Vector3(position.x, posy, 0);
                    rotated = rotate(position, new Vector3(position.x, posy, 0), angle);
                    newPosition = new Vector3(rotated.x, rotated.y, 0);
                    addLineToMesh(lineMesh, position, new Vector3(rotated.x, rotated.y, 0), Color.green);
                    //drawLSystemLine(position, new Vector3(rotated.x, rotated.y, 0), line, Color.red);
                    // set up for the next draw
                    position = newPosition;
                    posx = newPosition.x;
                    posy = newPosition.y;
                    addCircleToMesh(lineMesh, 0.45f, 0.45f, position, Color.magenta);
                    break;
                case 1:
                    // draw a line 
                    posy += initial_length;
                    newPosition = new Vector3(position.x, posy, 0);
                    rotated = rotate(position, new Vector3(position.x, posy, 0), angle);
                    newPosition = new Vector3(rotated.x, rotated.y, 0);
                    //drawLSystemLine(position, newPosition, line, Color.green);
                    addLineToMesh(lineMesh, position, newPosition, Color.green);
                    // set up for the next draw
                    position = newPosition;
                    posx = newPosition.x;
                    posy = newPosition.y;
                    break;
                case 6:
                    //[: push position and angle, turn left 45 degrees
                    positions.Push(posy);
                    positions.Push(posx);
                    float currentAngle = angle;
                    angles.Push(currentAngle);
                    angle -= 45;
                    break;
                case 9:
                    //]: pop position and angle, turn right 45 degrees
                    posx = positions.Pop();
                    posy = positions.Pop();
                    position = new Vector3(posx, posy, 0);
                    angle = angles.Pop();
                    angle += 45;
                    break;
                default: break;
            }
            // after we recreate the mesh we need to assign it to the original object
            filter.mesh = lineMesh;
        }
    }


    void addLineToMesh(Mesh mesh, Vector3 from, Vector3 to, Color color)
    {
        Vector3[] lineVerts = new Vector3[] { from, to };
        int numberOfPoints = vertices.Count;
        int[] indicesForLines = new int[]{0+numberOfPoints, 1+numberOfPoints

        };
        vertices.AddRange(lineVerts);
        indices.AddRange(indicesForLines);
        colors.Add(color);
        colors.Add(color);

        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.SetColors(colors);
    }

    // rotate a line and return the position after rotation
    // Assumes rotation around the Z axis
    Vector2 rotate(Vector3 pivotPoint, Vector3 pointToRotate, float angle) {
   		Vector2 result;
   		float Nx = (pointToRotate.x - pivotPoint.x);
   		float Ny = (pointToRotate.y - pivotPoint.y);
   		angle = -angle * Mathf.PI/180f;
   		result = new Vector2(Mathf.Cos(angle) * Nx - Mathf.Sin(angle) * Ny + pivotPoint.x, Mathf.Sin(angle) * Nx + Mathf.Cos(angle) * Ny + pivotPoint.y);
   		return result;
	}
   

	// Draw a circle with the given parameters
	// Should probably use different stuff than the default
    void addCircleToMesh(Mesh mesh, float radiusX, float radiusY, Vector3 center, Color color) {
        int numberOfPoints = vertices.Count;
        float x;
        float y;
        float z = 0f;
		int segments = 15;
        float angle = (360f / segments);

        for (int i = 0; i < (segments + 1); i++) {

            x = Mathf.Sin (Mathf.Deg2Rad * angle) * radiusX + center.x;
            y = Mathf.Cos (Mathf.Deg2Rad * angle) * radiusY + center.y;

            vertices.Add(new Vector3(x, y, 0));
            indices.Add(numberOfPoints + i);
            colors.Add(color);
            angle += (360f / segments);

        }
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.SetColors(colors);
    }
		
	
	// Update is called once per frame
	void Update () {
	
	}
	
}
