using PGC_Terrain;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviourPunCallbacks
{
    public PGCTerrain pgcTerrain;
    public AnimationCurve buildingHeightDistribution;
    public AnimationCurve buildingFootprintSizeDistribution;
    [Range(10,50)]
    public int numberOfBuildings = 20;
    [Range(5,20)]
    public float maxBuildingHeight = 10;
    [Range(1,5)]
    public float minBuildingHeight = 3;
    [Range(8,20)]
    public float maxBuildingFootprint = 15;
    [Range(3, 8)]
    public float minBuildingFootprint = 4;

    // Solves missing shaders in build for primitives in URP....
    public GameObject cubePrefab;

    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            // Call a buffered RPC for initialization so players get it whenever they join
            this.photonView.RPC("StartMapGeneration", RpcTarget.AllBuffered, Random.Range(1, 1000));
        }
    }

    [PunRPC]
    public void StartMapGeneration(int seed)
    {
        Random.InitState(seed);
        GenerateRandomTerrain();
        ApplyTerrainTextures();
        BuildWalls();
        PlaceBuildings();
    }

    private void GenerateRandomTerrain()
    {
        pgcTerrain.perlinScale = 93;
        pgcTerrain.perlinFrequency = 5;
        pgcTerrain.perlinAmplitude = 0.01f;

        pgcTerrain.ApplyPerlinNoise(pgcTerrain.gameObject.GetComponent<Terrain>(), false);
    }

    private void ApplyTerrainTextures()
    {
        pgcTerrain.minHeightDirt = 0f;
        pgcTerrain.maxHeightDirt = 0.0015f;
        pgcTerrain.minHeightGrass = 0.001f;
        pgcTerrain.maxHeightGrass = 30f;

        //pgcTerrain.minSlopeDirt = 0f;
        //pgcTerrain.maxSlopeDirt = 90f;
        //pgcTerrain.minSlopeGrass = 0f;
        //pgcTerrain.maxSlopeGrass = 90f;

        pgcTerrain.ApplyTerrainTextures(pgcTerrain.gameObject.GetComponent<Terrain>());
    }

    // Build walls around the outside of the map
    private void BuildWalls()
    {
        float buildingsPerWall = 50;
        float terrainSize = pgcTerrain.gameObject.GetComponent<Terrain>().terrainData.size.x;
        float buildingSize = terrainSize / buildingsPerWall * 1.1f;
        for (int i = 0; i < buildingsPerWall; i++)
        {
            // Bottom
            GameObject bottom = GameObject.Instantiate(cubePrefab);
            bottom.transform.position = new Vector3(i * terrainSize / buildingsPerWall, 0,0);
            bottom.transform.rotation = Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(-15f, 15f), Random.Range(-3f, 3f));
            bottom.transform.localScale = new Vector3(buildingSize, Random.Range(10f,20f), buildingSize);
            // Top
            GameObject top = GameObject.Instantiate(cubePrefab);
            top.transform.position = new Vector3(i * terrainSize / buildingsPerWall, 0, terrainSize);
            top.transform.rotation = Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(-15f, 15f), Random.Range(-3f, 3f));
            top.transform.localScale = new Vector3(buildingSize, Random.Range(10f, 20f), buildingSize);
            // Left
            GameObject left = GameObject.Instantiate(cubePrefab);
            left.transform.position = new Vector3(0, 0, i * terrainSize / buildingsPerWall);
            left.transform.rotation = Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(-15f, 15f), Random.Range(-3f, 3f));
            left.transform.localScale = new Vector3(buildingSize, Random.Range(10f, 20f), buildingSize);
            // Right
            GameObject right = GameObject.Instantiate(cubePrefab);
            right.transform.position = new Vector3(terrainSize, 0, i * terrainSize / buildingsPerWall);
            right.transform.rotation = Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(-15f, 15f), Random.Range(-3f, 3f));
            right.transform.localScale = new Vector3(buildingSize, Random.Range(10f, 20f), buildingSize);
        }
    }

    private void PlaceBuildings()
    {
        int frequencyResolution = 100;
        List<float> heightFrequencyDistribution = new List<float>();
        List<float> footprintFrequencyDistribution = new List<float>();
        for (int curvePositionIndex = 0; curvePositionIndex < frequencyResolution; curvePositionIndex++)
        {
            float step = (float)curvePositionIndex / (float)frequencyResolution;
            int heightFrequency = (int)(Mathf.Clamp01(buildingHeightDistribution.Evaluate(step)) * frequencyResolution);
            float heightRange = maxBuildingHeight - minBuildingHeight;
            for(int i = 0; i < heightFrequency; i++)
            {
                heightFrequencyDistribution.Add(step * heightRange + minBuildingHeight);
            }

            int footprintFrequency = (int)(Mathf.Clamp01(buildingFootprintSizeDistribution.Evaluate(step)) * frequencyResolution);
            float footprintRange = maxBuildingFootprint - minBuildingFootprint;
            for (int i = 0; i < heightFrequency; i++)
            {
                footprintFrequencyDistribution.Add(step * footprintRange + minBuildingFootprint);
            }
        }

        float terrainSize = pgcTerrain.gameObject.GetComponent<Terrain>().terrainData.size.x;

        for (int i = 0; i < numberOfBuildings; i++)
        {
            float buildingHeight = heightFrequencyDistribution[Random.Range(0, heightFrequencyDistribution.Count)];
            float buildingFootprint = footprintFrequencyDistribution[Random.Range(0, footprintFrequencyDistribution.Count)];

            Vector3 buildingPosition = new Vector3(Random.Range(terrainSize * 0.1f, terrainSize * 0.9f), 0, Random.Range(terrainSize * 0.1f, terrainSize * 0.9f));
            buildingPosition.y = pgcTerrain.gameObject.GetComponent<Terrain>().SampleHeight(buildingPosition);

            GameObject building = GameObject.Instantiate(cubePrefab);
            building.transform.position = buildingPosition;
            building.transform.localScale = new Vector3(buildingFootprint, buildingHeight * 2, buildingFootprint);
            building.transform.rotation = Quaternion.Euler(Random.Range(-7f, 7f), Random.Range(-45f, 45f), Random.Range(-7f, 7f));
        }
    }
}
