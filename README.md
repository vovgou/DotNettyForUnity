# DotNetty For Unity

[![release](https://img.shields.io/github/v/tag/vovgou/DotNettyForUnity?label=release)](https://github.com/vovgou/DotNettyForUnity/releases)
[![npm](https://img.shields.io/npm/v/com.vovgou.dotnetty)](https://www.npmjs.com/package/com.vovgou.dotnetty)

DotNetty is a port of [Netty](https://github.com/netty/netty), asynchronous event-driven network application framework for rapid development of maintainable high performance protocol servers & clients.

This version is modified based on [DotNetty](https://github.com/Azure/DotNetty)'s 0.7.5 version and is a customized version for the Unity development platform. It removes some dependencies and has been tested under IL2CPP.

## Fixes & Improvements

 * Removed assemblies System.Collections.Immutable,System.Threading.Tasks.Extensions and Microsoft.Extensions.Logging.
 * Added the Factory property  to the InternalLoggerFactory to integrate the log printing feature of Unity.
 * Added ScheduleWithFixedDelay and ScheduleAtFixedRate methods for IScheduledExecutorService.
 * Added FixedDelayScheduledTask and FixedRateScheduledTask.
 * Added object pools for ActionTaskQueueNode, StateActionTaskQueueNode, and StateActionWithContextTaskQueueNode, and optimized GC.
 * Added ReusableScheduledTask,reuse ScheduledTask to reduce heap memory allocation.
 * Solved the bug that when the SocketDatagramChannel does not set localAddress, the channel cannot be activated and cannot receive any messages.
 * Fixed a bug of SocketDatagramChannel on MacOSX system,when the channel is already connected to an address, calling the SentTo function will throw an exception.
 * Fixed a bug,in the netstandard2.x library, the Write method of SslStream will switch threads, and the DoFinishWrap of TlsHandler needs to be switched to the thread bound to Channel.
 * Pool ByteBuffer objects to reduce GC.
 * Pool some frequently used objects to reduce GC
 * Roll back the version of ThreadLocalPool to avoid the bug that pooled objects cannot be recycled correctly when released by another thread.
 * Merge the "writevaluetask" branch of https://github.com/maksimkim/DotNetty into the project, and use ValueTask to optimize the GC of Task.


## Installation

### Install via OpenUPM 

Modify the Packages/manifest.json file in your unity project, add the third-party repository "package.openupm.com"'s configuration and add "com.vovgou.dotnetty" in the "dependencies" node.

    {
      "dependencies": {
        ...
        "com.unity.modules.xr": "1.0.0",
        "com.vovgou.dotnetty": "0.7.5"
      },
      "scopedRegistries": [
        {
          "name": "package.openupm.com",
          "url": "https://package.openupm.com",
          "scopes": [
            "com.vovgou"
          ]
        }
      ]
    }

### Install via NPM 

Modify the Packages/manifest.json file in your unity project, add the third-party repository "npmjs.org"'s configuration and add "com.vovgou.dotnetty" in the "dependencies" node.

    {
      "dependencies": {
        ...
        "com.unity.modules.xr": "1.0.0",
        "com.vovgou.dotnetty": "0.7.5"
      },
      "scopedRegistries": [
        {
          "name": "npmjs.org",
          "url": "https://registry.npmjs.org/",
          "scopes": [
            "com.vovgou"
          ]
        }
      ]
    }

## Contribute

We gladly accept community contributions.

* Issues: Please report bugs using the Issues section of GitHub
* Source Code Contributions:
 * Please follow the [Microsoft Azure Projects Contribution Guidelines](https://opensource.microsoft.com/collaborate) open source that details information on onboarding as a contributor
 * See [C# Coding Style](https://github.com/Azure/DotNetty/wiki/C%23-Coding-Style) for reference on coding style.
