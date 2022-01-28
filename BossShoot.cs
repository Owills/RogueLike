using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using Scene.SceneManager; 

public class BossShoot : MonoBehaviour {
    public static bool startBattle = false;
    public static bool started;
    public static float timer = 2f;
    private int rand;
    public static float radius, moveSpeed;
    public static float fireRate;
   
    //private Vector3 translationDir;
    public GameObject ball;
    //public GameObject[] movePoints;
    //private bool moved = false; 

    private Vector2 center;
    private float _angle;
    private float bossHealth; 
    private Boss bossChar;
    private float fireTimer;

    	void Start() { 
		Invoke(nameof(LoadUp),3f);       
        	radius = 0.5f;
        	moveSpeed = 0.7f;
        	center = transform.position; 
       
    	}

	private void LoadUp() {
		fireTimer = fireRate;
		bossChar = this.GetComponent<Boss>(); 
        bossHealth = bossChar.GetComponent<Boss>().GetHealth();
        var pos = transform.position;
		started = true;
    }
   
    private void FixedUpdate() {
        if (Boss.startBattle) {
            float t = Time.deltaTime;
            timer -= t;
            if (timer <= 0) {                
                Shoot();
                if(bossChar.GetComponent<Boss>().GetHealth() <= (bossHealth / 2)) {
                    timer = 1.2f;
                }else {
                    timer = 2f;
                }                
            }
            if (bossChar.GetComponent<Boss>().GetHealth() <= (bossHealth / 2)) {
                moveSpeed = 1.1f;
            }
        }           
    }

    public void Shoot() {
        int numberOfProjectiles = Random.Range(6, 12);
        Vector2 startPoint = this.transform.position; 

        float angleStep = 360f / numberOfProjectiles;
        float angle = 0f;

        for (int i = 0; i <= numberOfProjectiles - 1; i++) {

            float projectileDirXposition = startPoint.x + Mathf.Sin((angle * Mathf.PI) / 180) * radius;
            float projectileDirYposition = startPoint.y + Mathf.Cos((angle * Mathf.PI) / 180) * radius;

            Vector2 projectileVector = new Vector2(projectileDirXposition, projectileDirYposition);
            Vector2 projectileMoveDirection = (projectileVector - startPoint).normalized * moveSpeed;

            var proj = Instantiate(ball, startPoint, Quaternion.identity);
            proj.GetComponent<Projectile>().SetIsFriendly(false);
            proj.GetComponent<Projectile>().SetDamage(10);
            proj.GetComponent<Rigidbody2D>().velocity =
                new Vector2(projectileMoveDirection.x, projectileMoveDirection.y);

            angle += angleStep;
        }
    }
    


















    /*
    [SerializeField] private TriggerFight colliderTrigger;
    //[SerializeField] private EnemySpawn enemy;
    private List<Vector3> spawnPositions;
    public static bool startBattle = false;
        
    private GameObject boss;
    //regen heath, spawn enemies in boss room, shield around boss 
    //spawn projectiles in circle
    //delete if beyond a certian range
    private void Start() {
        if (startBattle) {
            InvokeRepeating("SpawnProjectiles", 1.0f, 1.0f);
        }        
        //transform.position = spawnPoint;
        colliderTrigger.OnPlayerEnterTrigger += ColliderTrigger_OnPlayerEnterTrigger;
        //boss = GameObject.Find("JazzBoss");
        //Instantiate(boss, spawnPoint, Quaternion.identity);
    }
    */
    /*
    private void Awake() {
        spawnPositions = new List<Vector3>();
        foreach(Transform spawnPosition in transform.Find("EnemySpawnPositions")) {
            spawnPositions.Add(spawnPosition.position); 
        }
    }
    */
    /*
    void SpawnProjectiles() {
        //RadialProjectiles.SpawnProjectiles(RadialProjectiles.numberOfProjectiles);
        startBattle = true;
    }
    private void ColliderTrigger_OnPlayerEnterTrigger(object sender, System.EventArgs e) {
        StartBattle();
        startBattle = true;
        colliderTrigger.OnPlayerEnterTrigger -= ColliderTrigger_OnPlayerEnterTrigger; 
    }
    private void StartBattle() {
        //InvokeRepeating("SpawnProjectiles", 1.0f, 1.0f);
        Debug.Log("StartBattle");
        //SpawnEnemy();
    }
    private void Update()	{
        transform.position = spawnPoint;
    }
    */
    /*
    private void SpawnEnemy() {
    Vector3 spawnPosition = spawnPositions[Random.Range(0,spawnPositions.Count)]; 
        EnemySpawn enemySpawn = Instantiate(enemySpawn, transform.position +
                new Vector3(20, 0), Quaternion.identity);
        enemySpawn.Spawn(); 
    }
    */


}
