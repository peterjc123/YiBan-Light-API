
# YiBan-Light-API
The purpose of YiBan-Light-API is to add posts on YiBan automatically. It used the .NET Core, which means the code can run on major OS, such as Windows 7 or up, Ubuntu 14.04 or up and OS X 10.10 or up.

# Release
You can also download the latest version for Window, Ubuntu and OS X via [this link](https://github.com/peterjc123/YiBan-Light-API/releases).

# Build
To build this project, you must have the Visual Studio 2015 (only on Windows) or Visual Studio Code and the .Net Core SDK to be installed.

You could just type the command below to make the project work:

```
dotnet restore
dotnet build -c release
dotnet run
```

# Example
It's very easy to use. Suppose you just want to sign a new user:

```C#
var control = new SignController();
control.Set("user", "pass");
control.Start();
```

With these three lines, we're able to start signing.

Now that you've got the data.xml that contains data of your whole class, how to sign all those accounts one by one automatically?

```c#
var control = new SignController();
control.Init();
control.Start();
```

For more usage, please go to the [project wiki](https://github.com/peterjc123/YiBan-Light-API/wiki) for more information.