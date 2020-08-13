using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviourPunCallbacks
{
    public Vector3 bounds;
    public Vector2Int numRays;

    public float spawnRange;
    public float spawnDistance;

    public GameObject[] items;

    RaycastHit[,] hits; 
    bool[,] activeSpots;

    public float spawnCooldown;

    // Start is called before the first frame update
    void Start(){
        if (!PhotonNetwork.IsMasterClient)
        {
            this.enabled = false;
            return;
        }

        hits = new RaycastHit[numRays.x, numRays.y];
        activeSpots = new bool[numRays.x, numRays.y];
        SpawnableAreas();

        StartCoroutine(ItemSpawnerSystem());
    }

    IEnumerator ItemSpawnerSystem(){

        while(true){

            int index = Random.Range(0, items.Length);
            int x, y;
            do{
                x = Random.Range(0, numRays.x);
                y = Random.Range(0, numRays.y);
            } while(!activeSpots[x, y]);
            

            PhotonNetwork.Instantiate(items[index].name, hits[x, y].point + Vector3.up * 0.2f, Quaternion.identity);
            yield return new WaitForSeconds(spawnCooldown);
        }
    }


    void SpawnableAreas(){
        for(int i = 0; i < numRays.x; i++){
            for(int j = 0; j < numRays.y; j++){
                
                Vector3 position = this.transform.position + new Vector3(bounds.x * ((float)(i + 1) / (float)(numRays.x + 1)), bounds.y, bounds.z * ((float)(j + 1) / (float)(numRays.y + 1)));

                Physics.Raycast(position, Vector3.down, out hits[i, j], Mathf.Abs(bounds.y));
            }
        } 

        float topOfSpawnableArea = this.transform.position.y + spawnDistance + spawnRange / 2;
        float bottomOfSpawnableArea = this.transform.position.y + spawnDistance - spawnRange / 2;

        for (int x = 0; x < numRays.x; x++){
            for(int y = 0; y < numRays.y; y++){
                Vector3 position = this.transform.position + new Vector3(bounds.x * ((float)(x + 1) / (float)(numRays.x + 1)), bounds.y, bounds.z * ((float)(y + 1) / (float)(numRays.y + 1)));
                
                if(hits[x,y].point.y <= topOfSpawnableArea && hits[x, y].point.y >= bottomOfSpawnableArea) { 
                    activeSpots[x, y] = true;
                }
            }
        }
    }

    public void OnDrawGizmos() {
        for(int i = 0; i < numRays.x; i++){
            for(int j = 0; j < numRays.y; j++){
                Vector3 position = this.transform.position + new Vector3(bounds.x * ((float)(i + 1) / (float)(numRays.x + 1)), bounds.y, bounds.z * ((float)(j + 1) / (float)(numRays.y + 1)));
                

                if(hits != null){
                    if(activeSpots[i,j])
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.red;
                    
                    Gizmos.DrawSphere(hits[i,j].point, .5f);
                }

                Gizmos.DrawRay(position, Vector3.down);

            }
        }
        Gizmos.color = Color.white;


        Gizmos.DrawWireCube(this.transform.position + new Vector3(bounds.x/2, spawnDistance, bounds.z/2), new Vector3(bounds.x, spawnRange, bounds.z));
        Gizmos.DrawWireCube(this.transform.position + bounds/2, bounds);
    }
}
