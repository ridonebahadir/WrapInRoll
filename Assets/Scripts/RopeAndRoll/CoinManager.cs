using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;


public class CoinManager : MonoBehaviour
{
	//References
	//[Header("UI references")]
	//[SerializeField] TMP_Text coinUIText;
	//[SerializeField] GameObject animatedCoinPrefab;
	
	//[SerializeField] Transform target;

	//[Space]
	//[Header("Available coins : (coins to pool)")]
	//[SerializeField] int maxCoins;
	//Queue<GameObject> coinsQueue = new Queue<GameObject>();


	//[Space]
	[Header("Animation settings")]
	[SerializeField] [Range(0.5f, 0.9f)] float minAnimDuration;
	[SerializeField] [Range(0.9f, 2f)] float maxAnimDuration;

	[SerializeField] Ease easeType;
	[SerializeField] float spread;

	//Vector3 targetPosition;


	private int _c = 0;



	[Serializable]
	public struct Pool
    {
        public Queue<GameObject> pooledObjects;
        public GameObject objectPrefab;
        public int poolSize;
    }

	public Pool[] pools = null;

	

	public int Coins
	{
		get { return _c; }
		set
		{
			_c = value;
			//update UI text whenever "Coins" variable is changed
			//coinUIText.text = Coins.ToString();
		}
	}

	void Awake()
	{
		//targetPosition = target.position;

		//prepare pool
		PrepareCoins();
	}

	void PrepareCoins()
	{
		for (int j = 0; j < pools.Length; j++)
		{
			pools[j].pooledObjects = new Queue<GameObject>();

			for (int i = 0; i < pools[j].poolSize; i++)
			{
				GameObject obj = Instantiate(pools[j].objectPrefab);
				obj.SetActive(false);

				pools[j].pooledObjects.Enqueue(obj);
			}
		}
	}
	
	void Animate(Vector3 collectedCoinPosition, int amount,Vector3 target,int objectType,Color color)
	{
		for (int i = 0; i < amount; i++)
		{
			//check if there's coins in the pool
			if (pools[objectType].pooledObjects.Count > 0)
			{
				//extract a coin from the pool
				GameObject coin = pools[objectType].pooledObjects.Dequeue();
				coin.GetComponent<ParticleSystemRenderer>().material.color = color;
				coin.SetActive(true);

				//move coin to the collected coin pos
				coin.transform.position = collectedCoinPosition + new Vector3(UnityEngine.Random.Range(-spread, spread), 0f, 0f);

				//animate coin to target position
				float duration =UnityEngine.Random.Range(minAnimDuration, maxAnimDuration);
				coin.transform.DOMove(target, duration)
				.SetEase(easeType)
				.OnComplete(() => {
					//executes whenever coin reach target position
					coin.SetActive(false);
					pools[objectType].pooledObjects.Enqueue(coin);

					Coins++;
				});
			}
		}
	}

	public void AddCoins(Vector3 collectedCoinPosition, int amount,Vector3 target,int objectType,Color color)
	{
		Animate(collectedCoinPosition, amount,target, 0,color);
	}
}