using UnityEngine;

namespace Chronos.Example
{
	// A basic example for Chronos with deltaTime movement.
	// In this case, rotate the game object in a framerate independant
	// manner that takes into consideration all the time scales.
	public class ExampleSpin : ExampleBaseBehaviour
	{
		// The speed at which to rotate
		public float speed = 20;

		private void Update()
		{
			// Notice time.deltaTime (GetComponent<Timeline>().deltaTime)
			// instead of Time.deltaTime! Lowercase t is important here.
			// Remember "time" is defined in ExampleBaseBehaviour.
			transform.Rotate(time.deltaTime * Vector3.one * speed);
		}
	}
}
