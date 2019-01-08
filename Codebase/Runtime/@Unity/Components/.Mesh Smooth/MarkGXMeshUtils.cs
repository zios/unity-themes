using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
	Useful mesh functions
*/
public class MeshUtils : MonoBehaviour
{
	// Finds a set of adjacent vertices for a given vertex
	// Note the success of this routine expects only the set of neighboring faces to eacn contain one vertex corresponding
	// to the vertex in question
	public static List<Vector3> findAdjacentNeighbors ( Vector3[] v, int[] t, Vector3 vertex )
	{
		List<Vector3>adjacentV = new List<Vector3>();
		List<int>facemarker = new List<int>();
		int facecount = 0;

		// Find matching vertices
		for (int i=0; i<v.Length; i++)
			if (Mathf.Approximately (vertex.x, v[i].x) &&
				Mathf.Approximately (vertex.y, v[i].y) &&
				Mathf.Approximately (vertex.z, v[i].z))
			{
					int v1 = 0;
					int v2 = 0;
					bool marker = false;

					// Find vertex indices from the triangle array
					for(int k=0; k<t.Length; k=k+3)
						if(facemarker.Contains(k) == false)
						{
							v1 = 0;
							v2 = 0;
							marker = false;

							if(i == t[k])
							{
								v1 = t[k+1];
								v2 = t[k+2];
								marker = true;
							}

							if(i == t[k+1])
							{
								v1 = t[k];
								v2 = t[k+2];
								marker = true;
							}

							if(i == t[k+2])
							{
								v1 = t[k];
								v2 = t[k+1];
								marker = true;
							}

							facecount++;
							if(marker)
							{
								// Once face has been used mark it so it does not get used again
								facemarker.Add(k);

								// Add non duplicate vertices to the list
								if ( isVertexExist(adjacentV, v[v1]) == false )
								{
									adjacentV.Add(v[v1]);
									//Debug.Log("Adjacent vertex index = " + v1);
								}

								if ( isVertexExist(adjacentV, v[v2]) == false )
								{
									adjacentV.Add(v[v2]);
									//Debug.Log("Adjacent vertex index = " + v2);
								}
								marker = false;
							}
						}
			}

		//Debug.Log("Faces Found = " + facecount);

		return adjacentV;
	}


	// Finds a set of adjacent vertices indexes for a given vertex
	// Note the success of this routine expects only the set of neighboring faces to eacn contain one vertex corresponding
	// to the vertex in question
	public static List<int> findAdjacentNeighborIndexes ( Vector3[] v, int[] t, Vector3 vertex )
	{
		List<int>adjacentIndexes = new List<int>();
		List<Vector3>adjacentV = new List<Vector3>();
		List<int>facemarker = new List<int>();
		int facecount = 0;

		// Find matching vertices
		for (int i=0; i<v.Length; i++)
			if (Mathf.Approximately (vertex.x, v[i].x) &&
				Mathf.Approximately (vertex.y, v[i].y) &&
				Mathf.Approximately (vertex.z, v[i].z))
			{
					int v1 = 0;
					int v2 = 0;
					bool marker = false;

					// Find vertex indices from the triangle array
					for(int k=0; k<t.Length; k=k+3)
						if(facemarker.Contains(k) == false)
						{
							v1 = 0;
							v2 = 0;
							marker = false;

							if(i == t[k])
							{
								v1 = t[k+1];
								v2 = t[k+2];
								marker = true;
							}

							if(i == t[k+1])
							{
								v1 = t[k];
								v2 = t[k+2];
								marker = true;
							}

							if(i == t[k+2])
							{
								v1 = t[k];
								v2 = t[k+1];
								marker = true;
							}

							facecount++;
							if(marker)
							{
								// Once face has been used mark it so it does not get used again
								facemarker.Add(k);

								// Add non duplicate vertices to the list
								if ( isVertexExist(adjacentV, v[v1]) == false )
								{
									adjacentV.Add(v[v1]);
									adjacentIndexes.Add(v1);
									//Debug.Log("Adjacent vertex index = " + v1);
								}

								if ( isVertexExist(adjacentV, v[v2]) == false )
								{
									adjacentV.Add(v[v2]);
									adjacentIndexes.Add(v2);
									//Debug.Log("Adjacent vertex index = " + v2);
								}
								marker = false;
							}
						}
			}

		//Debug.Log("Faces Found = " + facecount);

		return adjacentIndexes;
	}

	// Does the vertex v exist in the list of vertices
	static bool isVertexExist(List<Vector3>adjacentV, Vector3 v)
	{
		bool marker = false;
		foreach (Vector3 vec in adjacentV)
		  if (Mathf.Approximately(vec.x,v.x) && Mathf.Approximately(vec.y,v.y) && Mathf.Approximately(vec.z,v.z))
		  {
			  marker = true;
			  break;
		  }

		return marker;
	}
}