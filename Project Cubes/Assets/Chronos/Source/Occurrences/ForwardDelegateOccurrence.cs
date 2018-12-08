using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// An occurrence action that is only executed when time goes forward. Does not return any transferable state object that could be used to revert its effect.
	/// </summary>
	public delegate void ForwardOnlyAction();

	internal sealed class ForwardDelegateOccurrence : Occurrence
	{
		private ForwardOnlyAction forward { get; set; }

		public ForwardDelegateOccurrence(ForwardOnlyAction forward)
		{
			this.forward = forward;
		}

		public override void Forward()
		{
			forward();
		}

		public override void Backward()
		{
			Debug.LogWarning("Trying to revert a forward-only occurrence.");
		}
	}
}
