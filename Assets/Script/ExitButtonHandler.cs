using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButtonHandler : MonoBehaviour
{
	[SerializeField] private Text skillText;

	public void ExitApp()
	{
		Application.Quit();
	}

	public void SoundOnOff()
	{
		GlobalVar.SoundOn = !GlobalVar.SoundOn;
		skillText.text = GlobalVar.SoundOn ? "SOUND ON" : "SOUND OFF";
	}
}

public static class GlobalVar
{
	public static bool SoundOn = true;
}
