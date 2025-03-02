using System;
using System.Collections.Generic;
using UnityEngine;

//Temporary class to store the Voronoi points
public class VPoints{
    public float x;
    public float y;
    public float fValue;
    public VPoints(float x, float y, float fValue){
        this.x = x;
        this.y = y;
        this.fValue = fValue;
    }
};

// Class to store each hex tile attributes
[System.Serializable]
public class TileAttribute : PropertyAttribute{
    public int iType;
    private float X;
    private float Y;
    public float GetX(){
        return X;
    }
    public float GetY(){
        return Y;
    }
    public TileAttribute(int type, float x, float y){
        iType = type;
        X = x;
        Y = y;
    }
}

// Main class to generate and display the map
public class map : MonoBehaviour{
    // Enumerations for the map type and size
        public enum MapType
    {
        Continents,
        Pangea,
        Archipelago,
        // Seven_Seas,
        Small_Continents,
        Continents_and_Islands,
        Lakes
    }
    public enum MapSize
    {
        Duel,
        Tiny,
        Small,
        Standard,
        Large
    }
    // Variables to store the map and map generator values
    private int mapWidth = 44;
    private int mapHeight = 26;
    [Header("Map Settings")]
    public bool randomSeed = false;
    public char[] seed = new char [8];
    public float frequency = 0.5f;
    public float amplitude = 1.0f;
    public int octaves = 4;
    public float lacunarity = 2.0f;
    public float presistence = 0.5f;
    public float ping_pong_scale = 0.5f;
    public MapSize mapSize;
    public MapType mapType;
    private int iMapSize = 0;

    [Header("Map Data")]
    public GameObject[] hexPre = new GameObject[8];
    public Mesh hexMesh = null;
    public Material hexMat = null;
    public  TileAttribute[] mapData = null;
    
    private MaterialPropertyBlock mpb = null;
    private List<Matrix4x4>[] matrix_array = new List<Matrix4x4>[7];
    private int matrixSize = 0;
    private float MAX_X_COORD = 0;
    private float MAX_Y_COORD = 0;
    private Vector4[] colors = new Vector4[6];
    private VPoints []vorPoints;
    ///////////////////////////////////////////////////
    // Scripts to display the map to screen (in progress)
    void DrawBoard(){
        for (int i = 0; i < matrixSize + 1; i++){
            Graphics.DrawMeshInstanced(hexMesh, 0, hexMat, matrix_array[i].ToArray(), matrix_array[i].Count ,mpb);
        }
    }

    void DrawBoard_v2(){
        for(int i=0; i<iMapSize; i++){
            Vector3 position = new Vector3(mapData[i].GetX(), 0, mapData[i].GetY());
            Instantiate(hexPre[mapData[i].iType], position, Quaternion.identity);
        }
    }
    ///////////////////////////////////////////////////
    // Scripts to generate Perlin Value

    // Basic Perlin Noise
    float PerlinValue(float x, float y, float frequency = 1.0f, float amplitude = 1.0f){
        float value = 0;
        int x_seed = 0;
        x_seed |= ((int)(seed[0] & 0xFF)) << 8;
        x_seed |= (int)(seed[1] & 0xFF);
        int y_seed = 0;
        y_seed |= ((int)(seed[2] & 0xFF)) << 8;
        y_seed |= (int)(seed[3] & 0xFF);
        value = Mathf.PerlinNoise((float)(x + x_seed)*frequency, (float)(y + y_seed)*frequency)*amplitude;
        return value;
    }
    
    // Perlin Noise with Octaves
    float PerlinValue(float x, float y, float frequency = 1.0f, float amplitude = 1.0f, int octaves = 4, float lacunarity = 2.0f, float presistence = 0.5f){
        float fValue = 0;
        float fMaxValue = 0.0f;
        for(int i=0; i<octaves; i++){
            fValue += PerlinValue(x, y, frequency, amplitude);
            fMaxValue += amplitude;
            amplitude *= presistence;
            frequency *= lacunarity;            
        }

        return fValue/fMaxValue;
    }
    
