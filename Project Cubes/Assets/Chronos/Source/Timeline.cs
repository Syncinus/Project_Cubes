using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// Determines what type of clock a timeline observes. 
	/// </summary>
	public enum TimelineMode
	{
		/// <summary>
		/// The timeline observes a LocalClock attached to the same GameObject.
		/// </summary>
		Local,

		/// <summary>
		/// The timeline observes a GlobalClock referenced by globalClockKey. 
		/// </summary>
		Global
	}

	/// <summary>
	/// A component that combines timing measurements from an observed LocalClock or GlobalClock and any AreaClock within which it is. This component should be attached to any GameObject that should be affected by Chronos. 
	/// </summary>
	[AddComponentMenu("Time/Timeline")]
	[DisallowMultipleComponent]
	[HelpURL("http://ludiq.io/chronos/documentation#Timeline")]
	public class Timeline : MonoBehaviour
	{
		protected internal const float DefaultRecordingDuration = 30;
		protected internal const float DefaultRecordingInterval = 0.5f;

		public Timeline()
		{
			areaClocks = new HashSet<IAreaClock>();
			occurrences = new HashSet<Occurrence>();
			handledOccurrences = new HashSet<Occurrence>();
			previousDeltaTimes = new Queue<float>();
			timeScale = lastTimeScale = 1;

			animation = new AnimationTimeline(this);
			animator = new AnimatorTimeline(this);
			audioSource = new AudioSourceTimeline(this);
			navMeshAgent = new NavMeshAgentTimeline(this);
			rigidbody = new RigidbodyTimeline3D(this);
			rigidbody2D = new RigidbodyTimeline2D(this);
			transform = new TransformTimeline(this);
			windZone = new WindZoneTimeline(this);
		}

		protected virtual void Awake()
		{
			CacheComponents();
		}

		protected virtual void Start()
		{
			timeScale = lastTimeScale = clock.timeScale;

			foreach (IComponentTimeline component in activeComponents)
			{
				component.AdjustProperties();
				component.Start();
			}
		}

		protected virtual void Update()
		{
			TriggerEvents();

			lastTimeScale = timeScale;

			timeScale = clock.timeScale; // Start with the time scale from local / global clock

			foreach (IAreaClock areaClock in areaClocks) // Blend it with the time scale of each area clock
			{
				if (areaClock != null)
				{
					float areaClockTimeScale = areaClock.TimeScale(this);

					if (areaClock.innerBlend == ClockBlend.Multiplicative)
					{
						timeScale *= areaClockTimeScale;
					}
					else // if (areaClock.innerBlend == ClockBlend.Additive)
					{
						timeScale += areaClockTimeScale;
					}
				}
			}

			if (timeScale != lastTimeScale)
			{
				foreach (IComponentTimeline component in activeComponents)
				{
					component.AdjustProperties();
				}
			}

			float unscaledDeltaTime = Timekeeper.unscaledDeltaTime;
			deltaTime = unscaledDeltaTime * timeScale;
			fixedDeltaTime = Time.fixedDeltaTime * timeScale;
			time += deltaTime;
			unscaledTime += unscaledDeltaTime;

			RecordSmoothing();

			foreach (IComponentTimeline component in activeComponents)
			{
				component.Update();
			}

			if (timeScale > 0)
			{
				TriggerForwardOccurrences();
			}
			else if (timeScale < 0)
			{
				TriggerBackwardOccurrences();
			}
		}

		protected virtual void FixedUpdate()
		{
			foreach (IComponentTimeline component in activeComponents)
			{
				component.FixedUpdate();
			}
		}

		#region Fields

		protected internal float lastTimeScale;
		protected Queue<float> previousDeltaTimes;
		protected HashSet<Occurrence> occurrences;
		protected HashSet<Occurrence> handledOccurrences;
		protected Occurrence nextForwardOccurrence;
		protected Occurrence nextBackwardOccurrence;
		protected internal HashSet<IAreaClock> areaClocks;

		#endregion

		#region Properties

		[SerializeField]
		private TimelineMode _mode;

		/// <summary>
		/// Determines what type of clock the timeline observes. 
		/// </summary>
		public TimelineMode mode
		{
			get { return _mode; }
			set
			{
				_mode = value;
				_clock = null;
			}
		}

		[SerializeField, GlobalClock]
		private string _globalClockKey;

		/// <summary>
		/// The key of the GlobalClock that is observed by the timeline. This value is only used for the Global mode. 
		/// </summary>
		public string globalClockKey
		{
			get { return _globalClockKey; }
			set
			{
				_globalClockKey = value;
				_clock = null;
			}
		}

		private Clock _clock;

		/// <summary>
		/// The clock observed by the timeline. 
		/// </summary>
		public Clock clock
		{
			get
			{
				if (_clock == null)
				{
					_clock = FindClock();
				}

				return _clock;
			}
		}

		/// <summary>
		/// The time scale of the timeline, computed from all observed clocks. For more information, see Clock.timeScale. 
		/// </summary>
		public float timeScale { get; protected set; }

		/// <summary>
		/// The delta time of the timeline, computed from all observed clocks. For more information, see Clock.deltaTime. 
		/// </summary>
		public float deltaTime { get; protected set; }

		/// <summary>
		/// The fixed delta time of the timeline, computed from all observed clocks. For more information, see Clock.fixedDeltaTime. 
		/// </summary>
		public float fixedDeltaTime { get; protected set; }

		/// <summary>
		/// A smoothed out delta time. Use this value if you need to avoid spikes and fluctuations in delta times. The amount of frames over which this value is smoothed can be adjusted via smoothingDeltas. 
		/// </summary>
		public float smoothDeltaTime
		{
			get { return (deltaTime + previousDeltaTimes.Sum()) / (previousDeltaTimes.Count + 1); }
		}

		/// <summary>
		/// The amount of frames over which smoothDeltaTime is smoothed. 
		/// </summary>
		public static int smoothingDeltas = 5;

		/// <summary>
		/// The time in seconds since the creation of this timeline, computed from all observed clocks. For more information, see Clock.time. 
		/// </summary>
		public float time { get; protected internal set; }

		/// <summary>
		/// The unscaled time in seconds since the creation of this timeline. For more information, see Clock.unscaledTime. 
		/// </summary>
		public float unscaledTime { get; protected set; }

		/// <summary>
		/// Indicates the state of the timeline. 
		/// </summary>
		public TimeState state
		{
			get { return Timekeeper.GetTimeState(timeScale); }
		}

		[SerializeField]
		private float _recordingDuration = DefaultRecordingDuration;

		/// <summary>
		/// The maximum duration in seconds during which snapshots will be recorded. Higher values offer more rewind time but require more memory. 
		/// </summary>
		public float recordingDuration
		{
			get { return _recordingDuration; }
			protected set { _recordingDuration = value; }
		}

		[SerializeField]
		private float _recordingInterval = DefaultRecordingInterval;

		/// <summary>
		/// The interval in seconds at which snapshots will be recorder. Lower values offer more rewind precision but require more memory. 
		/// </summary>
		public float recordingInterval
		{
			get { return _recordingInterval; }
			protected set { _recordingInterval = value; }
		}

		[SerializeField]
		private bool _recordTransform = true;

		/// <summary>
		/// Determines whether the timeline should record the transform (and physics, if the GameObject has a rigidbody).
		/// </summary>
		public bool recordTransform
		{
			get { return _recordTransform; }
			set
			{
				_recordTransform = value;
				CacheComponents();
			}
		}

		[SerializeField]
		private bool _rewindableParticleSystem = true;

		/// <summary>
		/// Determines whether the timeline should resimulate the particle system to enable rewind support.
		/// </summary>
		public bool rewindableParticleSystem
		{
			get { return _rewindableParticleSystem; }
			set
			{
				_rewindableParticleSystem = value;

				if (particleSystem != null)
				{
					particleSystem = null;
					CacheComponents();
				}
			}
		}

		#endregion

		#region Timing

		protected virtual Clock FindClock()
		{
			if (mode == TimelineMode.Local)
			{
				LocalClock localClock = GetComponent<LocalClock>();

				if (localClock == null)
				{
					throw new ChronosException(string.Format("Missing local clock for timeline."));
				}

				return localClock;
			}
			else if (mode == TimelineMode.Global)
			{
				GlobalClock oldGlobalClock = _clock as GlobalClock;

				if (oldGlobalClock != null)
				{
					oldGlobalClock.Unregister(this);
				}

				if (!Timekeeper.instance.HasClock(globalClockKey))
				{
					throw new ChronosException(string.Format("Missing global clock for timeline: '{0}'.", globalClockKey));
				}

				GlobalClock globalClock = Timekeeper.instance.Clock(globalClockKey);

				globalClock.Register(this);

				return globalClock;
			}
			else
			{
				throw new ChronosException(string.Format("Unknown timeline mode: '{0}'.", mode));
			}
		}

		/// <summary>
		/// Releases the timeline from the specified area clock's effects. 
		/// </summary>
		public virtual void ReleaseFrom(IAreaClock areaClock)
		{
			areaClock.Release(this);
		}

		/// <summary>
		/// Releases the timeline from the effects of all the area clocks within which it is. 
		/// </summary>
		public virtual void ReleaseFromAll()
		{
			foreach (IAreaClock areaClock in areaClocks.Where(ac => ac != null).ToArray())
			{
				areaClock.Release(this);
			}

			areaClocks.Clear();
		}

		protected virtual void TriggerEvents()
		{
			if (lastTimeScale != 0 && timeScale == 0)
			{
				SendMessage("OnStartPause", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale == 0 && timeScale != 0)
			{
				SendMessage("OnStopPause", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale >= 0 && timeScale < 0)
			{
				SendMessage("OnStartRewind", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale < 0 && timeScale >= 0)
			{
				SendMessage("OnStopRewind", SendMessageOptions.DontRequireReceiver);
			}

			if ((lastTimeScale <= 0 || lastTimeScale >= 1) && (timeScale > 0 && timeScale < 1))
			{
				SendMessage("OnStartSlowDown", SendMessageOptions.DontRequireReceiver);
			}

			if ((lastTimeScale > 0 && lastTimeScale < 1) && (timeScale <= 0 || timeScale >= 1))
			{
				SendMessage("OnStopSlowDown", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale <= 1 && timeScale > 1)
			{
				SendMessage("OnStartFastForward", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale > 1 && timeScale <= 1)
			{
				SendMessage("OnStopFastForward", SendMessageOptions.DontRequireReceiver);
			}
		}

		protected virtual void RecordSmoothing()
		{
			if (deltaTime != 0)
			{
				previousDeltaTimes.Enqueue(deltaTime);
			}

			if (previousDeltaTimes.Count > smoothingDeltas)
			{
				previousDeltaTimes.Dequeue();
			}
		}

		#endregion

		#region Occurrences

		protected void TriggerForwardOccurrences()
		{
			handledOccurrences.Clear();

			while (nextForwardOccurrence != null && nextForwardOccurrence.time <= time)
			{
				nextForwardOccurrence.Forward();

				handledOccurrences.Add(nextForwardOccurrence);

				nextBackwardOccurrence = nextForwardOccurrence;

				nextForwardOccurrence = OccurrenceAfter(nextForwardOccurrence.time, handledOccurrences);
			}
		}

		protected void TriggerBackwardOccurrences()
		{
			handledOccurrences.Clear();

			while (nextBackwardOccurrence != null && nextBackwardOccurrence.time >= time)
			{
				nextBackwardOccurrence.Backward();

				if (nextBackwardOccurrence.repeatable)
				{
					handledOccurrences.Add(nextBackwardOccurrence);

					nextForwardOccurrence = nextBackwardOccurrence;
				}
				else
				{
					occurrences.Remove(nextBackwardOccurrence);
				}

				nextBackwardOccurrence = OccurrenceBefore(nextBackwardOccurrence.time, handledOccurrences);
			}
		}

		protected Occurrence OccurrenceAfter(float time, params Occurrence[] ignored)
		{
			return OccurrenceAfter(time, (IEnumerable<Occurrence>) ignored);
		}

		protected Occurrence OccurrenceAfter(float time, IEnumerable<Occurrence> ignored)
		{
			Occurrence after = null;

			foreach (Occurrence occurrence in occurrences)
			{
				if (occurrence.time >= time &&
				    !ignored.Contains(occurrence) &&
				    (after == null || occurrence.time < after.time))
				{
					after = occurrence;
				}
			}

			return after;
		}

		protected Occurrence OccurrenceBefore(float time, params Occurrence[] ignored)
		{
			return OccurrenceBefore(time, (IEnumerable<Occurrence>) ignored);
		}

		protected Occurrence OccurrenceBefore(float time, IEnumerable<Occurrence> ignored)
		{
			Occurrence before = null;

			foreach (Occurrence occurrence in occurrences)
			{
				if (occurrence.time <= time &&
				    !ignored.Contains(occurrence) &&
				    (before == null || occurrence.time > before.time))
				{
					before = occurrence;
				}
			}

			return before;
		}

		protected virtual void PlaceOccurence(Occurrence occurrence, float time)
		{
			if (time == this.time)
			{
				if (timeScale >= 0)
				{
					occurrence.Forward();
					nextBackwardOccurrence = occurrence;
				}
				else
				{
					occurrence.Backward();
					nextForwardOccurrence = occurrence;
				}
			}
			else if (time > this.time)
			{
				if (nextForwardOccurrence == null ||
				    nextForwardOccurrence.time > time)
				{
					nextForwardOccurrence = occurrence;
				}
			}
			else if (time < this.time)
			{
				if (nextBackwardOccurrence == null ||
				    nextBackwardOccurrence.time < time)
				{
					nextBackwardOccurrence = occurrence;
				}
			}
		}

		/// <summary>
		/// Schedules an occurrence at a specified absolute time in seconds on the timeline. 
		/// </param>
		public virtual Occurrence Schedule(float time, bool repeatable, Occurrence occurrence)
		{
			occurrence.time = time;
			occurrence.repeatable = repeatable;
			occurrences.Add(occurrence);
			PlaceOccurence(occurrence, time);
			return occurrence;
		}

		/// <summary>
		/// Executes an occurrence now and places it on the schedule for rewinding. 
		/// </summary>
		public Occurrence Do(bool repeatable, Occurrence occurrence)
		{
			return Schedule(time, repeatable, occurrence);
		}

		/// <summary>
		/// Plans an occurrence to be executed in the specified delay in seconds. 
		/// </summary>
		public Occurrence Plan(float delay, bool repeatable, Occurrence occurrence)
		{
			if (delay <= 0)
			{
				throw new ChronosException("Planned occurrences must be in the future.");
			}

			return Schedule(time + delay, repeatable, occurrence);
		}

		/// <summary>
		/// Creates a "memory" of an occurrence at a specified "past-delay" in seconds. This means that the occurrence will only be executed if time is rewound, and that it will be executed backward first. 
		/// </summary>
		public Occurrence Memory(float delay, bool repeatable, Occurrence occurrence)
		{
			if (delay >= 0)
			{
				throw new ChronosException("Memory occurrences must be in the past.");
			}

			return Schedule(time - delay, repeatable, occurrence);
		}

		public Occurrence Schedule<T>(float time, bool repeatable, ForwardAction<T> forward, BackwardAction<T> backward)
		{
			return Schedule(time, repeatable, new DelegateOccurrence<T>(forward, backward));
		}

		public Occurrence Do<T>(bool repeatable, ForwardAction<T> forward, BackwardAction<T> backward)
		{
			return Do(repeatable, new DelegateOccurrence<T>(forward, backward));
		}

		public Occurrence Plan<T>(float delay, bool repeatable, ForwardAction<T> forward, BackwardAction<T> backward)
		{
			return Plan(delay, repeatable, new DelegateOccurrence<T>(forward, backward));
		}

		public Occurrence Memory<T>(float delay, bool repeatable, ForwardAction<T> forward, BackwardAction<T> backward)
		{
			return Memory(delay, repeatable, new DelegateOccurrence<T>(forward, backward));
		}

		public Occurrence Schedule(float time, ForwardOnlyAction forward)
		{
			return Schedule(time, false, new ForwardDelegateOccurrence(forward));
		}

		public Occurrence Plan(float delay, ForwardOnlyAction forward)
		{
			return Plan(delay, false, new ForwardDelegateOccurrence(forward));
		}

		public Occurrence Memory(float delay, ForwardOnlyAction forward)
		{
			return Memory(delay, false, new ForwardDelegateOccurrence(forward));
		}

		/// <summary>
		/// Removes the specified occurrence from the timeline. 
		/// </summary>
		public void Cancel(Occurrence occurrence)
		{
			if (!occurrences.Contains(occurrence))
			{
				throw new ChronosException("Occurrence to cancel not found on timeline.");
			}
			else
			{
				if (occurrence == nextForwardOccurrence)
				{
					nextForwardOccurrence = OccurrenceAfter(occurrence.time, occurrence);
				}

				if (occurrence == nextBackwardOccurrence)
				{
					nextBackwardOccurrence = OccurrenceBefore(occurrence.time, occurrence);
				}

				occurrences.Remove(occurrence);
			}
		}

		/// <summary>
		/// Removes the specified occurrence from the timeline and returns true if it is found. Otherwise, returns false. 
		/// </summary>
		public bool TryCancel(Occurrence occurrence)
		{
			if (!occurrences.Contains(occurrence))
			{
				return false;
			}
			else
			{
				Cancel(occurrence);
				return true;
			}
		}

		/// <summary>
		/// Change the absolute time in seconds of the specified occurrence on the timeline.
		/// </summary>
		public void Reschedule(Occurrence occurrence, float time)
		{
			occurrence.time = time;
			PlaceOccurence(occurrence, time);
		}

		/// <summary>
		/// Moves the specified occurrence forward on the timeline by the specified delay in seconds.
		/// </summary>
		public void Postpone(Occurrence occurrence, float delay)
		{
			Reschedule(occurrence, time + delay);
		}

		/// <summary>
		/// Moves the specified occurrence backward on the timeline by the specified delay in seconds.
		/// </summary>
		public void Prepone(Occurrence occurrence, float delay)
		{
			Reschedule(occurrence, time - delay);
		}

		#endregion

		#region Coroutines

		/// <summary>
		/// Suspends the coroutine execution for the given amount of seconds. This method should only be used with a yield statement in coroutines. 
		/// </summary>
		public Coroutine WaitForSeconds(float seconds)
		{
			return StartCoroutine(WaitingForSeconds(seconds));
		}

		protected IEnumerator WaitingForSeconds(float seconds)
		{
			float start = time;

			while (time < start + seconds)
			{
				yield return null;
			}
		}

		#endregion

		#region Components

		public new AnimationTimeline animation { get; protected set; }
		public AnimatorTimeline animator { get; protected set; }
		public AudioSourceTimeline audioSource { get; protected set; }
		public NavMeshAgentTimeline navMeshAgent { get; protected set; }
		public new IParticleSystemTimeline particleSystem { get; protected set; }
		public new RigidbodyTimeline3D rigidbody { get; protected set; }
		public new RigidbodyTimeline2D rigidbody2D { get; protected set; }
		public new TransformTimeline transform { get; protected set; }
		public WindZoneTimeline windZone { get; protected set; }

		protected IEnumerable<IComponentTimeline> activeComponents
		{
			get
			{
				if (animation.component != null) yield return animation;
				if (animator.component != null) yield return animator;
				if (audioSource.component != null) yield return audioSource;
				if (navMeshAgent.component != null) yield return navMeshAgent;
				if (particleSystem.component != null) yield return particleSystem;
				if (rigidbody.component != null) yield return rigidbody;
				if (rigidbody2D.component != null) yield return rigidbody2D;
				if (transform.component != null) yield return transform;
				if (windZone.component != null) yield return windZone;
			}
		}

		protected IEnumerable<IRecorder> activeRecorders
		{
			get
			{
				if (rigidbody.component != null) yield return rigidbody;
				if (rigidbody2D.component != null) yield return rigidbody2D;
				if (transform.component != null) yield return transform;
			}
		}

		/// <summary>
		/// The components used by the timeline are cached for performance optimization. If you add or remove built-in Unity components on the GameObject, you need to call this method to update the timeline accordingly. 
		/// </summary>
		public virtual void CacheComponents()
		{
			if (particleSystem == null)
			{
				if (rewindableParticleSystem)
				{
					particleSystem = new RewindableParticleSystemTimeline(this);
				}
				else
				{
					particleSystem = new NonRewindableParticleSystemTimeline(this);
				}
			}

			animator.Cache(GetComponent<Animator>());
			animation.Cache(GetComponent<Animation>());
			audioSource.Cache(GetComponent<AudioSource>());
			navMeshAgent.Cache(GetComponent<UnityEngine.AI.NavMeshAgent>());
			particleSystem.Cache(GetComponent<ParticleSystem>());
			windZone.Cache(GetComponent<WindZone>());

			// Only activate one of Rigidbody / Rigidbody2D / Transform timelines at once

			if (rigidbody.Cache(GetComponent<Rigidbody>()))
			{
				rigidbody2D.Cache(null);
				transform.Cache(null);
			}
			else if (rigidbody2D.Cache(GetComponent<Rigidbody2D>()))
			{
				rigidbody.Cache(null);
				transform.Cache(null);
			}
			else if (transform.Cache(GetComponent<Transform>()))
			{
				rigidbody.Cache(null);
				rigidbody2D.Cache(null);
			}
		}

		/// <summary>
		/// Sets the recording duration and interval in seconds. This will reset the saved snapshots.
		/// </summary>
		public void SetRecording(float duration, float interval)
		{
			recordingDuration = duration;
			recordingInterval = interval;

			ResetRecording();
		}

		/// <summary>
		/// Resets the saved snapshots. 
		/// </summary>
		public void ResetRecording()
		{
			foreach (IRecorder recorder in activeRecorders)
			{
				recorder.Reset();
			}
		}

		/// <summary>
		/// Estimate the memory usage in bytes from the storage of snapshots for the current recording duration and interval. 
		/// </summary>
		public int EstimateMemoryUsage()
		{
			if (Application.isPlaying)
			{
				return activeRecorders.Sum(r => r.EstimateMemoryUsage());
			}
			else
			{
				if (!recordTransform)
				{
					return 0;
				}

				if (GetComponent<Rigidbody>() != null)
				{
					return RigidbodyTimeline3D.EstimateMemoryUsage(recordingDuration, recordingInterval);
				}
				else if (GetComponent<Rigidbody2D>() != null)
				{
					return RigidbodyTimeline2D.EstimateMemoryUsage(recordingDuration, recordingInterval);
				}
				else if (GetComponent<Transform>() != null)
				{
					return TransformTimeline.EstimateMemoryUsage(recordingDuration, recordingInterval);
				}
				else
				{
					return 0;
				}
			}
		}

		#endregion
	}
}
