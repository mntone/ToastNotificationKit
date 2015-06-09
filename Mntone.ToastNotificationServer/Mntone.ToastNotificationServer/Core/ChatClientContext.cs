using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mntone.ToastNotificationServer.Core
{
	public sealed class ChatClientContext
	{
		private readonly Socket _clientSocket = null;

		private Thread _connectionThread = null;
		private int _enabled = 0;

		public ChatClientContext(ChatServer parent, Socket clientSocket)
		{
			this.Parent = parent;
			this._clientSocket = clientSocket;
		}

		public void Start()
		{
			if (Interlocked.CompareExchange(ref this._enabled, 0, 0) != 0)
			{
				// already started
				return;
			}

			Interlocked.Increment(ref this._enabled);
			this._connectionThread = new Thread(() =>
			{
				try
				{
					using (var stream = new NetworkStream(this._clientSocket))
					using (var reader = new StreamReader(stream))
					{
						while (true)
						{
							if (Interlocked.CompareExchange(ref this._enabled, 0, 0) == 0)
							{
								// expect to shutdown...
								break;
							}

							var text = reader.ReadLine();
							this.RaiseMessageReceived(text);
						}
					}
				}
				catch (Exception)
				{ }
				finally
				{
					// closing process
					this._clientSocket.Shutdown(SocketShutdown.Both);
					this._clientSocket.Close();
					this._clientSocket.Dispose();

					this.RaiseClientClosed();

					if (Interlocked.CompareExchange(ref this._enabled, 0, 0) != 0)
					{
						Interlocked.Decrement(ref this._enabled);
					}
				}
			});
			this._connectionThread.Start();
		}

		public Task CloseAsync()
		{
			if (Interlocked.CompareExchange(ref this._enabled, 0, 0) == 0)
			{
				// already stopped
				return Task.FromResult<object>(null);
			}

			return Task.Factory.StartNew(() =>
			{
				Interlocked.Decrement(ref this._enabled);
				while (this._connectionThread.IsAlive)
				{
					Task.Delay(1);
				}
			});
		}

		public event MessageReceivedEventHandler MessageReceived;
		private void RaiseMessageReceived(string jsonText)
		{
			Interlocked.CompareExchange(ref this.MessageReceived, null, null)?.Invoke(this, new MessageReceivedEventArgs(jsonText));
		}

		public event ClosedEventHandler ClientClosed;
		private void RaiseClientClosed()
		{
			Interlocked.CompareExchange(ref this.ClientClosed, null, null)?.Invoke(this, ClosedEventArgs.Empty);
		}

		public ChatServer Parent { get; }
	}
}