    // Perlin Noise with Ping Pong (Potencial rivers in the future)
    float PingPong(float value, float scale){
        return Mathf.Abs(Mathf.Sin(value * scale));
    }
    float PingPongPerlinValalue(float x, float y, float frequency, float amplitude = 2.29f, float ping_pong_scale = 3.16f){
        float fValue = PerlinValue(x, y, frequency, amplitude);
        fValue = PingPong(fValue, ping_pong_scale);
        return fValue;
    }
    ///////////////////////////////////////////////////
    // Main script to generate the map with soome variations (in progress)
    // 1 - Water, 2 - Land
    private int MapGenerator(float x, float y){
        // Not working yet
        if(mapType == MapType.Continents){
            // frequency = 11.32f;
            // amplitude = 0.9f;
            float fPerlinV = PerlinValue(x/mapWidth, y/mapHeight, frequency, amplitude, octaves, lacunarity, presistence);
            float vor = VoronoiDiagram(x, y);
            
            // float combValue = 0.6f*vor + 0.4f*fPerlinV;
            float combValue = Mathf.Lerp(vor, fPerlinV, 0.6f);
            Mathf.SmoothStep(0.0f, 1.0f, fPerlinV);

            if(vor < 0.5f){
                return 1;
            }
            else{
                return 2;
            }
        }
        // Not working yet
        else if(mapType == MapType.Pangea){
            frequency = 5.96f;
            amplitude = 1.54f;
            float fPerlinV = PerlinValue(x/mapWidth, y/mapHeight, frequency, amplitude, octaves, lacunarity, presistence);
            float vor = VoronoiDiagram(x, y);
            float combValue = Mathf.Lerp(vor, fPerlinV, 0.425f);
            if(combValue < 0.5){
                return 1;
            }
            else{
                return 2;
            }
        }
        else if(mapType == MapType.Archipelago){
            if(mapSize == MapSize.Duel || mapSize == MapSize.Tiny){
                frequency = 13.2f;
                amplitude = 1.19f;
            }
            else if(mapSize == MapSize.Small || mapSize == MapSize.Standard){
                frequency = 21.1f;
                amplitude = 1.19f;
            }
            else{
                frequency = 23.3f;
                amplitude = 1.19f;
            }
            float fPerlinV = PerlinValue(x/mapWidth, y/mapHeight, frequency, amplitude, octaves, lacunarity, presistence);
            float fPerlinV2 = PerlinValue(x/mapWidth, y/mapHeight, frequency*1.5f, amplitude, 2, lacunarity, presistence);
            if(fPerlinV < 0.55f){
                // fPerlinV2 = Mathf.Lerp(fPerlinV, fPerlinV2, 0.5f);
                if(fPerlinV2 < 0.65f){
                    return 1;
                }
                else{
                    return 2;
                }
                // return 1;
            }
            else{
                return 2;
            }
        }
        else if(mapType == MapType.Lakes){
            if(mapSize == MapSize.Duel || mapSize == MapSize.Tiny){
                frequency = 9.82f;
            }
            else{
                frequency = 13.32f;
            }
            amplitude = 1.54f;
            float fPerlinV = PerlinValue(x/mapWidth, y/mapHeight, frequency, amplitude, octaves, lacunarity, presistence);
            if(fPerlinV < 0.525){
                fPerlinV = PerlinValue(x/mapWidth, y/mapHeight, frequency*1.5f, amplitude, 2, lacunarity, presistence);
                if(fPerlinV < 0.7f){
                    return 2;
                }
                else{
                    return 1;
                }
            }
            else{
                return 1;
            }
        }
        // Temporary option
        else{
            float fPerlinV = PerlinValue(x/mapWidth, y/mapHeight, frequency, amplitude, octaves, lacunarity, presistence);
            if(fPerlinV < 0.55f){
                return 1;
            }
            else{
                return 2;
            }
        }
    }
    ////////////////////////////////////////////////////
    // Scripts to generate Voronoi Diagram

    // Function to calculate the distance between a point and a line
    private double PointDistanceFromLine(float x1, float y1, float x2, float y2, float xp, float yp){
        double A = 0, B = 0, C = 0;
        double distance = 0;
        A = y2 - y1;
        B = x1 - x2;
        C = (x2 * y1) - (x1 * y2);
        distance = Math.Abs(A * xp + B * yp + C) / Math.Sqrt(A * A + B * B);
        return distance;

    }
    
    // Function to find the closest point to the Voronoi diagram and returns the value of the Voronoi point (between 0 and 1)
    private float VoronoiDiagram(float x, float y){
        int index = 0;
        float distance = 0, dw = 0;
        for(int i=0; i<mapWidth; i++){
            dw = Mathf.Sqrt((x - vorPoints[i].x)*(x - vorPoints[i].x) + (y - vorPoints[i].y)*(y - vorPoints[i].y));
            if(dw < distance || i == 0){
                distance = dw;
                index = i;
            }
        }
        return vorPoints[index].fValue;
    }

