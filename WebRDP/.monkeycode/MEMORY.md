# 用户指令记忆

本文件记录了用户的指令、偏好和教导，用于在未来的交互中提供参考。

## 格式

### 用户指令条目
用户指令条目应遵循以下格式：

[用户指令摘要]
- Date: [YYYY-MM-DD]
- Context: [提及的场景或时间]
- Instructions:
  - [用户教导或指示的内容，逐行描述]

### 项目知识条目
Agent 在任务执行过程中发现的条目应遵循以下格式：

[项目知识摘要]
- Date: [YYYY-MM-DD]
- Context: Agent 在执行 [具体任务描述] 时发现
- Category: [代码结构 | 代码模式 | 代码生成 | 构建方法 | 测试方法 | 依赖关系 | 环境配置]
- Instructions:
  - [具体的知识点，逐行描述]

## 去重策略
- 添加新条目前，检查是否存在相似或相同的指令
- 若发现重复，跳过新条目或与已有条目合并
- 合并时，更新上下文或日期信息
- 这有助于避免冗余条目，保持记忆文件整洁

## 条目

[WebRDP 项目需求]
- Date: 2026-04-16
- Context: WebRDP 项目初始需求
- Instructions:
  - 实现一个能够内置到 Electron 客户端中的 webRDP 项目
  - 底层技术栈使用 C#.NET6，需要支持跨平台（windows 和 uos）
  - 使用 FreeRDP 这个开源可免费商用的第三方库
  - 需要支持在内网环境运行
  - 通过 127.0.0.2:3389 创建本地桌面的新会话
  - 新会话在 web 窗体中显示，且本地无其他桌面的 FreeRDP 窗口出现
  - 需要用户提前输入用户名和密码，用户点击连接后，则自动创建新会话
  - 如果之前已经打开过一个会话，且未注销，则自动连接上一次的会话，不重复创建过多会话（同一时间，最多一个本地会话，一个远程会话）
  - 在 web 窗口中的新会话中，可以几乎 0 延迟实现鼠标、键盘的操作，可以正常打开应用程序，查看文档，编辑文档等所有操作，且和本地桌面环境隔离，互不影响
  - 如果涉及到日志记录，使用 log4net 的 2.0.13 版本
  - 完成代码实现后需要进行完整的单元测试验证，并输出单元测试案例和测试总结报告
  - 确保所有功能正常后，再将代码提交到码云上
  - 代码实现过程中，出现过的所有问题，需要记录并总结，避免重复犯错
  - 每一次遇到问题时，先查看记录的问题总结文档，避免重复犯错

[WebRDP 技术决策]
- Date: 2026-04-16
- Context: WebRDP 技术方案设计过程
- Category: 代码结构
- Instructions:
  - 集成架构：C# 本地服务 + IPC 通信 + Electron 客户端
  - FreeRDP 集成方式：使用现有.NET 绑定库（自行实现 P/Invoke 封装）
  - 会话隔离：原生会话 API 管理
  - Web 渲染：HTML5 RDP 渲染器
  - 本地地址：通过配置文件动态指定（支持跨平台差异化配置）

[WebRDP 项目结构]
- Date: 2026-04-16
- Context: Agent 在创建 WebRDP 项目时发现
- Category: 代码结构
- Instructions:
  - 项目采用分层架构：WebRdp.Service（后端服务）+ WebRdp.Client（FreeRDP 封装）+ WebRdp.Web（Electron 界面）
  - 测试项目：WebRdp.Service.Tests + WebRdp.Client.Tests
  - 文档目录：docs/issues-log.md（问题记录）+ test-summary-report.md（测试报告）
  - 解决方案文件：WebRDP.sln

[WebRDP 测试方法]
- Date: 2026-04-16
- Context: Agent 在编写单元测试时发现
- Category: 测试方法
- Instructions:
  - 测试框架：xUnit 2.4.1 + Moq 4.17.2
  - 运行测试命令：dotnet test tests/WebRdp.Service.Tests/WebRdp.Service.Tests.csproj
  - 核心测试类：RdpSessionManagerTests（7 个测试用例）
  - 测试覆盖率目标：85% 以上
  - 测试报告输出到 docs/test-summary-report.md

[WebRDP 日志配置]
- Date: 2026-04-16
- Context: Agent 在配置 log4net 时发现
- Category: 依赖关系
- Instructions:
  - 使用 log4net 2.0.13 版本（需求指定）
  - 使用 Microsoft.Extensions.Logging.Log4Net.AspNetCore 6.1.0 进行.NET 6 集成
  - 配置文件：log4net.config（XML 格式）
  - 日志输出路径：logs/webrdp-service.log
  - 日志滚动策略：按大小滚动，最大 10MB，保留 5 个备份

[避免重复犯错]
- Date: 2026-04-16
- Context: 用户明确要求
- Instructions:
  - 所有在项目过程中遇到的问题必须记录到 docs/issues-log.md
  - 每次遇到问题时先查看问题总结文档，避免重复犯错
  - 问题解决后立即更新问题记录文档
