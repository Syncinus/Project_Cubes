namespace Chronos
{
	/// <summary>
	/// An occurrence action that is executed when time goes forward. Returns any object that will be transfered to the backward action if time is rewinded.
	/// </summary>
	public delegate T ForwardAction<T>();

	/// <summary>
	/// An occurrence action that is executed when time goes backward. Uses the object returned by the forward action to remember state information.
	/// </summary>
	public delegate void BackwardAction<T>(T transfer);

	internal sealed class DelegateOccurrence<T> : Occurrence
	{
		private ForwardAction<T> forward { get; set; }
		private BackwardAction<T> backward { get; set; }
		private T transfer { get; set; }

		public DelegateOccurrence(ForwardAction<T> forward, BackwardAction<T> backward)
		{
			this.forward = forward;
			this.backward = backward;
		}

		public override void Forward()
		{
			transfer = forward();
		}

		public override void Backward()
		{
			backward(transfer);
		}
	}
}
