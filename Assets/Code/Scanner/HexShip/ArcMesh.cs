using UnityEngine;

namespace Arkio
{
    public static class ArcMesh
    {
        // generates a solid, closed mesh in an arc centered around the positive z axis, offset at the arc radius from the model center
        public static Mesh SolidOffset(float arc, float outsideRadius, float insideRadius, float margin, float thickness, int segments = 10)
        {
            var result = new Mesh();

            int[] tris;
            var verts = CreateSolidArc(arc, outsideRadius, insideRadius, margin, thickness, segments, out tris);

            result.vertices = verts;
            result.triangles = tris;
            
            result.RecalculateBounds();
            result.RecalculateNormals();
            result.UploadMeshData(false);
            return result;
        }
        
        // generates a solid, closed mesh in an arc centered around the positive z axis
        public static Mesh Solid(float arc, float outsideRadius, float insideRadius, float margin, float thickness, int segments = 10)
        {
            var result = new Mesh();

            int[] tris;
            var verts = CreateSolidArc(arc, outsideRadius, insideRadius, margin, thickness, segments, out tris);

            // make positions center relative
            var center = new Vector3(0, 0, insideRadius + ((outsideRadius - insideRadius) / 2));
            for (var i = 0; i < verts.Length; ++i)
            {
                verts[i] -= center;
            }
            
            result.vertices = verts;
            result.triangles = tris;
            
            result.RecalculateBounds();
            result.RecalculateNormals();
            result.UploadMeshData(true);
            return result;
        }

        private static Vector3[] CreateSolidArc(float arc, float outsideRadius, float insideRadius, float margin,
            float thickness, int segments, out int[] tris)
        {
            var numVertsPerFace = (segments + 1) * 2;
            var numVertsForShortSide = 8;
            var numVertsForLongSide = segments * 4;

            var numTrisPerFace = segments * 12;
            var numTrisPerLongSide = segments * 12;
            var numTrisPerShortSide = 6;

            var numIndicies = (numTrisPerFace + numTrisPerShortSide + numTrisPerLongSide) * 2;
            var numVerts = (numVertsPerFace + numVertsForShortSide + numVertsForLongSide) * 2;

            Vector3[] verts = new Vector3[numVerts];
            tris = new int[numIndicies];
            int vertIndex = 0;
            int triIndex = 0;

            CreateSolidFace(ref verts, ref tris, ref vertIndex, ref triIndex, Vector3.zero, true, arc, outsideRadius,
                insideRadius, margin, segments);
            CreateSolidFace(ref verts, ref tris, ref vertIndex, ref triIndex, new Vector3(0.0f, -thickness, 0.0f), false, arc,
                outsideRadius, insideRadius, margin, segments);

            // short sides
            CreateShortSide(ref verts, ref tris, ref vertIndex, ref triIndex, 0, 1, numVertsPerFace);
            CreateShortSide(ref verts, ref tris, ref vertIndex, ref triIndex, numVertsPerFace - 1, numVertsPerFace - 2,
                numVertsPerFace);

            // long sides
            CreateLongSide(ref verts, ref tris, ref vertIndex, ref triIndex, numVertsPerFace - 2, 0, -2, numVertsPerFace);
            CreateLongSide(ref verts, ref tris, ref vertIndex, ref triIndex, 1, numVertsPerFace - 1, 2, numVertsPerFace);
            return verts;
        }

        // relies on two faces having been constructed already in verts
        private static void CreateShortSide(ref Vector3[] verts, ref int[] tris, ref int vertIndex, ref int triIndex,
            int firstVert, int secondVert, int numVertsPerFace)
        {
            var offset = vertIndex;
            verts[vertIndex++] = verts[firstVert];
            verts[vertIndex++] = verts[firstVert + numVertsPerFace];
            verts[vertIndex++] = verts[numVertsPerFace + secondVert];
            verts[vertIndex++] = verts[secondVert];

            tris[triIndex++] = offset;
            tris[triIndex++] = offset + 1;
            tris[triIndex++] = offset + 2;
            tris[triIndex++] = offset;
            tris[triIndex++] = offset + 2;
            tris[triIndex++] = offset + 3;
        }
        
