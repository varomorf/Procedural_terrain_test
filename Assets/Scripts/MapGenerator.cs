using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public const int mapChunkSize = 239;
    [Range(0, 6)]
    public int editorPreviewLevelOfDetail;

    public TerrainType[] regions;

    public bool autoUpdate;

    public TerrainData terrainData;
    public NoiseData noiseData;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void OnValuesUpdated() {
        if (!Application.isPlaying) {
            DrawMapInEditor();
        }
    }

    void OnValidate() {
        if(terrainData != null){
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }

        if(noiseData != null){
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
    }

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        switch (drawMode) {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.GenerateTextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.GenerateTextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.Mesh:
                Texture2D texture = TextureGenerator.GenerateTextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize);
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.heightMultiplier, terrainData.heighMultiplierCurve, editorPreviewLevelOfDetail), texture);
                break;
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback) {
        MapData mapData = GenerateMapData(center);

        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.heightMultiplier, terrainData.heighMultiplierCurve, lod);

        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update() {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> info = mapDataThreadInfoQueue.Dequeue();
                info.action(info.param);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MeshData> info = meshDataThreadInfoQueue.Dequeue();
                info.action(info.param);
            }
        }
    }

    MapData GenerateMapData(Vector2 center) {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

        Color[] colors = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight >= regions[i].height) {
                        colors[y * mapChunkSize + x] = regions[i].color;
                    } else {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colors);
    }

}

public struct MapData {

    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }

}

public struct MapThreadInfo<T> {
    public readonly Action<T> action;
    public readonly T param;

    public MapThreadInfo(Action<T> action, T param) {
        this.action = action;
        this.param = param;
    }
}