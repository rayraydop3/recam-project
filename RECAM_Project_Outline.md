# RECAM Real Estate Media Delivery Platform — Project Outline

---

## 1. 项目概述

**项目名称**: Recam Real Estate Media Delivery Platform  
**目标**: 连接摄影公司（Admin）与房地产中介（Agent），实现房产 listing 的媒体资产管理、内容选择与展示交付。  
**Figma UI**: https://www.figma.com/design/OfVlwdHYiZuFQdyg96Bgxm/Recam-Delivery--Copy-?node-id=215-272

---

## 2. 用户角色

| 角色 | 身份 | 核心权限 |
|------|------|----------|
| Admin | 摄影公司 | 创建/管理 Listing Cases、上传媒体、管理状态、查看 Agent 选择结果 |
| Agent | 房地产中介 | 查看分配给自己的 Listings、选择展示内容、编辑/预览展示页面 |

---

## 3. 技术栈

### 后端
| 类别 | 技术 |
|------|------|
| 框架 | ASP.NET Core Web API |
| 主数据库 | SQL Server + Entity Framework Core |
| 辅助存储 | MongoDB（变更历史、操作日志） |
| 媒体存储 | Azure Blob Storage |
| 认证授权 | JWT + ASP.NET Identity |
| 输入验证 | FluentValidation |
| 对象映射 | AutoMapper |
| API 文档 | Swagger / OpenAPI |
| 部署 | Azure App Service + GitHub Actions CI/CD |
| 容器化 | Docker |

### 前端
| 类别 | 技术 |
|------|------|
| 框架 | React + Vite |
| 语言 | TypeScript |
| 路由 | React Router v6 |
| 状态管理 | Zustand |
| 服务端状态 | TanStack Query (React Query) |
| 表单 | React Hook Form + Zod |
| UI 组件库 | shadcn/ui |
| 样式 | Tailwind CSS |
| HTTP 请求 | Axios |

---

## 4. 数据库设计

### 4.1 SQL Server 表结构

#### UserIdentityUser（ASP.NET Identity 用户表）
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | string (PK) | 用户 ID |
| ... | ... | Identity 标准字段 |
| IsDeleted | boolean | 软删除 |
| CreatedAt | DateTime | 创建时间 |

#### PhotographyCompany（摄影公司）
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | string (PK/FK) | 公司 ID，关联 User |
| PhotographyCompanyName | string | 公司名称 |

#### Agent（中介）
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | string (PK/FK) | 中介 ID，关联 User |
| AgentFirstName | string | 名 |
| AgentLastName | string | 姓 |
| AvatarUrl | string | 头像地址 |
| CompanyName | string | 所属公司名 |

#### AgentsPhotographyCompany（中介-公司关联表）
| 字段 | 类型 | 说明 |
|------|------|------|
| AgentId | string (Composite PK) | 中介 ID |
| PhotographyCompanyId | string (Composite PK) | 公司 ID |

#### ListingCase（房产 Listing）
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int (PK) | Listing ID |
| Title | string | 标题 |
| Description | string | 描述 |
| Street | string | 街道 |
| City | string | 城市 |
| State | string | 州/省 |
| Postcode | int | 邮编 |
| Longitude | decimal | 经度 |
| Latitude | decimal | 纬度 |
| Price | double | 价格 |
| Bedrooms | int | 卧室数 |
| Bathrooms | int | 浴室数 |
| Garages | int | 车库数 |
| FloorArea | double | 建筑面积 |
| CreatedAt | datetime | 创建时间 |
| IsDeleted | boolean | 软删除 |
| PropertyType | int(enum) | House / Unit / Townhouse / Villa / Others |
| SaleCategory | int(enum) | For Sale / For Rent / Auction |
| ListcaseStatus | int(enum) | Created / Pending / Delivered |
| UserId | string (FK) | 创建者（Admin） |

#### AgentListingCase（中介-Listing 关联表）
| 字段 | 类型 | 说明 |
|------|------|------|
| AgentId | string (Composite PK) | 中介 ID |
| ListingCaseId | int (Composite PK) | Listing ID |

