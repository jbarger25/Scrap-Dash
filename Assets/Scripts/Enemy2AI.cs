using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2AI : MonoBehaviour
{
    public Transform scrap;
    public Rigidbody rb;
    public float speed = 5f;
    public Vector3 lookOffset = new Vector3(0f, 1f, 0f);
    public float lookDistance = 10f;
    public bool avoiding = false;
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            this.enabled = false;
            return;
        }
        rb = GetComponent<Rigidbody>();
        FindScrap();
    }

    // Update is called once per frame
    void Update()
    {
        MoveToScrap();
    }

    void FindScrap()
    {
        if (scrap == null)
        {
            GameObject[] scrapArray = GameObject.FindGameObjectsWithTag("LargeScrap");
            if (scrapArray.Length != 0)
            {
                scrap = GameObject.FindGameObjectWithTag("LargeScrap").transform;
            }
            else
            {
                scrapArray = GameObject.FindGameObjectsWithTag("Scrap");
                if (scrapArray.Length != 0)
                {
                    scrap = GameObject.FindGameObjectWithTag("Scrap").transform;
                }
                else
                {
                    scrapArray = GameObject.FindGameObjectsWithTag("SmallScrap");
                    if (scrapArray.Length != 0)
                    {
                        scrap = GameObject.FindGameObjectWithTag("SmallScrap").transform;
                    }
                }
            }
        }
    }

    void MoveToScrap()
    {
        if (scrap == null)
        {
            Invoke("FindScrap", 2f);
        }
        if(scrap == null)
        {
            return;
        }
        Vector3 moveDirection = scrap.position - transform.position;
        rb.MoveRotation(Quaternion.LookRotation(moveDirection, Vector3.up));
        RaycastHit forwardHit;
        Vector3 forwardRay = transform.position + lookOffset;
        float avoidMultiplier = 0f;
        avoiding = false;
        if(Physics.Raycast(forwardRay, transform.forward, out forwardHit, lookDistance))
        {
            if (forwardHit.collider.CompareTag("Obstacle")) { 
                Debug.DrawLine(forwardRay, forwardHit.point);
                avoiding = true;
                if(Vector3.Distance(scrap.position, forwardHit.transform.position) <= 0.5f)
                {
                    avoiding = false;
                }
                if (!(Physics.Raycast(forwardRay, Quaternion.AngleAxis(-90f, transform.up) * transform.forward, out forwardHit, lookDistance))&&avoiding == true)
                {      
                        Debug.DrawLine(forwardRay, forwardHit.point);
                        avoiding = true;
                        avoidMultiplier -= 1f;
                }
                else if (!(Physics.Raycast(forwardRay, Quaternion.AngleAxis(90f, transform.up) * transform.forward, out forwardHit, lookDistance))&&avoiding == true)
                {
                        Debug.DrawLine(forwardRay, forwardHit.point);
                        avoiding = true;
                        avoidMultiplier += 1f;
                }
            }
        }

        if (avoiding)
        {
            moveDirection = Quaternion.Euler(0, (100 * avoidMultiplier), 0) * moveDirection;
            rb.MoveRotation(Quaternion.LookRotation(moveDirection, Vector3.up));
            rb.velocity = moveDirection.normalized * speed;
        }
        else
        {
            rb.MoveRotation(Quaternion.LookRotation(moveDirection, Vector3.up));
            rb.velocity = moveDirection.normalized * speed;
            //transform.position = Vector3.MoveTowards(transform.position, scrap.position, Time.deltaTime * speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "LargeScrap")
        {
            PhotonNetwork.Destroy(other.gameObject);
            FindScrap();
        }
        if (other.tag == "Scrap")
        {
            PhotonNetwork.Destroy(other.gameObject);
            FindScrap();
        }
        if (other.tag == "SmallScrap")
        {
            PhotonNetwork.Destroy(other.gameObject);
            FindScrap();
        }
    }
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10, Color.red);
    }
}
