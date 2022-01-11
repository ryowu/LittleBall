using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillFruitController : MonoBehaviour
{
	[SerializeField] private FruitSkills SkillIndex;
	public ProcessController pc;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		pc.EnableSkill(SkillIndex);
		GetComponent<Animator>().SetTrigger("collect");
		Destroy(GetComponent<BoxCollider2D>());
	}

	private void ItemCollected()
	{
		Destroy(this.gameObject);
	}
}
