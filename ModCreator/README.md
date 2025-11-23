# ModCreator

Tool WPF để tạo và quản lý mod projects cho GuiGuBaHuang.

## ✨ Tính năng

- ✅ Tạo project mới từ template
- ✅ Quản lý danh sách projects (Dashboard)
- ✅ Tìm kiếm, xem chi tiết, sửa, xóa projects
- ✅ Tách biệt business logic và UI (CWindow pattern)
- ✅ Styles tái sử dụng

## 🚀 Cài đặt & Chạy

**Yêu cầu**: .NET Framework 4.7.2, Visual Studio 2019+

### Chạy nhanh
```powershell
.\run.ps1
```

### Build & Run thủ công
```powershell
.\build.ps1
.\ModCreator\bin\Release\ModCreator.exe
```

## 📖 Sử dụng

### Tạo Project
1. Click **"➕ Tạo Mới"**
2. Nhập **Tên** và chọn **Thư mục đích** (Project ID tự động gen)
3. Click **"Tạo"**

### Quản lý
- **Tìm kiếm**: Gõ từ khóa vào search box
- **Sửa**: Chọn project → Click **"✏️"** 
- **Xóa**: Click **"🗑️"** → Chọn Yes (xóa cả folder) hoặc No (giữ folder)
- **Mở folder**: Click **"📂"**

### Dữ liệu
- Projects: `projects.json` (tại thư mục app)
- Template: `../ProjectTemplate/ModProject_0hKMNX/`

## 🏗️ Kiến trúc

### CWindow Pattern - Tách biệt Business Logic và UI

```
MainWindow (UI)  →  MainWindowData (Business Logic)  →  Helpers/Models  →  JSON
```

**Cấu trúc**:
- `Windows/` - UI Layer (XAML + Event handlers)
- `WindowData/` - Business Logic Layer (Data + Methods)
- `Styles/` - Tái sử dụng styles (Colors, Buttons, TextBlocks, etc.)
- `Helpers/` - Utility functions
- `Models/` - Data structures

**AutoNotifiableObject**:
```csharp
[NotifyMethod(nameof(ValidateInput))]
public string ProjectName { get; set; }
// ValidateInput() tự động gọi khi ProjectName thay đổi
```

## 🛠️ Phát triển

### Thêm Window mới
```csharp
// WindowData
public class MyWindowData : CWindowData { ... }

// Window
public partial class MyWindow : CWindow<MyWindowData> { ... }
```

### Thêm Style
Tạo file `.xaml` trong `Styles/` và thêm vào `AppStyles.xaml`

## 🆘 Troubleshooting

- **Template not found**: Check `../ProjectTemplate/ModProject_0hKMNX/` exists
- **Build errors**: Restore NuGet packages, clean & rebuild
- **Cannot create project**: Check write permissions, change target directory

---