        // relies on two faces having been constructed already in verts
        private static void CreateLongSide(ref Vector3[] verts, ref int[] tris, ref int vertIndex, ref int triIndex,
            int start, int end, int stride, int numVertsPerFace)
        {
            for (int strides = (end - start) / stride; strides > 0; strides -= 1, start += stride)
            {
                var offset = vertIndex;
                verts[vertIndex++] = verts[start];
                verts[vertIndex++] = verts[start + numVertsPerFace];
                verts[vertIndex++] = verts[numVertsPerFace + start + stride];
                verts[vertIndex++] = verts[start + stride];

                tris[triIndex++] = offset;
                tris[triIndex++] = offset + 1;
                tris[triIndex++] = offset + 2;
                tris[triIndex++] = offset;
                tris[triIndex++] = offset + 2;
                tris[triIndex++] = offset + 3;
            }
        }

        private static void CreateSolidFace(ref Vector3[] verts, ref int[] tris, ref int vertIndex, ref int triIndex,
            Vector3 offset, bool top, float arc, float outsideRadius, float insideRadius, float margin, int segments)
        {
            var numVerts = (segments + 1) * 2;          
            
            var startVert = vertIndex + 2;
            var endVert = vertIndex + numVerts - 2;
            
            CreateFaceVertices(ref verts, ref vertIndex, offset, arc, outsideRadius, insideRadius, margin, segments);
            
            for (; startVert < endVert; startVert += 2)
            {
                QuadIndicies(ref tris, ref triIndex, startVert, top);
            }

            // last segment
            QuadIndicies(ref tris, ref triIndex, startVert, top);
        }

        private static void CreateFaceVertices(ref Vector3[] verts, ref int vertIndex, Vector3 offset, float arc, float outsideRadius, float insideRadius,
            float margin, int segments = 10)
        {
            var arcSegment = arc / segments;
            var arcPosition = 0.0f - (arc / 2.0f);
            var numVerts = (segments + 1) * 2;
            
            var halfMargin = margin / 2.0f;
            
            // work out margins for start and end, mirrored so can just flip x
            verts[vertIndex] = new Vector3(insideRadius * Mathf.Sin(arcPosition), 0.0f, insideRadius * Mathf.Cos(arcPosition)) + offset;
            verts[vertIndex + 1] = new Vector3(outsideRadius * Mathf.Sin(arcPosition), 0.0f, outsideRadius * Mathf.Cos(arcPosition)) + offset;
            verts[vertIndex + numVerts - 2] = new Vector3(-verts[vertIndex].x, verts[vertIndex].y, verts[vertIndex].z);
            verts[vertIndex + numVerts - 1] = new Vector3(-verts[vertIndex + 1].x, verts[vertIndex + 1].y, verts[vertIndex + 1].z);

            var dir = Vector3.Cross((verts[vertIndex] - verts[vertIndex + 1]).normalized, Vector3.up);
            
            // start vertices
            verts[vertIndex] += dir * halfMargin;
            verts[vertIndex + 1] += dir * halfMargin;
            
            dir.x = -dir.x;
            
            // end vertices
            verts[vertIndex + numVerts - 2] += dir * halfMargin;
            verts[vertIndex + numVerts - 1] += dir * halfMargin;

            // middle vertices
            arcPosition += arcSegment;
            var startVert = vertIndex +2;
            for (; startVert < vertIndex + numVerts - 2; startVert += 2)
            {
                verts[startVert] = new Vector3(insideRadius * Mathf.Sin(arcPosition), 0.0f, insideRadius * Mathf.Cos(arcPosition))  + offset;
                verts[startVert + 1] = new Vector3(outsideRadius * Mathf.Sin(arcPosition), 0.0f, outsideRadius * Mathf.Cos(arcPosition))  + offset;

                arcPosition += arcSegment;
            }
            
            vertIndex += numVerts;
        }

