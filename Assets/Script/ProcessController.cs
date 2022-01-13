using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FruitSkills
{
	BigBall,
	PowerBall,
	LessObstacle,
	Rearrange,
	RPG
}

public class ProcessController : MonoBehaviour
{
	#region  Const
	private const float DEAD_LINE = 5f;
	private const float OBSTACLE_MOVE_DISTANCE = 1.4f;
	private const float OBSTACLE_HORZ_DISTANCE = 1.5f;
	private const float OBSTACLE_START_Y = -7.6f;
	private const int STARTUP_MAX_BALL_COUNT = 3;
	//String format
	private const string BallStringFormat = "BALL:{0}/{1}";
	private const string ScoreStringFormat = "SCORE:{0}";
	private const string LevelStringFormat = "LEVEL:{0}";
	private const string HighScoreStringFormat = "HIGHEST:{0}";
	//Skill fruit
	private const float FRUIT_TOTAL_RATE = 20f; //max 100
	private const float LESS_BLOCKS_RATE = 0.8f; //banana skill
	private const string MELON_TEXT = "BIG BALL IN NEXT 3 ROUNDS";
	private const string BANANA_TEXT = "BLOCKS 20% OFF IN NEXT 2 ROUNDS";
	private const string APPLE_TEXT = "POWER BALL IN NEXT 2 ROUNDS";
	private const string PINEAPPLE_TEXT = "BLOCKS REARRANGED";
	private const string KIWI_TEXT = "RPG ACQUIRED";
	#endregion

	#region SerializeField
	[SerializeField] private GameObject playerBall;
	[SerializeField] private GameObject playerRPG;
	[SerializeField] private GameObject obstacleSquare;
	[SerializeField] private GameObject obstacleHexagon;
	[SerializeField] private GameObject obstacleTriangle;
	[SerializeField] private Text BallCount;
	[SerializeField] private Text ScoreText;
	[SerializeField] private Text LevelText;
	[SerializeField] private Text HighScoreText;
	[SerializeField] private GameObject TextArea;
	[SerializeField] private Text SkillText;
	[SerializeField] private Text GameoverText;
	[SerializeField] private AudioSource GameOverEffect;
	[SerializeField] private AudioSource RPGEffect;
	[SerializeField] private GameObject infoLabelPlaceholder;
	[SerializeField] private GameObject Melon;
	[SerializeField] private GameObject Apple;
	[SerializeField] private GameObject Banana;
	[SerializeField] private GameObject Pineapple;
	[SerializeField] private GameObject Kiwi;
	[SerializeField] private GameObject RPGButton;
	#endregion

	#region private members
	private GameObject pb;
	private Rigidbody2D rb;
	private bool isPlaying;
	private Vector2 forceVec;
	private List<GameObject> obstacles;
	private int maxPlayerBallCount;
	private int currentPlayerBallCount;
	private int recyledBallCount;
	private int level;
	private int gameStep;
	private int highScore;
	private DateTime dtBallStart;
	private int skillTimes;
	private bool itemExist;
	private bool obstacleStartPosDeltaTag;
	private bool isRPGflying;
	private FruitSkills currentSkill;
	#endregion

	public int Score;
	public bool RPG_Ready;
	public bool IsPlaying { get { return this.isPlaying; } }

	#region Start and Update
	// Start is called before the first frame update
	void Start()
	{
		ResetCameraSize();
		UpdateTextPosition();

		highScore = 0;
		obstacles = new List<GameObject>();
		InitGame();
	}

