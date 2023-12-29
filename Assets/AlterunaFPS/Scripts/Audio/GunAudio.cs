using UnityEngine;
using FMODUnity;

namespace AlterunaFPS
{
	public class GunAudio : MonoBehaviour
	{
		public StudioEventEmitter ReloadSfx;
		public StudioEventEmitter FireSfx;
		
		public void PlayReloadSfx() => ReloadSfx.Play();
		public void PlayFireSfx() => FireSfx.Play();
	}
}