        private static void QuadIndicies(ref int[] tris, ref int triIndex, int vertIndex, bool top = true)
        {
            if (top)
            {
                tris[triIndex++] = vertIndex - 2;
                tris[triIndex++] = vertIndex - 1;
                tris[triIndex++] = vertIndex;

                tris[triIndex++] = vertIndex;
                tris[triIndex++] = vertIndex - 1;
                tris[triIndex++] = vertIndex + 1;
            }
            else
            {
                tris[triIndex++] = vertIndex;
                tris[triIndex++] = vertIndex - 1;
                tris[triIndex++] = vertIndex - 2;

                tris[triIndex++] = vertIndex + 1;
                tris[triIndex++] = vertIndex - 1;
                tris[triIndex++] = vertIndex;
            }
        }

        // generates a correctly ordered list of points for Unity's Line Renderer
        public static Vector3[] Outline(float arc, float outsideRadius, float insideRadius, float inset, float margin, int segments = 10)
        {
            var numFaceVerts = (segments + 1) * 2; 
            
            // create the path for our outline
            Vector3[] faceVerts = new Vector3[numFaceVerts];
            Vector3[] results = new Vector3[numFaceVerts];
            var vertIdx = 0;
            CreateFaceVertices(ref faceVerts, ref vertIdx, Vector3.zero, arc, outsideRadius, insideRadius, margin, segments);

            // reorder for the line renderer
            var halfNumVerts = numFaceVerts / 2;
            for (int i = 0, j = numFaceVerts - 1, result = 0; i < numFaceVerts; i += 2, j -= 2, ++result)
            {
                faceVerts[i].y = faceVerts[i].z;
                faceVerts[j].y = faceVerts[j].z;
                faceVerts[i].z = faceVerts[j].z = 0.0f;
                results[result] = faceVerts[i];
                results[result + halfNumVerts] = faceVerts[j];
            }

            // inset
            for (uint i = 0; i < numFaceVerts; ++i)
            {
                var lastPos = results[(numFaceVerts + i - 1) % numFaceVerts];
                var nextPos = results[(i + 1) % numFaceVerts];
                var currentPos = results[i];

                var lastToCurrent = (currentPos - lastPos).normalized;
                var currentToNext = (nextPos - currentPos).normalized;
                
                var lastNormal = new Vector3(-lastToCurrent.y, lastToCurrent.x);
                var nextNormal = new Vector3(-currentToNext.y, currentToNext.x);
                var averageNormal = (lastNormal + nextNormal) / 2.0f;

                results[i] += averageNormal.normalized * inset;
            }
            
            return results;
        }
        
        // generates a box collider per segment as a new game object assumed to be at the center of circle described by the arc, correctly sized and oriented to match each segment, offset onto arc
        public static void ArcBoxCollisionOffset(GameObject collisionObject, float arc, float outsideRadius, float insideRadius, float thickness, float colliderHeight, int segments = 10, int layer = 0, bool trigger = false)
        {
            collisionObject.layer = layer;
            
            var arcSegment = arc / segments;
            var width = outsideRadius - insideRadius;          

            var currentSegmentStart = -arc / 2.0f;
            var currentSegmentEnd = currentSegmentStart + arcSegment;

            // length of the outside line of the first segment
            var length = CalculateSegmentLength(outsideRadius, currentSegmentStart, currentSegmentEnd);
            
            for (var i = 0; i < segments; ++i)
            {
                CreateCollisionSegment(collisionObject, outsideRadius, insideRadius, thickness, colliderHeight, layer, width, length, currentSegmentStart, currentSegmentEnd, Vector3.zero, trigger);

                currentSegmentStart += arcSegment;
                currentSegmentEnd += arcSegment;
            }  
        }
        