    // Generator of the Voronoi points with configurations (in progress)
    private void GenVoronoiValues(){
        UnityEngine.Random.InitState(System.Environment.TickCount);
        vorPoints = new VPoints[mapWidth];

        // convert seed to int
        float randNum = 0, x_random = 0, y_random = 0;
        int iSeed = 0;
        iSeed |= ((int)(seed[0] & 0xFF)) << 24;
        iSeed |= ((int)(seed[1] & 0xFF)) << 16;
        iSeed |= ((int)(seed[2] & 0xFF)) << 8;
        iSeed |= (int)(seed[3] & 0xFF);
        System.Random randObj = new System.Random(iSeed);

        // Voronoi points by map type
        if(mapType == MapType.Continents){
            float []LinePoint_x;
            float []LinePoint_y;
            // float zoneXSize = 0, zoneYSize = 0;
            int iPointsSize = 0;
            if(mapSize == MapSize.Duel || mapSize == MapSize.Tiny){
                // zoneXSize = 0.3f * MAX_X_COORD;
                // zoneYSize = 0.4f * MAX_Y_COORD;
                iPointsSize = 2;
                LinePoint_x = new float [iPointsSize];
                LinePoint_y = new float [iPointsSize];
                
                // LinePoint_x[0] = (float)(randObj.NextDouble() * MAX_X_COORD*0.3f + 0.35f*MAX_X_COORD);
                // LinePoint_y[0] = (float)(randObj.NextDouble() * MAX_Y_COORD*0.1f);
                // LinePoint_x[1] = (float)(randObj.NextDouble() * MAX_X_COORD*0.3f + 0.35f*MAX_X_COORD);
                // LinePoint_y[1] = (float)(randObj.NextDouble() * MAX_Y_COORD*0.1f + 0.9f*MAX_Y_COORD);
                
                Debug.Log($"LibePoint 1: {LinePoint_x[0]}, {LinePoint_y[0]}");
                Debug.Log($"LibePoint 2: {LinePoint_x[1]}, {LinePoint_y[1]}");

                for(int i=0; i < mapWidth; i++){
                    x_random = (float)(randObj.NextDouble() * MAX_X_COORD);
                    y_random = (float)(randObj.NextDouble() * MAX_Y_COORD);
                        // if((x_random > zone_x[0] && x_random < zone_x[0] + zoneXSize && y_random > zone_y[0] && y_random < zone_y[0] + zoneYSize) || x_random>zone_x[1] && x_random < zone_x[1] + zoneXSize && y_random > zone_y[1] && y_random < zone_y[1] + zoneYSize){
                    randNum = PerlinValue(x_random/MAX_X_COORD, y_random/MAX_Y_COORD, 3.73f, 1.46f, 3, 2.0f, 0.75f);
                        // if(x_random > 0.1*MAX_X_COORD && x_random < 0.9*MAX_X_COORD && y_random > 0.1*MAX_Y_COORD && y_random < 0.9*MAX_Y_COORD){
                        //     if(PointDistanceFromLine(LinePoint_x[0], LinePoint_y[0], LinePoint_x[1], LinePoint_y[1], x_random, y_random) > mapHeight/10*1.5){//2.5
                        //         // randNum = (randObj.Next(0, 3000)/6000f) + 0.45f;
                        //     }
                        //     else{
                        //         randNum = (randObj.Next(0, 3000)/12000f) +0.01f;
                        //     }
                        //     // randNum = (randObj.Next(0, 2137)/5342.5f) + 0.5f;
                        // }
                        // else{
                        //     // randNum = randObj.Next(0, 2137)/5342.5f + 0.1f;
                        //     randNum = (randObj.Next(0, 3000)/7500f) + 0.1f;
                        // }
                    vorPoints[i] = new VPoints(x_random, y_random, randNum);
                }
            }
            else if(mapSize == MapSize.Small || mapSize == MapSize.Standard){
                // iZoneSize = 3;
                // zone_x = new float [iZoneSize];
                // zone_y = new float [iZoneSize];
            }
            else{
                // iZoneSize = 4;
                // zone_x = new float [iZoneSize];
                // zone_y = new float [iZoneSize];
            }
        }
        else if(mapType == MapType.Pangea){
            for(int i=0; i<mapWidth; i++){
                x_random = (float)(randObj.NextDouble() * MAX_X_COORD);
                y_random = (float)(randObj.NextDouble() * MAX_Y_COORD);
                if(x_random > 0.2*MAX_X_COORD && x_random < 0.8*MAX_X_COORD && y_random > 0.1*MAX_Y_COORD && y_random < 0.9*MAX_Y_COORD){
                    randNum = (randObj.Next(0, 3000)/12000f) + 0.45f;
                }
                else{
                    randNum = randObj.Next(0, 3000)/12000f+0.1f;
                }
                vorPoints[i] = new VPoints(x_random, y_random, randNum);
            }
        }
    }
    ///////////////////////////////////////////////////
    // Scripts to set the map size, colours and seed
    private void SetMapSize(){
        if(mapSize == MapSize.Duel){
            mapWidth = 44;
            mapHeight = 26;
            iMapSize = 44*26;
        }
        else if(mapSize == MapSize.Tiny){
            mapWidth = 60;
            mapHeight = 38;
            iMapSize = 60*38;
        }
        else if(mapSize == MapSize.Small){
            mapWidth = 74;
            mapHeight = 46;
            iMapSize = 74*46;
        }
        else if(mapSize == MapSize.Standard){
            mapWidth = 84;
            mapHeight = 54;
            iMapSize = 84*54;
        }
        else if(mapSize == MapSize.Large){
            mapWidth = 96;
            mapHeight = 60;
            iMapSize = 96*60;
        }
        MAX_X_COORD = mapWidth * 0.5f + 0.25f;
        MAX_Y_COORD = mapHeight * Mathf.Sqrt((0.5f * 0.5f) - (0.25f * 0.25f));
        Debug.Log($"MAX_X_COORD: {MAX_X_COORD}, MAX_Y_COORD: {MAX_Y_COORD}");
    }
    private void SetColours(){
        colors[0] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        colors[1] = new Vector4(0.3647f, 0.4392f, 0.8549f, 1.0f);
        colors[2] = new Vector4(0.6667f, 0.8863f, 0.4039f, 1.0f);
        colors[3] = new Vector4(0.3216f, 0.4078f, 0.2118f, 1.0f);
        colors[4] = new Vector4(0.9804f, 0.9490f, 0.3725f, 1.0f);
        colors[5] = new Vector4(0.4118f, 0.4157f, 0.4157f, 1.0f);
    }
    private void SetSeed(){
        if(randomSeed){
            for(int i=0; i<8; i++){
                seed[i] = (char)UnityEngine.Random.Range(0, 255);
            }
        }
    }
    ///////////////////////////////////////////////////
    void Start(){
        // Set the map size, seed and colours
        SetMapSize();
        SetSeed();
        mapData = new TileAttribute[iMapSize];
        SetColours();

        // Set the mesh and material to the hex tiles
        hexMesh = hexPre[0].GetComponent<MeshFilter>().sharedMesh;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        Texture2D worldMap = new Texture2D(mapWidth, mapHeight);
        matrix_array[0] = new List<Matrix4x4>();

        // Generate the map
        GenVoronoiValues();
        int tile = 0;
        float w_x = 0, w_y = 0;
        int counter = 1;
        for(int i=0; i<iMapSize; i++){
            w_x += 0.5f;

            // Generate the tile
            tile = MapGenerator(w_x, w_y);

            // Set and store the tile attributes
            mapData[i] = new TileAttribute(tile, w_x, w_y);
            Vector3 position = new Vector3(mapData[i].GetX(), 0, mapData[i].GetY());
            matrix_array[matrixSize].Add(Matrix4x4.TRS(position, Quaternion.identity, Vector3.one*0.5f));
            worldMap.SetPixel(i%mapWidth, (int)Mathf.Floor(w_y/0.43f), colors[tile]);
            if(matrix_array[matrixSize].Count == 1022){
                matrixSize++;
                matrix_array[matrixSize] = new List<Matrix4x4>();
            }
            // Set the position of x coord on the next line of the map
            if((i + 1) % mapWidth == 0){
                if(counter % 2 == 0){
                    w_x = 0;
                }
                else{
                    w_x = 0.25f;
                }
                // Set the position of y coord on the next line of the map
                w_y += (float)Math.Sqrt((0.5 * 0.5) - (0.25 * 0.25));
                counter++;
            }
        }
        // Apply the map to the shader (in future i will display the map to the screen with shader)
        worldMap.Apply();
        mpb.SetTexture("_WorldMap", worldMap);

        ////////////////////////////////////////////////////////////
        // Save the map to a PNG file, in future i will use this to save the map to a file and load to the shader
        byte[] bytes = worldMap.EncodeToPNG();
        string filePath = Application.dataPath + "/worldMap.png";
        System.IO.File.WriteAllBytes(filePath, bytes);
        ////////////////////////////////////////////////////////////
        DrawBoard_v2();
    }

    //Debugging perlin noise
    // void OnValidate(){
    //     GameObject go = GameObject.Find("tex");
    //     Texture2D tex = new Texture2D(mapWidth, mapHeight);
    //     for(int y = 0; y < mapWidth; y++){
    //         for(int x = 0; x < mapHeight; x++){
    //             // float sample = PerlinValue(x/96.0f, y/60.0f, frequency, amplitude, octaves, lacunarity, presistence);
    //             float sample = PingPongPerlinValalue(x/96.0f, y/60.0f, frequency);
    //             // float sample = PerlinValue(x/96.0f, y/60.0f, frequency, amplitude);
    //             tex.SetPixel(x, y, new Color(sample, sample, sample));
    //         }
    //     }
    //     tex.Apply();
    //     go.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
    // }

    void Update(){
        // DrawBoard();
    }
}