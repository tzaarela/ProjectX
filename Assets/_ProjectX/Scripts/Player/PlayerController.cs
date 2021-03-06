using Mirror;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Data.ScriptableObjects;
using Managers;
using UnityEngine;
using Game.Flag;
using UnityEngine.InputSystem;
using Data.Containers;
using FMOD.Studio;

namespace Player
{
	public class PlayerController : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal, IReceiveDamageAOE
	{
		[Header("References")]
		[SerializeField] private GameObject flagOnRoof;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private TMPro.TextMeshProUGUI playerNameText;
		[SerializeField] private SO_CarMaterials carMaterials;

		[Header("DeathSettings")]
		[SerializeField] private ParticleSystem deathFX;
		[SerializeField] private ParticleSystem deathFX2;
		[SerializeField] private ParticleSystem deathSmoke;
		public float upsideDownTimerBeforeDeath = 3f;
		public float upsideDownMinimumVelocityBeforeDeath = 10f;


		[Header("Debug")]
		[SyncVar(hook = nameof(FlagStateChanged))]
		public bool hasFlag;

		[SyncVar(hook = nameof(PlayerNameChanged))]
		public string playerName;

		[SyncVar(hook = nameof(PlayerMaterialIndex))]
		public int playerMaterialIndex;


		[HideInInspector]
		public Rigidbody rb;
		
		private Flag flag;
		
		private InputManager inputManager;
		private Health health;
		private PlayerSound playerSound;
		private float upsideDownTimeStep = 0;

		private int playerId;
		public int PlayerId => playerId;

		public bool localPlayer;
		
		[SerializeField]
		[FMODUnity.EventRef]
		private string crashSound;
		private FMOD.Studio.EventInstance crashSoundInstance;
		
		[SerializeField]
		[FMODUnity.EventRef]
		private string hornSound;
		private FMOD.Studio.EventInstance hornSoundInstance;

		private void Awake()
		{
			inputManager = GetComponent<InputManager>();
			health = GetComponent<Health>();
			playerSound = GetComponent<PlayerSound>();

			crashSoundInstance = FMODUnity.RuntimeManager.CreateInstance(crashSound);
			crashSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, rb));
			