#### CaseContact（联系人）
| 字段 | 类型 | 说明 |
|------|------|------|
| ContactId | int (PK) | 联系人 ID |
| FirstName | string | 名 |
| LastName | string | 姓 |
| CompanyName | string | 公司 |
| ProfileUrl | string | 头像 |
| Email | string | 邮箱 |
| PhoneNumber | string | 电话 |
| ListingCaseId | int (FK) | 关联 Listing |

#### MediaAsset（媒体资产）
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int (PK) | 媒体 ID |
| MediaType | string | 类型：Photo / Video / FloorPlan / VR |
| MediaUrl | string | Blob Storage URL |
| UploadedAt | datetime | 上传时间 |
| IsSelect | boolean | Agent 是否选择展示 |
| IsHero | boolean | 是否为封面图 |
| ListingCaseId | int (FK) | 关联 Listing |
| UserId | string (FK) | 上传者 |
| IsDeleted | boolean | 软删除 |

### 4.2 MongoDB 集合

| 集合 | 用途 |
|------|------|
| CaseHistory | 记录 Listing Case 的所有变更历史 |
| UserActivityLog | 存储用户操作日志 |

### 4.3 Enum 定义

```csharp
// PropertyType
enum PropertyType { House = 1, Unit = 2, Townhouse = 3, Villa = 4, Others = 5 }

// SaleCategory
enum SaleCategory { ForSale = 1, ForRent = 2, Auction = 3 }

// ListingCaseStatus
enum ListingCaseStatus { Created = 1, Pending = 2, Delivered = 3 }

// MediaType
enum MediaType { Picture = 1, Video = 2, FloorPlan = 3, VR = 4 }
```

---

## 5. 后端项目结构

```
src/
├── Controllers/        # 接口层
├── Services/           # 业务逻辑层
├── Repositories/       # 数据访问层
├── Domain/             # 领域模型
├── Collection/         # MongoDB 集合定义
├── Data/               # DbContext
├── DTOs/               # 数据传输对象
├── Middlewares/        # 中间件（异常处理等）
├── Exceptions/         # 自定义异常类
├── Validator/          # FluentValidation 验证器
├── Mappers/            # AutoMapper Profile
└── Common/
    ├── Constants/      # 常量
    ├── Enums/          # 枚举类型
    ├── Extensions/     # 扩展方法
    ├── Helpers/        # 静态工具（DateHelper 等）
    └── Utilities/      # 其他工具（EmailSender 等）
```

---

## 6. 前端项目结构

```
src/
├── pages/
│   ├── admin/          # Admin 专属页面
│   │   ├── ListingsPage.tsx
│   │   ├── CreateListingPage.tsx
│   │   ├── EditListingPage.tsx
│   │   └── MediaUploadPage.tsx
│   ├── agent/          # Agent 专属页面
│   │   ├── MyListingsPage.tsx
│   │   ├── SelectMediaPage.tsx
│   │   └── EditDisplayPage.tsx
│   ├── shared/         # 共享页面
│   │   ├── PreviewPage.tsx
│   │   └── LoginPage.tsx
│   └── public/
│       └── PropertyWebsitePage.tsx  # 可分享的公开页面
├── components/         # 通用组件
│   ├── ui/             # shadcn/ui 基础组件
│   ├── MediaCard.tsx
│   ├── ListingTable.tsx
│   └── StatusBadge.tsx
├── features/           # 按功能模块
│   ├── auth/
│   ├── listings/
│   ├── media/
│   └── agents/
├── services/           # API 调用层（axios）
│   ├── listingService.ts
│   ├── mediaService.ts
│   ├── authService.ts
│   └── agentService.ts
├── hooks/              # 自定义 hooks
├── stores/             # Zustand 状态管理
│   ├── authStore.ts
│   └── listingStore.ts
├── types/              # TypeScript 类型定义（对应后端 DTOs）
└── utils/              # 工具函数
```

---

## 7. API 端点总览

### 认证
| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| POST | /auth/register | 摄影公司注册 | Public |
| POST | /auth/login | 用户登录 | Public |
| GET | /users/me | 获取当前用户信息 | Admin/Agent |
| PATCH | /users/me/password | 修改密码 | Admin/Agent |

