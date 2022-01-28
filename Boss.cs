using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Boss : Character {
    private static Rigidbody2D rb;
    public static bool startBattle = false;
    public static Transform player;
    private float changeDirTimer;
	private int rand;
	private Vector3 translationDir;	
	public float bossDamage;
	private AudioSource[] allAudioSources;
	[SerializeField] private AudioSource bossSource;
	private bool battleStarted = false;
	public BossHealth healthBar;



	// Start is called before the first frame update
	void Start()
	{
		base.OnStartClient();
		changeDirTimer = 1f;
		rb = GetComponent<Rigidbody2D>();
		rand = 1;
		healthBar.SetHealth(this.GetComponent<Boss>().GetHealth(),
				this.GetComponent<Boss>().GetMaxHealth());
	}


    // Update is called once per frame
    void FixedUpdate()
	{
		if(startBattle)		{
			if (!battleStarted)
			{
				StopAllAudio();
				bossSource.Play();
				battleStarted = true;
			}
			if (changeDirTimer <= 0)
			{
				rand = Random.Range(1, 5);
				changeDirTimer = 1;
			}
			else
			{
				changeDirTimer -= Time.fixedDeltaTime;
			}
			
			GetComponent<Rigidbody2D>().velocity = Vector3.zero;
			GetComponent<Rigidbody2D>().angularVelocity = 0f;
			if(rand == 1)
			{
				translationDir = new Vector3(1, 0, 0);
			}
			else if(rand == 2)
			{
				translationDir = new Vector3(0, 1, 0);
			}
			else if(rand == 3)
			{
				translationDir = new Vector3(-1, 0, 0);
			}
			else if(rand == 4)
			{
				translationDir = new Vector3(0, -1, 0);
			}

			//GetComponent<BossMovement>().MoveTo(transform.position + translationDir.normalized * (Time.fixedDeltaTime * movementSpeed));
			
			
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if(!other.gameObject.CompareTag("Projectile") && !other.gameObject.CompareTag("Player"))
		{
			// then it is a wall
			translationDir *= -1; // invert translation
		}
	}
	public void SetStartBattle(bool isBattling)
	{
		startBattle = isBattling;
	}
	public override void ApplyDamage(float damage) {
		base.ApplyDamage(damage);		
		if(health <= 0) {
			Destroy(gameObject);
			Debug.Log("die");
			Die(); 
		}
		healthBar.SetHealth(this.GetComponent<Boss>().GetHealth(), 
				this.GetComponent<Boss>().GetMaxHealth());
	}

	public float GetDamage() {
		return bossDamage;
	}

	public void StopAllAudio()
	{
		allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		foreach (AudioSource audioS in allAudioSources)
		{
			audioS.Stop();
		}
	}

	protected override void Die()
	{
		var Music = GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>();
		Music.Play();
		base.Die();
		Destroy(gameObject);
		Debug.Log("sdfsd");

	}
}