        // generates a box collider per segment as a new game object assumed to be at the center of circle described by the arc, correctly sized and oriented to match each segment
        public static void ArcBoxCollision(GameObject collisionObject, float arc, float outsideRadius, float insideRadius, float thickness, float colliderHeight, int segments = 10, int layer = 0, bool trigger = false)
        {
            collisionObject.layer = layer;
            
            var arcSegment = arc / segments;
            var width = outsideRadius - insideRadius;          

            var currentSegmentStart = -arc / 2.0f;
            var currentSegmentEnd = currentSegmentStart + arcSegment;

            // length of the outside line of the first segment
            var length = CalculateSegmentLength(outsideRadius, currentSegmentStart, currentSegmentEnd);
            
            var arcCenter = new Vector3(0, 0, insideRadius + ((outsideRadius - insideRadius) / 2));
            
            for (var i = 0; i < segments; ++i)
            {  
                CreateCollisionSegment(collisionObject, outsideRadius, insideRadius, thickness, colliderHeight, layer, width, length, currentSegmentStart, currentSegmentEnd, arcCenter, trigger);

                currentSegmentStart += arcSegment;
                currentSegmentEnd += arcSegment;
            }
           
        }

        private static float CalculateSegmentLength(float outsideRadius, float currentSegmentStart, float currentSegmentEnd)
        {
            return (new Vector3(outsideRadius * Mathf.Sin(currentSegmentEnd), 0,
                        outsideRadius * Mathf.Cos(currentSegmentEnd)) - new Vector3(outsideRadius * Mathf.Sin(currentSegmentStart), 0,
                        outsideRadius * Mathf.Cos(currentSegmentStart))).magnitude;
        }

        private static void CreateCollisionSegment( GameObject collisionObject, 
                                                    float outsideRadius, 
                                                    float insideRadius,
                                                    float thickness, 
                                                    float colliderHeight, 
                                                    int layer, 
                                                    float width, 
                                                    float length, 
                                                    float currentSegmentStart,
                                                    float currentSegmentEnd,
                                                    Vector3 positionOffset,
                                                    bool trigger
                                                    )
        {
            var segmentObject = new GameObject("Collision");
            var segmentTransform = segmentObject.GetComponent<Transform>();
            var boxCollider = segmentObject.AddComponent<BoxCollider>();

            segmentObject.layer = layer;

            var boxSize = boxCollider.size;
            boxSize.z = width;
            boxSize.x = length;
            boxSize.y = colliderHeight;
            boxCollider.size = boxSize;
            boxCollider.isTrigger = trigger;

            segmentTransform.SetParent(collisionObject.GetComponent<Transform>(), false);
            var localPosition = segmentTransform.localPosition;
            localPosition.y = (colliderHeight / 2.0f) - thickness;

            var outsideLeftPos = new Vector3(outsideRadius * Mathf.Sin(currentSegmentStart), 0,
                                     outsideRadius * Mathf.Cos(currentSegmentStart)) - positionOffset;
            var outsideRightPos = new Vector3(outsideRadius * Mathf.Sin(currentSegmentEnd), 0,
                                      outsideRadius * Mathf.Cos(currentSegmentEnd)) - positionOffset;
            var insideLeftPos = new Vector3(insideRadius * Mathf.Sin(currentSegmentStart), 0,
                                    insideRadius * Mathf.Cos(currentSegmentStart)) - positionOffset;
            var insideRightPos = new Vector3(insideRadius * Mathf.Sin(currentSegmentEnd), 0,
                                     insideRadius * Mathf.Cos(currentSegmentEnd)) - positionOffset;

            var center = (outsideLeftPos + outsideRightPos + insideLeftPos + insideRightPos) / 4.0f;

            localPosition.x = center.x;
            localPosition.z = center.z;

            segmentTransform.localPosition = localPosition;
            var currentSegmentCenter = currentSegmentStart + ((currentSegmentEnd - currentSegmentStart) / 2);
            segmentTransform.localRotation = Quaternion.Euler(0, currentSegmentCenter * Mathf.Rad2Deg, 0);
        }
    }
}

