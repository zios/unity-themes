using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SmoothFilter : MonoBehaviour
{
	/*
		Standard Laplacian Smooth Filter
	*/
	public static Vector3[] laplacianFilter(Vector3[] sv, int[] t)
	{
		Vector3[] wv = new Vector3[sv.Length];
		List<Vector3> adjacentVertices = new List<Vector3>();

		float dx = 0.0f;
		float dy = 0.0f;
		float dz = 0.0f;

		for (int vi=0; vi< sv.Length; vi++)
		{
			// Find the sv neighboring vertices
			adjacentVertices = MeshUtils.findAdjacentNeighbors (sv, t, sv[vi]);

			if (adjacentVertices.Count != 0)
			{
				dx = 0.0f;
				dy = 0.0f;
				dz = 0.0f;

				//Debug.Log("Vertex Index Length = "+vertexIndexes.Length);
				// Add the vertices and divide by the number of vertices
				for (int j=0; j<adjacentVertices.Count; j++)
				{
					dx += adjacentVertices[j].x;
					dy += adjacentVertices[j].y;
					dz += adjacentVertices[j].z;
				}

				wv[vi].x = dx / adjacentVertices.Count;
				wv[vi].y = dy / adjacentVertices.Count;
				wv[vi].z = dz / adjacentVertices.Count;
			}
		}

		return wv;
	}

	/*
		HC (Humphrey’s Classes) Smooth Algorithm - Reduces Shrinkage of Laplacian Smoother

		Where sv - original points
				pv - previous points,
				alpha [0..1] influences previous points pv, e.g. 0
				beta  [0..1] e.g. > 0.5
	*/
	public static Vector3[] hcFilter(Vector3[] sv, Vector3[] pv, int[] t, float alpha, float beta)
	{
		Vector3[] wv = new Vector3[sv.Length];
		Vector3[] bv = new Vector3[sv.Length];



		// Perform Laplacian Smooth
		wv = laplacianFilter(sv, t);

		// Compute Differences
		for(int i=0; i<wv.Length; i++)
		{
			bv[i].x = wv[i].x - (alpha * sv[i].x + ( 1 - alpha ) * sv[i].x );
			bv[i].y = wv[i].y - (alpha * sv[i].y + ( 1 - alpha ) * sv[i].y );
			bv[i].z = wv[i].z - (alpha * sv[i].z + ( 1 - alpha ) * sv[i].z );
		}

		List<int> adjacentIndexes = new List<int>();

		float dx = 0.0f;
		float dy = 0.0f;
		float dz = 0.0f;

		for(int j=0; j<bv.Length; j++)
		{
			adjacentIndexes.Clear();

			// Find the bv neighboring vertices
			adjacentIndexes = MeshUtils.findAdjacentNeighborIndexes (sv, t, sv[j]);

			dx = 0.0f;
			dy = 0.0f;
			dz = 0.0f;

			for (int k=0; k<adjacentIndexes.Count; k++)
			{
				dx += bv[adjacentIndexes[k]].x;
				dy += bv[adjacentIndexes[k]].y;
				dz += bv[adjacentIndexes[k]].z;

			}

			wv[j].x -= beta * bv[j].x + ((1 - beta) / adjacentIndexes.Count) * dx;
			wv[j].y -= beta * bv[j].y + ((1 - beta) / adjacentIndexes.Count) * dy;
			wv[j].z -= beta * bv[j].z + ((1 - beta) / adjacentIndexes.Count) * dz;
		}

		return wv;
	}
}