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
	StringBuilder start = new StringBuilder("");
	StringBuilder lang = new StringBuilder("");
	GameObject contents;
	float angleToUse = 45f;
	public int iterations = 4;
	
	// for drawing lines
	public float lineWidth = 1.0f;
    List<Color> colors = new List<Color>(100);
    Mesh lineMesh;
    List<Vector3> vertices = new List<Vector3>(100);
    List<int> indices = new List<int>(100);
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
        //start = new StringBuilder("0");
        //ruleHash.Add("1", "11");
        //ruleHash.Add("0", "1[0]0");
        //angleToUse = 45f;
        //run(iterations);
        //watch.Stop();
        //UnityEngine.Debug.Log("Time for generation took: " + watch.ElapsedMilliseconds);
        //print(lang);
        //watch.Reset();
        //watch.Start();
        //display2();



        // Weed type plant example from: 
        // http://en.wikipedia.org/wiki/L-system
        start = new StringBuilder("X");
        ruleHash.Add("X", "F-[[X]+X]+F[+FX]-X");
        ruleHash.Add("F", "FF");
        angleToUse = 25f;
        run(iterations);
        watch.Stop();
        UnityEngine.Debug.Log("Time for generation took: " + watch.ElapsedMilliseconds);
        print(lang);
        watch.Reset();
        watch.Start();
        display3();


        watch.Stop();
        UnityEngine.Debug.Log("Time for display took: " + watch.ElapsedMilliseconds);
        UnityEngine.Debug.Log("Count of vertices in list: " + vertices.Count);

    }

    // Get a rule from a given letter that's in our array
    string getRule( string input) {		
		if (ruleHash.ContainsKey(input))
        {
            return (string)ruleHash[input];

        }
		return input;
	}
	
	// Run the lsystem iterations number of times on the start axiom.
	void run(int iterations) {
    	StringBuilder curr = start;
		
    	for (int i = 0; i < iterations; i++) {
        	for (int j = 0; j < curr.Length; j++) {
            	string buff = getRule(curr[j].ToString() );
                curr = curr.Replace(curr[j].ToString(), buff, j, 1);
                j += buff.Length - 1;
        	}
    	}

    	lang = curr;
	}


    // The display routine for the weed type plant above
    void display3() {
		
		// to push and pop location and angles
		Stack<float> positions = new Stack<float>(10);
		Stack<float> angles = new Stack<float>(10);

		// current location and angle
		float angle = 0f;
		Vector3 position = new Vector3(0,0,0);
		float posy = 0.0f;
		float posx = 0.0f;

        // location and rotation to draw towards
		Vector3 newPosition;
		Vector2 rotated;
        
        // start at 0,0,0
        // Apply all the drawing rules to the lsystem string
        for (int i=0; i<lang.Length; i++) {
			string buff = lang[i].ToString();
			switch (buff) {
			case "-" : 
				// Turn left 25
				angle -= angleToUse;
				break;
			case "+" : 
				// Turn right 25
				angle += angleToUse;
				break;
			case "F" : 
				// draw a line 
				posy += initial_length;
				newPosition = new Vector3(position.x, posy, 0);
				rotated = rotate (position, new Vector3(position.x,posy,0), angle);
				newPosition = new Vector3(rotated.x,rotated.y,0);
                addLineToMesh(lineMesh, position, newPosition, Color.green);
                // set up for the next draw
                position = newPosition;
				posx = newPosition.x;
				posy = newPosition.y;    
                break;
			case "[" :
				//[: push position and angle
				positions.Push (posy);
				positions.Push (posx);
				float currentAngle = angle;
				angles.Push(currentAngle);
				break;
			case "]" : 
				//]: pop position and angle
				posx = positions.Pop();
				posy = positions.Pop();
				position = new Vector3(posx, posy, 0);
				angle = angles.Pop();
                break;
			default : break;
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
        for (int i = 0; i < lang.Length; i++)
        {
            string buff = lang[i].ToString();
            switch (buff)
            {
                case "0":
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
                case "1":
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
                case "[":
                    //[: push position and angle, turn left 45 degrees
                    positions.Push(posy);
                    positions.Push(posx);
                    float currentAngle = angle;
                    angles.Push(currentAngle);
                    angle -= 45;
                    break;
                case "]":
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
