# Toast Notification (WinRT) Kit

Toast Notification Kit is pushing chat to Windows Notification System (> Windows 8). Now developing….

## § Toast Notification Bridge Mod (Java 8)

### Features

- [x] Parse standard message.
- [ ] Parse dynmap message.
- [ ] Parse reply.
- [ ] Start up server.
- [ ] Change connection port.

### Support environment

- Java 8
- JDK 8


## § Toast Notification Server (C# 6.0)

### Features

- [x] Server can be connected by clients.
- [ ] Shutting down by user. (Now, stop with task manager)
- [ ] Change conection port.


### Support environment

- .NET 4.5 (Desktop)
- C# 6.0 (you **MUST HAVE VS 2015**)


## How to develop

### Toast Notification Bridge Mod

1. Open your command line.
2. Input `call gradlew setupDevWorkspace setupDecompWorkspace`.
	However, you can use IDE.
	IntelliJ IDEA: `call gradlew setupDevWorkspace setupDecompWorkspace idea`
	Eclipse: `call gradlew setupDevWorkspace setupDecompWorkspace eclipse`
3. Change the code with editor or IDE.

### Toast Notification Server

1. Install Visual Studio 2015 RC to your Windows.
2. Open project file, Mntone.ToastNotificationServer.sln.


## 開発方法 (Japanese)

### Toast Notification Bridge Mod

1. コマンドラインを開きます
2. `call gradlew setupDevWorkspace setupDecompWorkspace` と入力します
	ただし、IDE を使うこともできます。
	IntelliJ IDEA: `call gradlew setupDevWorkspace setupDecompWorkspace idea`
	Eclipse: `call gradlew setupDevWorkspace setupDecompWorkspace eclipse`
3. エディターや IDE でコードを変更します


### Toast Notification Server

1. Visual Studio 2015 RC をインストールします
2. プロジェクトファイル Mntone.ToastNotificationServer.sln　を開きます