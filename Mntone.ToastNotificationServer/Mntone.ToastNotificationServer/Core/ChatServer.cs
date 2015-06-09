using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mntone.ToastNotificationServer.Core
{
	public sealed class ChatServer
	{
		public const int DEFAULT_SERVER_PORT = 4123;

		private readonly Socket _socket;
		private readonly List<ChatClientContext> _connectionContexts = null;

		private Thread _connectionThread = null;
		private int _enabled = 0;

		public ChatServer()
		{
			var endPointHost = new IPEndPoint(IPAddress.Loopback, DEFAULT_SERVER_PORT);
			this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this._socket.Bind(endPointHost);
			this._socket.Listen(5);
			this._socket.NoDelay = true;

			this._connectionContexts = new List<ChatClientContext>();
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
					  while (true)
					  {
						  if (Interlocked.CompareExchange(ref this._enabled, 0, 0) == 0)
						  {
							  // expect to shutdown...
							  break;
						  }

						  var clientSocket = this._socket.Accept();
						  var clientContext = new ChatClientContext(this, clientSocket);
						  this._connectionContexts.Add(clientContext);
						  this.RaiseClientConnected(clientContext);
						  clientContext.Start();
					  }
				  }
				  catch (Exception)
				  { }
				  finally
				  {
					  // closing process
					  this._connectionContexts.AsParallel().ForAll(ctx => ctx.CloseAsync().GetAwaiter().GetResult());

					  this._socket.Close();
					  this._socket.Dispose();

					  this.RaiseServerClosed();
				  }
			  });
			this._connectionThread.Start();
		}

		public Task ShutdownAsync()
		{
			return Task.Factory.StartNew(() =>
			{
				Interlocked.Decrement(ref this._enabled);
				while (this._connectionThread.IsAlive)
				{
					Task.Delay(1);
				}
			});
		}

		public event ClientConnectedEventHandler ClientConnected;
		private void RaiseClientConnected(ChatClientContext context)
		{
			Interlocked.CompareExchange(ref this.ClientConnected, null, null)?.Invoke(this, new ClientConnectedEventArgs(context));
		}

		public event ClosedEventHandler ServerClosed;
		private void RaiseServerClosed()
		{
			Interlocked.CompareExchange(ref this.ServerClosed, null, null)?.Invoke(this, ClosedEventArgs.Empty);
		}
	}
}
