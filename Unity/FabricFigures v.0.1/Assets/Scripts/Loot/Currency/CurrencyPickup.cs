using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyPickup : MonoBehaviour 
{

	//VARIABLES
	
	//Components
	GameObject player;
	PlayerManager pMan;
	Animator anim;

	//Main
	public enum PickupObject {Small, Medium, Big};
	public PickupObject currentObject;
	public int pickupQuantity;
	int pickupValue = 1;
	
	
	void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
		pMan = player.GetComponent<PlayerManager>();
		
		//Determine value
		Value();

		anim = GetComponent<Animator>();
		AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);//could replace 0 by any other animation layer index
		anim.Play(state.fullPathHash, -1, Random.Range(0f, 1f));
	}
	
	void Value()
	{
		switch(currentObject)
		{
			case PickupObject.Small:
				pickupValue = 1;
				break;
			case PickupObject.Medium:
				pickupValue = 5;
				break;
			case PickupObject.Big:
				pickupValue = 50;
				break;
			default:
				pickupValue = 0;
				break;
		}
	}
    private void OnTriggerEnter(Collider other)
    {
		//print(other.gameObject.tag);
		if (other.gameObject.tag == "Player")
		{
			pMan.charCurrency += pickupValue;
			//Debug.Log(pMan.charCurrency);
			Destroy(gameObject);
		}
	}
}