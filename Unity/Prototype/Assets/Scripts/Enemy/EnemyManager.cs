using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
	//VARIABLES
	
	//Components
	private GameObject player;
	[SerializeField] Transform[] routePoints;
	DropsGen dropsGen;
	Collider eCol;
	
	//Main
	[SerializeField] int maxHP;
	public float currHP;
	[SerializeField] int maxPoise;
	public float currPoise;
	[SerializeField] float pRegenRate = 1f;
	
	//Loot
	public int currDrop = 0;
	[SerializeField] GameObject[] lootDrop;
	[SerializeField] GameObject[] lootDropChance;
	
	//Visión
	[SerializeField] float vRange;
	[SerializeField] float vRadius;
	RaycastHit hit;
	
	
	//METHODS
	
	#region START/UPDATE
	
	void Start()
	{
		//Set components
		player = GameObject.FindGameObjectWithTag("Player");
		dropsGen = gameObject.GetComponent<DropsGen>();
		eCol = gameObject.GetComponent<Collider>();


		currHP = maxHP;
		currPoise = maxPoise;
		StartCoroutine("PoiseRegen");
	}
	
	void Update()
	{
	}
	
	#endregion
	
	IEnumerator PoiseRegen ()
	{
		while(currPoise < maxPoise)
		{
			currPoise++;
		yield return new WaitForSeconds(pRegenRate);
		}
	}
	
	public void Death()
	{
		eCol.enabled = false;
		Debug.Log("Dead");
		dropsGen.Drops();
		Destroy(gameObject, 3f);
	}
}