### 公司 & Agent 管理
| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| GET | /photography-companies | 获取所有摄影公司 | Admin |
| POST | /agents | 创建 Agent 账号 | Admin |
| GET | /agents | 获取所有 Agent | Admin |
| GET | /agents/search?email= | 按邮箱搜索 Agent | Admin |
| POST | /photography-companies/{id}/agents | 将 Agent 加入公司 | Admin |
| GET | /photography-companies/{id}/agents | 获取公司下所有 Agent | Admin |

### Listing Case
| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| POST | /listings | 创建 Listing Case | Admin |
| GET | /listings | 获取所有 Listings（含筛选/分页） | Admin |
| GET | /listings/{id} | 获取 Listing 详情 | Admin/Agent |
| PUT | /listings/{id} | 更新 Listing 信息 | Admin |
| DELETE | /listings/{id} | 删除 Listing | Admin |
| PATCH | /listings/{id}/status | 变更状态 | Admin |
| POST | /listings/{id}/agents | 分配 Agent | Admin |

### 媒体管理
| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| POST | /listings/{id}/media | 上传媒体文件 | Admin |
| GET | /listings/{id}/media | 获取 Listing 所有媒体 | Admin/Agent |
| DELETE | /media/{id} | 删除媒体文件 | Admin |
| PUT | /listings/{id}/cover-image | 设置封面图 | Admin |

### Agent 内容选择
| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| PUT | /listings/{id}/selected-media | Agent 选择展示媒体 | Agent |
| GET | /listings/{id}/final-selection | 获取最终选择结果 | Admin/Agent |

### 联系人
| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| POST | /listings/{id}/contacts | 添加联系人 | Admin/Agent |
| GET | /listings/{id}/contacts | 获取联系人列表 | Admin/Agent |
| DELETE | /contacts/{id} | 删除联系人 | Admin/Agent |

### 预览 & 下载
| 方法 | 路径 | 说明 | 权限 |
|------|------|------|------|
| GET | /listings/{id}/preview | 获取预览数据 | Admin/Agent |
| GET | /listings/{id}/download | 打包下载所有资源（ZIP） | Admin/Agent |
| GET | /listings/{id}/view/{token} | 公开可分享页面 | Public |
| POST | /listings/{id}/publish | 生成分享链接 | Admin/Agent |

---

## 8. 核心业务规则

### Listing Case 状态流转
```
Created ──(Admin 上传媒体后)──► Pending ──(Agent 完成选择后)──► Delivered
```

### 权限矩阵
| 功能 | Admin | Agent |
|------|-------|-------|
| 创建 Listing Case | ✅ | ❌ |
| 上传媒体 | ✅ | ❌ |
| 编辑 Property 内容 | ✅ | ✅ |
| 选择展示媒体 | 查看结果 | ✅ |
| 查看/预览展示页面 | ✅ | ✅ |
| 切换 Listing 状态 | ✅ | ❌ |
| 管理 Agent 账号 | ✅ | ❌ |

### 媒体上传规则
- 支持格式：jpg、png、mp4、mov、PDF、gltf/vr
- 每次只能上传一种媒体类型（Picture / Video / FloorPlan）
- 封面图（Hero）只能有一张
- 媒体存储于 Azure Blob Storage，数据库只存 URL

---

## 9. 代码规范

### 后端命名规范
- 类: PascalCase（`UserService`, `ListingController`）
- 方法: PascalCase（`GetListingById`, `CreateListing`）
- 变量: camelCase（`listingId`, `mediaAssets`）
- 接口: 以 `I` 开头（`IListingService`, `IMediaRepository`）

### 统一响应格式
```json
{
  "succeed": true,
  "message": "Success",
  "data": { ... },
  "errorMessage": null,
  "errorCode": null
}
```

### Git Commit 规范
```
<type>(<scope>): <subject>
feat(RECAM-99): Create listing case API
fix(RECAM-12): Fix media upload validation bug
```
类型：`feat` | `fix` | `docs` | `style` | `refactor` | `test` | `chore`

### 后端质量要求
- 所有异常通过 ExceptionMiddleware 统一处理
- 必须使用 Dependency Injection
- 多表操作必须使用 Transaction
- 所有请求数据必须 FluentValidation
- 接口使用 XML 文档注释
- 缩进：4 个空格

---

## 10. 开发任务清单（Epic 顺序）