	// Update is called once per frame
	void Update()
	{
		if (isRPGflying) return;

		//Process RPG
		if (RPG_Ready && Input.GetMouseButton(0))
		{
			//Get click postion
			Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			//if it is valid position, under deadline
			if (worldPosition.y > 5.15f || worldPosition.y < -9.3f) return;

			RPGEffect.Play();

			RPG_Ready = false;
			RPGButton.GetComponent<RPG_Handler>().rpgReady = false;
			RPGButton.SetActive(false);
			isRPGflying = true;

			//
			forceVec = worldPosition - new Vector2(0f, 7.2f);

			//Send new RPG out
			Vector3 relativeTarget = forceVec.normalized;
			Quaternion toQuaternion = Quaternion.FromToRotation(Vector3.up, relativeTarget);

			GameObject newRPG = Instantiate(playerRPG, new Vector2(0f, 7.2f), toQuaternion, transform.parent);
			Rigidbody2D rpgRB = newRPG.GetComponent<Rigidbody2D>();
			rpgRB.bodyType = RigidbodyType2D.Dynamic;
			rpgRB.AddForce(forceVec.normalized * 800f);
			return;
		}

		//Process mouse input
		if (Input.GetMouseButton(0) && currentPlayerBallCount >= maxPlayerBallCount)
		{
			//Get click postion
			Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			//if it is valid position, under deadline
			if (worldPosition.y > 5.15f || worldPosition.y < -9.3f) return;

			//
			forceVec = worldPosition - new Vector2(0f, 7.2f);
			//
			GameoverText.enabled = false;
			isPlaying = true;

			//consume skill
			SkillText.text = string.Empty;
			skillTimes--;
			if (skillTimes < 0) skillTimes = 0;
		}

		//Send new ball
		if (isPlaying && currentPlayerBallCount > 0)
		{
			TimeSpan ts = DateTime.Now - dtBallStart;
			if (ts.TotalMilliseconds < 200)
				return;
			else
				dtBallStart = DateTime.Now;

			currentPlayerBallCount--;
			RefreshBallCount();

			pb = Instantiate(playerBall, new Vector2(0f, 7.2f), transform.rotation, transform.parent);

			//Big ball skill
			if (skillTimes > 0 && currentSkill == FruitSkills.BigBall)
			{
				pb.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
			}

			//Power ball skill
			if (skillTimes > 0 && currentSkill == FruitSkills.PowerBall)
			{
				pb.GetComponent<SpriteRenderer>().color = new Color(0.788f, 0.611f, 0.305f, 1f);//201,156,78
				pb.GetComponent<PlayerBallController>().BallDamage = 2;
			}
			else
			{
				pb.GetComponent<SpriteRenderer>().color = new Color(0.87f, 0.45f, 0.53f, 1f);//222,116,135
				pb.GetComponent<PlayerBallController>().BallDamage = 1;
			}

			rb = pb.GetComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Dynamic;
			rb.AddForce(forceVec.normalized * 800f);
		}
	}
	#endregion