			hornSoundInstance = FMODUnity.RuntimeManager.CreateInstance(hornSound);
			hornSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, rb));
		}

		[Server]
		public override void OnStartServer()
		{
			rb = GetComponent<Rigidbody>();
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnStartClient()
		{
			if (!isLocalPlayer)
				return;
			
			inputManager.playerControls.Player.HonkHorn.performed += InputPlayHornSound;
			
			rb = GetComponent<Rigidbody>();
			localPlayer = true;
			playerNameText.gameObject.SetActive(false);
			playerId = (int)GetComponent<NetworkIdentity>().netId;
			CmdUpdateActivePlayersList();
			name += "-local";
			SendGlobal(GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME, new GameObjectData(gameObject));
		}
		
		private void Update()
		{
			if (!isLocalPlayer)
				return;


			//is the roof touching the city-ground?
			if (!health.IsDead && Physics.Raycast(transform.position + transform.up, transform.up, 1, 1 << 9) && rb.velocity.sqrMagnitude < upsideDownMinimumVelocityBeforeDeath)
			{
				upsideDownTimeStep += Time.deltaTime;
				if (upsideDownTimeStep > upsideDownTimerBeforeDeath)
					CmdAccident();
			}
			else
			{
				upsideDownTimeStep = 0;
			}

			// DEBUG: F-Key resets car-rotation (when turned over)
			if (Keyboard.current.rKey.wasPressedThisFrame)
			{
				CmdFlipCar();
			}
			
			// DEBUG: I-Key = InstantDeath
			if (Keyboard.current.iKey.wasPressedThisFrame)
			{
				CmdAccident();
			}
			
			// DEBUG: L-Key = Lower Health by 25
			if (Keyboard.current.lKey.wasPressedThisFrame)
			{
				CmdLowerHealthByTwentyFive();
			}
		}

		[Server]
		public void TakeFlag(Flag flag)
		{
			this.flag = flag;
			hasFlag = true;
			SendGlobal(GlobalEvent.FLAG_TAKEN, new GameObjectData(gameObject));
		}

		[ContextMenu("Drop Flag")]
		[Server]
		public void DropFlag()
		{
			if (!hasFlag)
				return;

			hasFlag = false;
			flag.Drop(transform.position, rb.velocity);
			SendGlobal(GlobalEvent.FLAG_DROPPED, new GameObjectData(gameObject));
		}

		//SyncVar Hook
		[Client]
		private void FlagStateChanged(bool oldValue, bool newValue)
		{
			flagOnRoof.SetActive(newValue);
		}

		//SyncVar Hook
		[Client]
		private void PlayerNameChanged(string oldValue, string newValue)
		{
			playerNameText.text = newValue;
		}

		// SyncVar Hook
		 [Client]
		 private void PlayerMaterialIndex(int oldValue, int newValue)
		 {
			 meshRenderer.material = carMaterials.GetMaterial(newValue);
		 }

		[Command]
		private void CmdUpdateActivePlayersList()
		{
			ServiceLocator.RoundManager.AddActivePlayer(this);
		}

		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}

		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			switch (eventState)
			{
				case GlobalEvent.END_OF_COUNTDOWN:
					RpcEnableAllPlayersInput();
					break;
				
				case GlobalEvent.END_GAMESTATE:
					RpcSetPlayerEndState();
					break;
			}
		}

		[ClientRpc]
		private void RpcEnableAllPlayersInput()
		{
			if (!isLocalPlayer)
				return;

			playerSound.PlayEmitter();
			inputManager.EnableInput();
		}

		[ClientRpc]
		private void RpcSetPlayerEndState()
		{
			inputManager.DisableInput();
			GetComponent<DriveController>().enabled = false;
			playerSound.StopEmitter();

			if (!isServer)
				return;
			
			rb.velocity = Vector3.zero;
		}

		[Client]
		public void Death()
		{
			FMODUnity.RuntimeManager.PlayOneShot("event:/Vehicles/DeathExplosion", Camera.main.transform.position);
			deathFX.Play();
			deathFX2.Play();
			deathSmoke.Play();
			meshRenderer.material.color = Color.black;
			playerSound.StopEmitter();
			playerSound.StopDriftSound();
			
			if (!isLocalPlayer)
				return;
			
			inputManager.DisableInput();
		}

		[Command]
		public void CmdRespawnPlayer()
		{
			health.ResetCurrentHealth();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			
			//rb.AddForce(Vector3.up * 5000, ForceMode.Impulse);
			GetComponent<PowerupController>().Drop();
			ServiceLocator.RespawnManager.RespawnPlayer(transform);
			RpcRespawnPlayer();
		}
		
		[ClientRpc]
		private void RpcRespawnPlayer()
		{
			deathSmoke.Stop();
			meshRenderer.material.color = Color.white;
			playerSound.PlayEmitter();

			if (!isLocalPlayer)
				return;

			inputManager.EnableInput();
		}
		
		[Server]
		private void FlipCar()
		{
			Vector3 newRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
			transform.rotation = Quaternion.Euler(newRotation);
			
			rb.AddForce(Vector3.up * 5000, ForceMode.Impulse);
			rb.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 5000, ForceMode.Impulse);

			if (transform.position.y > 2f)
			{
				transform.position = new Vector3(transform.position.x,2f, transform.position.z);
				rb.velocity = Vector3.zero;
			}
		}

		[Command]
		private void CmdAccident()
		{
			DropFlag();
			health.SetHealthToZero();
			ServiceLocator.HudManager.TargetActivateDeathTexts(connectionToClient, "accident");
		}

		// TEMP!
		[Command]
		private void CmdFlipCar()
		{
			//rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			FlipCar();
		}

		// TEMP
		[Command]
		private void CmdLowerHealthByTwentyFive()
		{
			health.LowerHealthByTwentyFive();
		}
		
		public void ReceiveDamageAOE(AoeData aoeData)
		{
			rb.AddForce(aoeData.direction * aoeData.distance + Vector3.up * aoeData.upwardEffect, ForceMode.Impulse);
			
			if (aoeData.shouldRotate)
				rb.AddRelativeTorque(aoeData.direction * aoeData.distance * aoeData.rotationEffect, ForceMode.Impulse);
			
			health.ReceiveDamage(50, aoeData.spawnedById);
		}

		private void OnCollisionEnter(Collision other)
		{
			if (!isServer)
				return;
			
			if (other.transform.CompareTag("Obstacle"))
			{
				// if (rb.velocity.magnitude > 0.1f)
				// 	return;
				
				//Vector3.Dot(other.contacts[0].normal,other.relativeVelocity) * rb.mass

				if (other.impulse.magnitude < 500f)
					return;
				
				//Debug.Log("FORCE MAG: " + other.impulse.magnitude);

				Vector3 pushForce = Vector3.ClampMagnitude(other.impulse, 12000);
				
				ForcePush(pushForce);
			}
		}

		[Server]
		private void ForcePush(Vector3 force)
		{
			//Debug.Log("FORCE PUSH: " + force);
			Debug.DrawRay(transform.position, force, Color.red, 0.1f);
			rb.AddForce(force, ForceMode.Impulse);
			RpcPlayCrashSound();
		}
		
		[ClientRpc]
		private void RpcPlayCrashSound()
		{
			crashSoundInstance.stop(STOP_MODE.IMMEDIATE);
			crashSoundInstance.start();
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(crashSoundInstance, transform);
		}

		private void InputPlayHornSound(InputAction.CallbackContext obj)
		{
			CmdPlayHornSound();
		}
		
		[Command]
		private void CmdPlayHornSound()
		{
			RpcPlayHornSound();
		}
		
		[ClientRpc]
		private void RpcPlayHornSound()
		{
			hornSoundInstance.stop(STOP_MODE.ALLOWFADEOUT);
			hornSoundInstance.start();
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(hornSoundInstance, transform);
		}
	}
}