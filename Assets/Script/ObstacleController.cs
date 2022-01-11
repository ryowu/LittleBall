using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObstacleController : MonoBehaviour
{
	private int hp;
	public int HP
	{
		get { return hp; }
		set
		{
			if (value >= 99)
				hp = 99;
			else if (value <= 1)
				hp = 1;
			else
				hp = value;
		}
	}

	[SerializeField] private Text HP_Text;
	public ProcessController pc;

	private SpriteRenderer sr;

	private DateTime dtStart;

	Text t;
	// Start is called before the first frame update
	void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		GameObject cvs = GameObject.FindGameObjectWithTag("MainCanvas");
		t = Instantiate(HP_Text, Camera.main.WorldToScreenPoint(transform.position), cvs.transform.rotation, cvs.transform);
		t.text = hp.ToString();

		dtStart = DateTime.Now;
	}

	// Update is called once per frame
	void Update()
	{
		UpdateColor();
		t.transform.position = Camera.main.WorldToScreenPoint(transform.position);
	}

	private void UpdateColor()
	{
		if (hp >= 80)
		{
			sr.color = new Color(0.349f, 0.368f, 0.4f, 1f);//89,94,102
		}
		else if (hp >= 60)
		{
			sr.color = new Color(0.89f, 0.549f, 0.478f, 1f);//227,140,122
		}
		else if (hp >= 40)
		{
			sr.color = new Color(0.84f, 0.79f, 0.69f, 1f);//215,202,177
		}
		else if (hp >= 30)
		{
			sr.color = new Color(0.88f, 0.8f, 0.8f, 1f);//226,206,206
		}
		else if (hp >= 20)
		{
			sr.color = new Color(0.73f, 0.79f, 0.69f, 1f);//188,203,176
		}
		else if (hp >= 10)
		{
			sr.color = new Color(0.85f, 0.78f, 0.60f, 1f);//217,200,155
		}
		else
		{
			sr.color = new Color(0.6f, 0.64f, 0.73f, 1f); //153,164,188
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		ProcessCollision(collision);
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		TimeSpan ts = DateTime.Now - dtStart;
		if (ts.TotalMilliseconds > 100f)
		{
			ProcessCollision(collision);
			dtStart = DateTime.Now;
		}
	}

	private void ProcessCollision(Collision2D collision)
	{
		Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
		rb.AddForce(new Vector2((UnityEngine.Random.Range(-1f, 1f) > 0 ? 1f : -1f) * 50f, 200f));

		int ballDamage = collision.gameObject.GetComponent<PlayerBallController>().BallDamage;
		hp -= ballDamage;
		UpdateColor();
		pc.Score++;
		pc.RefreshScore();
		t.text = hp.ToString();

		if (hp <= 0)
		{
			Destroy(t.gameObject);
			Destroy(this.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (t != null)
			Destroy(t.gameObject);
	}
}