	#region Private method
	private void OnTriggerEnter2D(Collider2D collision)
	{
		//Destroy player RPG
		Destroy(collision.gameObject);

		if (collision.gameObject.CompareTag("RPG"))
		{
			isRPGflying = false;
			SkillText.text = string.Empty;
			GameoverText.text = string.Empty;
			GenerateNextStep();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//Destroy player ball
		Destroy(collision.gameObject);

		//Recycle ball
		recyledBallCount++;

		if (recyledBallCount >= maxPlayerBallCount)
		{
			GenerateNextStep();
		}
	}

	private void GenerateNextStep()
	{
		//game steps calculate
		gameStep++;
		level = gameStep / 10 + 1;
		if (level > 10)
			level = 10;
		maxPlayerBallCount = STARTUP_MAX_BALL_COUNT - 1 + (int)((float)level * 1.5f);
		RefreshScore();

		//Reset ball count
		recyledBallCount = 0;
		currentPlayerBallCount = maxPlayerBallCount;
		RefreshBallCount();

		//move and add new obstacles
		MoveAndAddObstalces();

		//if reach top

		//allow playing again
		isPlaying = false;

		//create a fruit item
		GenerateSkillFruit();
	}

	private void GenerateSkillFruit()
	{
		if (!itemExist)
		{
			float fruitRate = UnityEngine.Random.Range(0f, 100f);

			////test
			//fruitRate = 6f;

			if (fruitRate <= FRUIT_TOTAL_RATE)
			{
				float posX = UnityEngine.Random.Range(-2.97f, 2.95f);
				if (fruitRate < 4f)
				{
					GameObject newPineapple = Instantiate(Pineapple, new Vector2(posX, 4.3f), transform.rotation, transform.parent);
					newPineapple.GetComponent<SkillFruitController>().pc = this;
				}
				else if (fruitRate < 7f)
				{
					GameObject newKiwi = Instantiate(Kiwi, new Vector2(posX, 4.3f), transform.rotation, transform.parent);
					newKiwi.GetComponent<SkillFruitController>().pc = this;
				}
				else if (fruitRate < 11f)
				{
					GameObject newApple = Instantiate(Apple, new Vector2(posX, 4.3f), transform.rotation, transform.parent);
					newApple.GetComponent<SkillFruitController>().pc = this;
				}
				else if (fruitRate < 15f)
				{
					GameObject newBanana = Instantiate(Banana, new Vector2(posX, 4.3f), transform.rotation, transform.parent);
					newBanana.GetComponent<SkillFruitController>().pc = this;
				}
				else
				{
					GameObject newMelon = Instantiate(Melon, new Vector2(posX, 4.3f), transform.rotation, transform.parent);
					newMelon.GetComponent<SkillFruitController>().pc = this;
				}
				itemExist = true;
			}
		}
	}

	private void RefreshBallCount()
	{
		BallCount.text = string.Format(BallStringFormat, currentPlayerBallCount, maxPlayerBallCount);
	}

	public void RefreshScore()
	{
		ScoreText.text = string.Format(ScoreStringFormat, Score);
		LevelText.text = string.Format(LevelStringFormat, level);
		HighScoreText.text = string.Format(HighScoreStringFormat, highScore);
	}

	private void MoveAndAddObstalces()
	{
		//Move current obstacles upstairs
		for (int i = obstacles.Count - 1; i >= 0; i--)
		{
			if (obstacles[i] == null)
				obstacles.RemoveAt(i);
			else
			{
				obstacles[i].transform.position += new Vector3(0f, OBSTACLE_MOVE_DISTANCE, obstacles[i].transform.position.z);
				//judge if game over
				if (obstacles[i].transform.position.y > DEAD_LINE)
				{
					GameOver();
					return;
				}
			}
		}

		//Add new obstacles
		float startX = -3.1f;
		if (obstacleStartPosDeltaTag)
		{
			startX = -1.5f;
		}
		obstacleStartPosDeltaTag = !obstacleStartPosDeltaTag;

		int tempCount = 0;
		if (RandomTrue())
		{
			obstacles.Add(InitObsctale(new Vector2(startX, OBSTACLE_START_Y)));
			tempCount++;
		}
		if (RandomTrue())
		{
			obstacles.Add(InitObsctale(new Vector2(startX + OBSTACLE_HORZ_DISTANCE, OBSTACLE_START_Y))); 
			tempCount++;
		}
		if (RandomTrue())
		{
			obstacles.Add(InitObsctale(new Vector2(startX + OBSTACLE_HORZ_DISTANCE * 2, OBSTACLE_START_Y)));
			tempCount++;
		}
		if (RandomTrue() || tempCount < 1)
		{
			obstacles.Add(InitObsctale(new Vector2(startX + OBSTACLE_HORZ_DISTANCE * 3, OBSTACLE_START_Y)));
			tempCount++;
		}
	}

	private GameObject InitObsctale(Vector2 position)
	{
		int obsSeed = UnityEngine.Random.Range(1, 4);
		GameObject newobj;

		switch (obsSeed)
		{
			case 1:
				{
					newobj = Instantiate(obstacleSquare, position, transform.rotation, transform.parent);
					newobj.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 45f));
					break;
				}
			case 2:
				{
					newobj = Instantiate(obstacleHexagon, position, transform.rotation, transform.parent);
					newobj.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 45f));
					break;
				}
			case 3:
				{
					newobj = Instantiate(obstacleTriangle, position, transform.rotation, transform.parent);
					newobj.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 90f));
					break;
				}
			default:
				{
					newobj = Instantiate(obstacleSquare, position, transform.rotation, transform.parent);
					newobj.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 45f));
					break;
				}
		}

		newobj.GetComponent<ObstacleController>().pc = this;
		float hptemp = UnityEngine.Random.Range(10f * (level - 1), 10f * level);
		//Less blocks skill
		if (skillTimes > 0 && currentSkill == FruitSkills.LessObstacle)
		{
			hptemp *= LESS_BLOCKS_RATE;
		}

		newobj.GetComponent<ObstacleController>().HP = (int)hptemp;
		return newobj;
	}

	private void RearrangeObstacles()
	{
		float startX = -3.1f;

		for (int i = 0; i < 10; i++) //layer
		{
			for (int j = 0; j < 5; j++) //one line, 4 blocks
			{
				int index = i * 5 + j;
				if (index > obstacles.Count - 1) return;

				if (obstacles[index] != null)
					obstacles[index].transform.position = new Vector2(startX + OBSTACLE_HORZ_DISTANCE * j, OBSTACLE_START_Y + i * OBSTACLE_MOVE_DISTANCE);
			}
		}
	}

	private bool RandomTrue()
	{
		return UnityEngine.Random.Range(-1f, 1f) > 0 ? true : false;
	}

	private void GameOver()
	{
		if (Score > highScore)
		{
			highScore = Score;
		}
		GameoverText.text = string.Format("SCORE:{0}" + Environment.NewLine + Environment.NewLine + "GAMEOVER" + Environment.NewLine + "CLICK TO START", Score);
		GameoverText.enabled = true;
		GameOverEffect.Play();
		InitGame();
	}

	private void InitGame()
	{
		//clear obstacles
		for (int i = obstacles.Count - 1; i >= 0; i--)
		{
			Destroy(obstacles[i]);
			obstacles.RemoveAt(i);
		}

		//Clear skill text
		SkillText.text = string.Empty;
		itemExist = false;
		skillTimes = 0;
		GameObject[] fruitItems = GameObject.FindGameObjectsWithTag("item");
		for (int i = fruitItems.Length - 1; i >= 0; i--)
		{
			Destroy(fruitItems[i]);
		}

		//reset score
		Score = 0;
		level = 1;
		gameStep = 0;
		RefreshScore();

		//reset ball count
		maxPlayerBallCount = STARTUP_MAX_BALL_COUNT;
		currentPlayerBallCount = maxPlayerBallCount;
		RefreshBallCount();

		recyledBallCount = 0;
		dtBallStart = DateTime.Now;

		//Init startup obstacles
		obstacleStartPosDeltaTag = false;
		MoveAndAddObstalces();
		//Allow start game
		isPlaying = false;

		//RPG
		RPG_Ready = false;
		isRPGflying = false;
	}

	private void ResetCameraSize()
	{
		float ratio = (float)(Screen.height) / (float)(Screen.width);
		Camera.main.orthographicSize = ratio / 1.777f * 9f;
	}

	private void UpdateTextPosition()
	{
		TextArea.transform.position = Camera.main.WorldToScreenPoint(infoLabelPlaceholder.transform.position);
	}
	#endregion

	#region public methods
	public void EnableSkill(FruitSkills index)
	{
		switch (index)
		{
			case FruitSkills.BigBall:
				{
					SkillText.text = MELON_TEXT;
					skillTimes = 4;
					break;
				}
			case FruitSkills.PowerBall:
				{
					SkillText.text = APPLE_TEXT;
					skillTimes = 3;
					break;
				}
			case FruitSkills.LessObstacle:
				{
					SkillText.text = BANANA_TEXT;
					skillTimes = 3;
					break;
				}
			case FruitSkills.Rearrange:
				{
					SkillText.text = PINEAPPLE_TEXT;
					RearrangeObstacles();
					skillTimes = 0;
					break;
				}
			case FruitSkills.RPG:
				{
					SkillText.text = KIWI_TEXT;
					RPGButton.SetActive(true);
					skillTimes = 0;
					break;
				}
		}

		currentSkill = index;
		itemExist = false;
	}
	#endregion
}
