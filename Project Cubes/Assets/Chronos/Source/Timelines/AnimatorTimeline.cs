using UnityEngine;

namespace Chronos
{
	public class AnimatorTimeline : ComponentTimeline<Animator>
	{
		public AnimatorTimeline(Timeline timeline) : base(timeline) { }

		protected internal const int DefaultRecordedFrames = 0;

		private float _speed;

		/// <summary>
		/// The speed that is applied to the animator before time effects. Use this property instead of Animator.speed, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float speed
		{
			get { return _speed; }
			set
			{
				_speed = value;
				AdjustProperties();
			}
		}

		private int _recordedFrames = DefaultRecordedFrames;

		/// <summary>
		/// The maximum amount of frames (updates) that will be recorded. Higher values offer more rewind time but require more memory. A value of zero indicates an indefinite amount of frames.
		/// </summary>
		public int recordedFrames
		{
			get { return _recordedFrames; }
			set { _recordedFrames = Mathf.Clamp(value, 0, 10000); }
		}

		public override void CopyProperties(Animator source)
		{
			speed = source.speed;
		}

		public override void AdjustProperties(float timeScale)
		{
			if (timeScale >= 0)
			{
				component.speed = speed * timeScale;
			}
			else
			{
				component.speed = 0;
			}
		}

		public override void Start()
		{
			component.StartRecording(recordedFrames);
		}

		public override void Update()
		{
			float timeScale = timeline.timeScale;
			float lastTimeScale = timeline.lastTimeScale;

			if (lastTimeScale >= 0 && timeScale < 0) // Started rewind
			{
				component.StopRecording();

				// There seems to be a bug in some cases in which no data is recorded
				// and recorder start and stop time are at -1. Can't seem to figure
				// when or why it happens, though. Temporary hotfix to disable playback
				// in that case.
				if (component.recorderStartTime >= 0)
				{
					component.StartPlayback();
					component.playbackTime = component.recorderStopTime;
				}
				else
				{
					Debug.LogWarning("Animator timeline failed to record for unknown reasons.\nSee: http://forum.unity3d.com/threads/341203/", component);
				}
			}
			else if (lastTimeScale <= 0 && timeScale > 0) // Stopped pause or rewind
			{
				component.StopPlayback();
				component.StartRecording(recordedFrames);
			}

			if (timeScale < 0 && component.recorderMode == AnimatorRecorderMode.Playback)
			{
				float playbackTime = Mathf.Max(component.recorderStartTime, component.playbackTime + timeline.deltaTime);

				component.playbackTime = playbackTime;
			}
		}
	}
}
