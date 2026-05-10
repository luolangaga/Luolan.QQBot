# C# 基础知识

本页面为刚开始接触 C# 的新手准备，涵盖使用 Luolan.QQBot SDK 所需的核心知识。

## .NET 是什么？

.NET 是微软开发的开源开发平台。你可以用它来构建各种类型的应用：
- **Console App**（控制台应用）— 命令行程序
- **ASP.NET Core**（Web 应用）— 网站和 API 服务
- **Desktop App**（桌面应用）— Windows 桌面程序

Luolan.QQBot 可以在 Console App 和 ASP.NET Core 中运行。

## 环境准备

### 1. 安装 .NET SDK

从 [dotnet.microsoft.com](https://dotnet.microsoft.com/download) 下载并安装 **.NET 8.0 SDK**。

安装完成后，打开终端验证：

```bash
dotnet --version
# 输出类似: 8.0.x
```

### 2. 安装编辑器

推荐使用以下任一编辑器：
- **[Visual Studio 2022 Community](https://visualstudio.microsoft.com/)**（免费，功能全面）
- **[Visual Studio Code](https://code.visualstudio.com/)**（免费，轻量级）+ C# 扩展
- **[JetBrains Rider](https://www.jetbrains.com/rider/)**（付费，功能强大）

## 创建第一个项目

打开终端，运行以下命令：

```bash
# 创建控制台应用
dotnet new console -n MyFirstBot
cd MyFirstBot

# 添加 Luolan.QQBot 包
dotnet add package Luolan.QQBot

# 运行项目
dotnet run
```

项目结构：
```
MyFirstBot/
├── MyFirstBot.csproj  ← 项目文件（定义依赖和配置）
├── Program.cs         ← 入口文件（程序从这里开始执行）
└── obj/               ← 编译输出（不需要管）
```

## C# 语言基础

### 变量和类型

C# 是**强类型**语言，每个变量都有明确的类型。

```csharp
// 基本类型
int age = 25;           // 整数
long bigNumber = 99L;   // 长整数
double price = 19.99;   // 浮点数
decimal money = 99.99m; // 精确小数（适合金额）
bool isOnline = true;   // 布尔值（true/false）
string name = "小明";    // 字符串
char letter = 'A';      // 单个字符

// 类型推断 — 用 var 让编译器自动推断类型
var count = 10;         // count 是 int
var message = "你好";    // message 是 string

// 可空类型 — 可以没有值
int? maybeNull = null;  // 可能为 null 的整数
string? optional = null; // 可空字符串（需要 #nullable enable）
```

### 字符串

```csharp
// 字符串拼接
string hello = "你好，" + "世界";

// 字符串插值（推荐）
string name = "小明";
int age = 18;
string intro = $"我叫{name}，今年{age}岁";

// 多行字符串
string multiLine = """
    第一行
    第二行
    第三行
    """;
```

### 数组和集合

```csharp
// 数组 — 固定大小
string[] names = new string[] { "小明", "小红", "小刚" };
int[] numbers = { 1, 2, 3, 4, 5 };

// List — 动态大小
List<string> items = new List<string>();
items.Add("第一个");
items.Add("第二个");
Console.WriteLine(items.Count); // 2

// Dictionary — 键值对
Dictionary<string, int> scores = new()
{
    ["小明"] = 95,
    ["小红"] = 88
};
Console.WriteLine(scores["小明"]); // 95
```

### 条件判断

```csharp
int score = 85;

if (score >= 90)
{
    Console.WriteLine("优秀");
}
else if (score >= 60)
{
    Console.WriteLine("及格");
}
else
{
    Console.WriteLine("不及格");
}

// 三元运算符 — 单行条件
string result = score >= 60 ? "及格" : "不及格";

// switch 表达式
string level = score switch
{
    >= 90 => "优秀",
    >= 60 => "及格",
    _ => "不及格"
};
```

### 循环

```csharp
// for 循环
for (int i = 0; i < 5; i++)
{
    Console.WriteLine(i);
}

// foreach 循环 — 遍历集合
string[] names = { "小明", "小红" };
foreach (string name in names)
{
    Console.WriteLine($"你好，{name}");
}

// while 循环
int count = 0;
while (count < 3)
{
    Console.WriteLine(count);
    count++;
}
```

### 方法（函数）

```csharp
// 基本方法
int Add(int a, int b)
{
    return a + b;
}

// 表达式体方法 — 单行简写
int Multiply(int a, int b) => a * b;

// 可选参数
string Greet(string name = "世界")
{
    return $"你好，{name}！";
}

// params 参数 — 可变数量
string Join(string separator, params string[] parts)
{
    return string.Join(separator, parts);
}
// 调用: Join(", ", "A", "B", "C") → "A, B, C"
```

### 类（Class）

类是 C# 中最基本的结构，用于组织代码和数据。

```csharp
// 定义一个类
public class Student
{
    // 属性 — 封装数据
    public string Name { get; set; }
    public int Age { get; set; }

    // 构造函数 — 创建对象时的初始化
    public Student(string name, int age)
    {
        Name = name;
        Age = age;
    }

    // 方法 — 定义行为
    public string Introduce()
    {
        return $"我叫{Name}，今年{Age}岁";
    }
}

// 使用类
var student = new Student("小明", 18);
Console.WriteLine(student.Introduce());
```

### 异步编程（Async/Await）

这是使用 Luolan.QQBot **最重要的概念**。机器人 API 调用都是异步的。

```csharp
// 异步方法 — 关键字 async Task
async Task<string> FetchDataAsync()
{
    // await 等待异步操作完成，不阻塞线程
    await Task.Delay(1000);  // 模拟网络请求
    return "数据加载完成";
}

// 调用异步方法
string result = await FetchDataAsync();  // 等待结果

// 并行执行多个任务
Task<string> task1 = FetchDataAsync();
Task<string> task2 = FetchDataAsync();
string[] results = await Task.WhenAll(task1, task2);
```

::: tip 为什么用 async/await？
网络请求（调用 QQ API）需要几毫秒到几秒。如果用同步方式，程序会卡住等待。
async/await 让程序在等待时不阻塞，可以同时处理其他任务。
:::

### Lambda 表达式

Lambda 是一种简洁的匿名函数写法，在事件处理中大量使用。

```csharp
// 完整写法
Func<int, int, int> add = (a, b) => { return a + b; };

// 简写
Func<int, int, int> add2 = (a, b) => a + b;

// 无参数
Action sayHello = () => Console.WriteLine("Hello");

// 在事件中使用
bot.OnReady += async e =>
{
    Console.WriteLine($"机器人上线：{e.User.Username}");
};
```

### using 语句

```csharp
using System;              // 引入命名空间
using Luolan.QQBot;        // 引入 Luolan.QQBot 命名空间

// using 也用于资源管理
using var client = new HttpClient(); // 用完自动释放
```

### 异常处理

```csharp
try
{
    // 可能出错的代码
    await bot.StartAsync();
}
catch (QQBotApiException ex)
{
    Console.WriteLine($"API 错误: [{ex.Code}] {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"未知错误: {ex.Message}");
}
finally
{
    // 无论成功失败都执行的清理代码
    await bot.StopAsync();
}
```

## 命名空间（Namespace）

命名空间用于组织代码，避免命名冲突。

```csharp
namespace MyApp;

public class MyController
{
    // 完整类型名: MyApp.MyController
}
```

使用不同命名空间的类型需要 `using` 引入：

```csharp
using Luolan.QQBot;           // QQBotClient, QQBotClientBuilder
using Luolan.QQBot.Controllers; // QQBotController, CommandAttribute, ImageResult
using Luolan.QQBot.Models;     // Message, User, Guild 等模型
using Luolan.QQBot.Events;     // 各种事件类型
using Luolan.QQBot.Extensions; // UseControllers, SendMarkdownAsync 等扩展
using Luolan.QQBot.Helpers;    // KeyboardBuilder, MarkdownBuilder, CommandParser
```

## NuGet 包管理

NuGet 是 .NET 的包管理器，类似 npm（Node.js）或 pip（Python）。

```bash
# 安装包
dotnet add package Luolan.QQBot

# 安装特定版本
dotnet add package Luolan.QQBot --version 1.4.0

# 列出已安装的包
dotnet list package

# 更新包
dotnet add package Luolan.QQBot --version 1.5.0
```

## 常用 .NET CLI 命令

```bash
dotnet new console     # 创建控制台项目
dotnet new webapi      # 创建 Web API 项目
dotnet build           # 编译项目
dotnet run             # 运行项目
dotnet watch run       # 监听文件变化，自动重启
dotnet publish         # 发布项目（用于部署）
dotnet clean           # 清理编译输出
dotnet restore         # 还原 NuGet 包
```

## 下一步

了解了这些基础知识后，你可以继续阅读：

- [快速开始](/guide/getting-started) — 创建第一个机器人
- [控制器模式](/guide/controller-mode) — Luolan.QQBot 的核心功能
- [事件系统](/guide/events) — 处理各种 QQ 事件
