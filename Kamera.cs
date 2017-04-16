using UnityEngine;
using System.Collections;
using System;
using Cekirdekler;
using Cekirdekler.ClArrays;
using System.IO;
using System.Text;

public class Kamera : MonoBehaviour
{


    public static class UnitySystemConsoleRedirector
    {
        private class UnityTextWriter : TextWriter
        {
            private StringBuilder buffer = new StringBuilder();

            public override void Flush()
            {
                Debug.Log(buffer.ToString());
                buffer.Length = 0;
            }

            public override void Write(string value)
            {
                buffer.Append(value);
                if (value != null)
                {
                    var len = value.Length;
                    if (len > 0)
                    {
                        var lastChar = value[len - 1];
                        if (lastChar == '\n')
                        {
                            Flush();
                        }
                    }
                }
            }

            public override void Write(char value)
            {
                buffer.Append(value);
                if (value == '\n')
                {
                    Flush();
                }
            }

            public override void Write(char[] value, int index, int count)
            {
                Write(new string(value, index, count));
            }

            public override Encoding Encoding
            {
                get { return Encoding.Default; }
            }
        }

        public static void Redirect()
        {
            Console.SetOut(new UnityTextWriter());
        }
    }
    public Vector3[] vertices=null;
    public Vector3[] verticesBase=null;
    public Vector3[] normals=null;
    public Mesh mesh = null;
    public GameObject copy = null;

    void Start()
    {

        UnitySystemConsoleRedirector.Redirect();GameObject.CreatePrimitive(PrimitiveType.Sphere);
        copy = (GameObject)Instantiate(GameObject.Find("RollerBall"),new Vector3(GameObject.Find("RollerBall").transform.position.x, GameObject.Find("RollerBall").transform.position.y, GameObject.Find("RollerBall").transform.position.z),new Quaternion());
        mesh = createSphere(copy.GetComponent<MeshFilter>().mesh);
        //mesh = copy.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        verticesBase = mesh.vertices;
        normals = mesh.normals;
        
    }

    Mesh createSphere(Mesh mesh_)
    {
        Mesh mesh = mesh_;
        mesh.Clear();

        float radius = 0.3f;
        // Longitude |||
        int nbLong = 224;
        // Latitude ---
        int nbLat = 256;

        #region Vertices
        Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radius;
        for (int lat = 0; lat < nbLat; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= nbLong; lon++)
            {
                float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radius;
        #endregion

        #region Normales		
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normales[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < nbLat; lat++)
            for (int lon = 0; lon <= nbLong; lon++)
                uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < nbLat - 1; lat++)
        {
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        //Bottom Cap
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = vertices.Length - 1;
            triangles[i++] = vertices.Length - (lon + 2) - 1;
            triangles[i++] = vertices.Length - (lon + 1) - 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();
        return mesh;
    }

    private float t = 0;
    private float ctr = 0;
    private ClNumberCruncher numberCruncher = null;
    private ClArray<byte> xyzGPU = null;

    private ClArray<byte> xyznGPU = null;
   
    private ClArray<byte> xyzoGPU = null;

    private ClArray<float> arguments = null;
    void Update()
    {
        if (ctr < 0.3)
            ctr += 0.001f;
        t += 0.001f;
        if (vertices != null)
        {
            float x = verticesBase[0].x;
            float y = verticesBase[0].y;

            bool strategy = false;

            if (strategy)
            {
                // CPU start
                for (int i = 0; i < vertices.Length; i++)
                {
                    float dx = verticesBase[i].x - x;
                    float dy = verticesBase[i].y - y;
                    vertices[i] = verticesBase[i] + 0.02f * normals[i] * ctr * (float)Math.Sin(40.0f * t + 100.0f * Math.Sqrt(dx * dx + dy * dy));
                }
                // CPU end
            }
            else
            {
                // GPGPU start
                int nGPU = 224 * 256; // number of vertices aligned to multiple of 64

                // init number cruncher start
                if (numberCruncher == null)
                {
                    Cekirdekler.Hardware.ClPlatforms platforms = Cekirdekler.Hardware.ClPlatforms.all();
                    var devices = platforms.devicesAmd(true, true); // faster when not screen recording
                    platforms.logInfo();
                    devices.logInfo();
                    numberCruncher = new ClNumberCruncher(devices, @"
                        __kernel void waveEquation( __global float *xyz,__global float *xyzn,__global float *xyzo,__global float * arguments)
                        {
                            int threadId=get_global_id(0);
                            if(threadId<arguments[4])
                            {
                                float dx=xyz[threadId*3]-arguments[2];float dy=xyz[threadId*3+1]-arguments[3];float t=arguments[1];
                                float ctr=arguments[0];float wave=0.02f*ctr*sin(40.0f*t+100.0f*sqrt(dx*dx+dy*dy));
                                xyzo[threadId*3]=xyz[threadId*3]+xyzn[threadId*3]*wave; // wave equation for all surface vertices
                                xyzo[threadId*3+1]=xyz[threadId*3+1]+xyzn[threadId*3+1]*wave; // wave equation for all surface vertices
                                xyzo[threadId*3+2]=xyz[threadId*3+2]+xyzn[threadId*3+2]*wave; // wave equation for all surface vertices
                            }
                        }
                    ", true);
                }
                // init number cruncher end

                // init arrays start to optimize read/writes 
                if (xyzGPU == null)
                {
                    xyzGPU = ClArray<byte>.wrapArrayOfStructs(verticesBase); xyznGPU = ClArray<byte>.wrapArrayOfStructs(normals);
                    xyzoGPU = ClArray<byte>.wrapArrayOfStructs(vertices); arguments = new float[64];
                    arguments.write = false; arguments.partialRead = false;
                }
                // init arrays end

                // wave parameters for all vertices
                arguments[0] = ctr;
                arguments[1] = t;
                arguments[2] = x;
                arguments[3] = y;
                arguments[4] = nGPU;

                // compute start CPU+GPU 3x as fast
                xyzGPU.nextParam(xyznGPU, xyzoGPU, arguments).
                    compute(numberCruncher, 1, "waveEquation", nGPU, 64);
                // compute end

                xyznGPU.read = false;
                xyzGPU.read = false;
                // GPGPU end
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals(); // just for reflections
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    void OnDestroy()
    {

    }

 



}

