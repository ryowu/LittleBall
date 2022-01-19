using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBallController : MonoBehaviour
{
	[SerializeField] private AudioSource ballhit;
	[SerializeField] private AudioSource CollectEffect;

	private Vector2 lastPos;
	private DateTime dtStart;
	private int samePosCount;
	public int BallDamage;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (GlobalVar.SoundOn)
		{
			if (collision.gameObject.CompareTag("obstacle"))
				ballhit.Play();
			else if (collision.gameObject.CompareTag("item"))
				CollectEffect.Play();
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (GlobalVar.SoundOn)
		{
			if (collision.gameObject.CompareTag("obstacle"))
				ballhit.Play();
			else if (collision.gameObject.CompareTag("item"))
				CollectEffect.Play();
		}
	}

	private void Start()
	{
		samePosCount = 0;
		lastPos = transform.position;
		dtStart = DateTime.Now;
	}

	private void Update()
	{
		TimeSpan ts = DateTime.Now - dtStart;
		if (ts.TotalMilliseconds > 500)
		{
			if (Vector2.Distance(lastPos, transform.position) < 0.1f)
				samePosCount++;
			else
				lastPos = transform.position;

			if (samePosCount >= 3)
			{
				transform.position = new Vector2(transform.position.x, transform.position.y - 1.5f);
				samePosCount = 0;
			}

			dtStart = DateTime.Now;
		}
	}
}
