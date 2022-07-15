# DotNetty For Unity

[![release](https://img.shields.io/github/v/tag/vovgou/DotNettyForUnity?label=release)](https://github.com/vovgou/DotNettyForUnity/releases)
[![npm](https://img.shields.io/npm/v/com.vovgou.dotnetty)](https://www.npmjs.com/package/com.vovgou.dotnetty)

DotNetty is a port of [Netty](https://github.com/netty/netty), asynchronous event-driven network application framework for rapid development of maintainable high performance protocol servers & clients.

This version is modified based on [DotNetty](https://github.com/Azure/DotNetty)'s 0.7.2 version and is a customized version for the Unity development platform. It removes some dependent libraries and passes the test under IL2CPP.

## Installation

### Install via OpenUPM 

Modify the Packages/manifest.json file in your unity project, add the third-party repository "package.openupm.com"'s configuration and add "com.vovgou.dotnetty" in the "dependencies" node.

    {
      "dependencies": {
        ...
        "com.unity.modules.xr": "1.0.0",
        "com.vovgou.dotnetty": "0.7.2"
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
        "com.vovgou.dotnetty": "0.7.2"
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
 * Please follow the [Contribution Guidelines for Microsoft Azure](http://azure.github.io/guidelines.html) open source that details information on onboarding as a contributor
 * See [C# Coding Style](https://github.com/Azure/DotNetty/wiki/C%23-Coding-Style) for reference on coding style.
