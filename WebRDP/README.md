# WebRDP 项目 README

## 项目简介

WebRDP 是一个能够内置到 Electron 客户端中的 Web RDP 远程桌面解决方案。

**技术栈**:
- 后端：C# .NET 6 (跨平台：Windows + UOS)
- RDP 引擎：FreeRDP (开源，可免费商用)
- 容器：Electron 客户端
- 日志：log4net v2.0.13

## 核心功能

- [x] 通过可配置的本地地址创建隔离的本地桌面新会话
- [x] 新会话在 Electron Web 窗体中渲染显示
- [x] 用户预设用户名密码，自动创建/重连会话
- [x] 会话复用：同一时间最多一个本地会话 + 一个远程会话
- [x] 低延迟的鼠标键盘操作
- [x] 内网环境运行，不依赖外网

## 项目结构

```
WebRDP/
├── src/
│   ├── WebRdp.Service/          # C# .NET 6 后端服务
│   ├── WebRdp.Client/           # FreeRDP .NET 封装库
│   └── WebRdp.Web/              # Electron Web 界面
├── tests/
│   ├── WebRdp.Service.Tests/    # 服务层单元测试
│   └── WebRdp.Client.Tests/     # FreeRDP 封装测试
├── docs/
│   └── issues-log.md            # 问题记录文档
├── WebRDP.sln                   # 解决方案文件
└── README.md                    # 本文档
```

## 快速开始

### 环境要求

- .NET 6 SDK
- Node.js 18+
- FreeRDP 3.x (libfreerdp3)

### Windows

```bash
# 安装 FreeRDP
# 下载地址：https://github.com/FreeRDP/FreeRDP/releases

# 构建项目
dotnet build WebRDP.sln

# 运行后端服务
cd src/WebRdp.Service
dotnet run

# 运行 Electron 客户端 (新终端)
cd src/WebRdp.Web
npm install
npm start
```

### UOS (Linux)

```bash
# 安装 FreeRDP
sudo apt-get install libfreerdp3-3 freerdp3-x11

# 构建项目
dotnet build WebRDP.sln

# 运行后端服务
cd src/WebRdp.Service
dotnet run

# 运行 Electron 客户端 (新终端)
cd src/WebRdp.Web
npm install
npm start
```

## 配置说明

### appsettings.json

```json
{
  "RdpSettings": {
    "DefaultHost": "127.0.0.2",
    "DefaultPort": 3389,
    "MaxSessionCount": 1
  },
  "PlatformSettings": {
    "Windows": {
      "LocalAddress": "127.0.0.2"
    },
    "UOS": {
      "LocalAddress": "127.0.0.2"
    }
  }
}
```

## API 接口

### RDP 控制 API

- `POST /api/rdp/connect` - 创建/连接会话
- `DELETE /api/rdp/disconnect/{sessionId}` - 断开会话
- `GET /api/rdp/status/{sessionId}` - 获取会话状态
- `POST /api/rdp/input/{sessionId}` - 发送输入事件
- `GET /api/rdp/stream` - WebSocket 视频流

## 测试

### 运行单元测试

```bash
# 运行所有测试
dotnet test WebRDP.sln

# 生成测试覆盖率报告
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# 运行特定测试
dotnet test --filter "FullyQualifiedName~RdpSessionManagerTests"
```

### 测试报告

测试完成后，测试报告将输出到控制台。覆盖率报告生成在 `TestResults/` 目录。

## 开发流程

1. 修改代码
2. 运行单元测试：`dotnet test`
3. 验证功能
4. 提交代码

## 问题排查

详见 [问题记录文档](docs/issues-log.md)

## 许可证

本项目使用的 FreeRDP 库采用 Apache 2.0 许可证，可免费商用。

## 相关文档

- [技术方案](../../.monkeycode/specs/2026-04-16-webrdp/design.md)
- [问题记录](docs/issues-log.md)
- [项目架构](../../.monkeycode/docs/ARCHITECTURE.md)
