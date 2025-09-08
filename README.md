# API 测试工具 (WinForms / .NET 8)

轻量、直观的本地 GUI API 调试工具：分栏布局、路径分段、键值对式参数/请求头/Cookie 管理、响应 & Cookie 变化分离显示。

## 功能概览

- 路径分段：逐段输入自动拼接 URL。
- 参数 / 请求头 / Cookie：可增删的键值对行，空键自动忽略。
- Cookie 会话：发送前写入，响应后对比差异（新增/修改/删除）。
- JSON 自动格式化（失败则原样显示）。
- 深色代码区 + Emoji 分组标题。

## 界面结构

左侧：
1. 基础：Base URL + 方法选择
2. 路径分段
3. 查询参数
4. 请求头
5. Cookie 设定
6. 发送按钮

右侧（上下 Split）：
- 上：响应状态/头/体
- 下：Cookie 变化日志 + 当前 Cookie 列表

## 运行

```pwsh
cd APITestApp
dotnet build
dotnet run
```

可选：启动内置测试服务器（Node）
```pwsh
node test-server.js
```
默认监听 http://localhost:3001

## 使用步骤
1. 填 Base URL（如 http://localhost:3001）与方法。
2. 路径区添加段：api / users / 123 → /api/users/123。
3. 参数区填写键值（自动 URL 编码）。
4. 请求头（例：Authorization, Accept）。
5. Cookie 区自定义会话值。
6. 点击“发送请求”。
7. 右侧查看响应 + 下方 Cookie 变化。

## 典型请求示例
| 模块 | 值 |
| ---- | --- |
| Base URL | http://localhost:3001 |
| 路径段 | api, users, 123 |
| 查询参数 | include=profile, format=json |
| 方法 | GET |
| 请求头 | Accept=application/json |
| Cookie | session=abc123 |

最终 URL：`http://localhost:3001/api/users/123?include=profile&format=json`

## 关键实现说明
- `PathPanel` / `KeyValuePanel`：采用普通 `Panel` + 子控件 Dock=Top，避免 FlowLayoutPanel 在某些 DPI/布局下导致子控件宽度塌缩看不见的问题；新增控件后 `SendToBack()` 保持自上而下顺序。
- `CookieContainer` + 差异对比：记录上次快照 ⇒ 新集合比较 ⇒ 输出新增 / 修改 / 删除。
- `HttpClient` 复用单实例，避免端口耗尽。
- JSON 处理：`Newtonsoft.Json` 反序列化后再格式化，失败则原样。

## 常见问题 (FAQ)
| 问题 | 说明 |
| ---- | ---- |
| 看不到路径/参数行 | 已修复：改为 Panel + Dock。请更新到当前版本。 |
| Cookie 没变化 | 目标接口未返回 Set-Cookie 或域不匹配。确保主机一致。 |
| 自定义 Content-Type 不生效 | 当前仅演示 GET/无 Body，POST 发送体可扩展。 |

## 扩展方向
- 支持请求 Body（raw / form / JSON 编辑器）
- 导入导出请求集合 (JSON)
- 响应时间统计 / 重复压力测试
- Header / Cookie 预设模板

## 依赖
| 名称 | 用途 |
| ---- | ---- |
| .NET 8 | 运行时 |
| Windows Forms | GUI |
| Newtonsoft.Json | JSON 解析/格式化 |
| Node.js (可选) | 本地测试服务 |

## 许可
仅供学习与内部调试使用，未附加开源协议，可根据需要自行添加 LICENSE。

## 更新摘要
| 版本 | 内容 |
| ---- | ---- |
| 2.0 | 路径分段、响应/ Cookie 分离、布局重构、可视化增强 |
| 2.1(当前) | 修复子行不可见（移除 FlowLayoutPanel），README 精简重写 |

—— 享受更顺手的本地 API 调试体验。
