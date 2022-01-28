using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private List<Transform> targets;
    private Transform currentTarget;
    // Start is called before the first frame update
    void Start()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            targets.Add(player.transform);
        }
        if(targets != null)
        {
            currentTarget = targets[0];
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        bool inRange =  CheckCloseToTag("Player", 1.8f);
        if (Input.GetKeyDown(KeyCode.E) && inRange)
        {
            Destroy(gameObject);
        }
    }
    bool CheckCloseToTag(string tag, float minimumDistance)
    {
        GameObject[] goWithTag = GameObject.FindGameObjectsWithTag(tag);

        for (int i = 0; i < goWithTag.Length; ++i)
        {
            if (Vector3.Distance(transform.position, goWithTag[i].transform.position) <= minimumDistance)
                return true;
        }

        return false;
    }
}
