using System;

namespace InstaFeed
{
	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(string innerText)
		{
			Message = innerText;
		}

		public string Message { get; private set; }
	}
}