### Epic 1: 项目初始化
- [ ] 初始化 Git 仓库，创建 .gitignore
- [ ] 创建 ASP.NET Core Web API 项目
- [ ] 配置 appsettings.json 环境变量管理
- [ ] 设定项目结构
- [ ] 配置 Swagger
- [ ] 配置 EF Core（SQL Server）
- [ ] 配置 MongoDB
- [ ] 配置 AutoMapper
- [ ] 配置 JWT 认证（ASP.NET Identity）
- [ ] 初始化 React + Vite + TypeScript 前端项目

### Epic 2: 数据库 & 基础设施
- [ ] 创建 DbContext 及所有 Domain Model
- [ ] 创建全局异常处理中间件
- [ ] 创建统一响应格式 ApiResponse<T>
- [ ] 创建 Global Email Sender Service
- [ ] 编写 EF Core 迁移脚本并运行
- [ ] 设计 MongoDB CaseHistory / UserActivityLog 集合
- [ ] 配置 Azure Blob Storage

### Epic 3: 用户 & 公司 API
- [ ] Photography Company 注册 API
- [ ] User 登录 API（JWT）
- [ ] 获取当前用户信息 API（GET /users/me）
- [ ] 修改密码 API
- [ ] 创建 Agent 账号 API（Admin only）
- [ ] 获取所有 Agent API
- [ ] 按邮箱搜索 Agent API
- [ ] 将 Agent 加入摄影公司 API
- [ ] 获取摄影公司旗下 Agent 列表 API
- [ ] 获取所有摄影公司 API

### Epic 4: Listing Case API
- [ ] 创建 Listing Case API（POST /listings）
- [ ] 获取所有 Listing Cases API（含筛选/分页）
- [ ] 获取 Listing Case 详情 API
- [ ] 更新 Listing Case API
- [ ] 删除 Listing Case API
- [ ] 变更 Listing Case 状态 API
- [ ] 分配 Agent 到 Listing Case API

### Epic 5: 媒体管理 API
- [ ] 上传媒体文件到 Blob Storage API
- [ ] 获取 Listing 所有媒体 API
- [ ] 删除媒体文件 API
- [ ] 设置封面图 API

### Epic 6: Agent 内容选择 & 联系人 API
- [ ] Agent 选择展示媒体 API（PUT /listings/{id}/selected-media）
- [ ] 获取最终选择结果 API（GET /listings/{id}/final-selection）
- [ ] 添加/获取/删除 CaseContact API

### Epic 7: 预览 & 分享 & 下载 API
- [ ] 获取预览数据 API
- [ ] 生成可分享链接 API
- [ ] 查看公开分享页面 API
- [ ] 打包下载所有资源 API（ZIP）
- [ ] 单文件下载 API

### Epic 8: 单元测试
- [ ] Listing Case API 单元测试（xUnit + Moq）
- [ ] Media API 单元测试
- [ ] Agent 选择 API 单元测试
- [ ] 业务逻辑测试（状态流转、MongoDB 写入）

### Epic 9: 代码质量 & 文档
- [ ] FluentValidation 配置
- [ ] 代码重构（SOLID 原则）
- [ ] 分页 & 筛选（Listing 列表）
- [ ] Swagger API 文档完善
- [ ] README 编写

### Epic 10: 部署
- [ ] 配置 Azure App Service
- [ ] 配置 Azure SQL
- [ ] 配置 CosmosDB
- [ ] 配置 Azure Blob Storage
- [ ] 编写 Dockerfile
- [ ] 配置 CI/CD（GitHub Actions）

---

## 11. 部署架构

```
┌─────────────────────────────────────────────┐
│                  客户端                        │
│          React App (Vercel / Azure)           │
└────────────────────┬────────────────────────┘
                     │ HTTPS / JWT
┌────────────────────▼────────────────────────┐
│          ASP.NET Core Web API                │
│           (Azure App Service)                │
└──────┬─────────────┬────────────────────────┘
       │             │
┌──────▼──────┐ ┌────▼──────┐  ┌─────────────┐
│  Azure SQL  │ │  MongoDB  │  │ Azure Blob  │
│ (主数据库)   │ │ (日志/历史)│  │  Storage    │
└─────────────┘ └───────────┘  │ (媒体文件)   │
                                └─────────────┘
```

---

*文档生成日期: 2026-04-08*
