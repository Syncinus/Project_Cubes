using UnityEngine;

namespace Chronos
{
	public class WindZoneTimeline : ComponentTimeline<WindZone>
	{
		/// <summary>
		/// The wind that is applied to the wind zone before time effects. Use this property instead of WindZone.windMain, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windMain { get; set; }

		/// <summary>
		/// The turbulence that is applied to the wind zone before time effects. Use this property instead of WindZone.windTurbulence, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windTurbulence { get; set; }

		/// <summary>
		/// The pulse magnitude that is applied to the wind zone before time effects. Use this property instead of WindZone.windPulseMagnitude, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windPulseMagnitude { get; set; }

		/// <summary>
		/// The pulse frquency that is applied to the wind zone before time effects. Use this property instead of WindZone.windPulseFrequency, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windPulseFrequency { get; set; }

		public WindZoneTimeline(Timeline timeline) : base(timeline) { }

		public override void CopyProperties(WindZone source)
		{
			windMain = source.windMain;
			windTurbulence = source.windTurbulence;
			windPulseFrequency = source.windPulseFrequency;
			windPulseMagnitude = source.windPulseMagnitude;
		}

		public override void AdjustProperties(float timeScale)
		{
			component.windTurbulence = windTurbulence * timeScale * Mathf.Abs(timeScale);
			component.windPulseFrequency = windPulseFrequency * timeScale;
			component.windPulseMagnitude = windPulseMagnitude * Mathf.Sign(timeScale);
		}
	}
}
