# 项目文档索引

## 目录结构

```
.monkeycode/
├── docs/             # 项目文档
│   ├── INDEX.md      # 本文档
│   └── ARCHITECTURE.md  # 项目架构
├── specs/            # 历史特性规格
│   └── [date]-[feature]/
│       ├── requirements.md
│       ├── design.md
│       └── tasklist.md
└── MEMORY.md         # 用户指令和项目知识记忆
```

## 当前项目

### WebRDP

- **日期**: 2026-04-16
- **描述**: 内置到 Electron 客户端的 Web RDP 远程桌面解决方案
- **技术栈**: C# .NET 6 + FreeRDP + HTML5
- **规格文档**: [2026-04-16-webrdp](../specs/2026-04-16-webrdp/design.md)

## 架构文档

- [系统架构](./ARCHITECTURE.md)
