# 🌍 LocalText Translation System

Hệ thống tự động dịch các file `*localText.json` cho game GuiGuBaHuang ModLib với khả năng xử lý cả main files và locale files.

## 📁 Cấu trúc Project

```
Translate/
├── run.py                    # Script chính để chạy và dịch
├── translation_service.py    # Service xử lý dịch text
├── local_text_processor.py   # Processor xử lý localText files  
├── translate_utils.py        # Utilities cho translation logic
├── consts.py                 # Constants và cấu hình hệ thống
├── data_types.py             # Định nghĩa class và types
├── file_utils.py             # Utilities xử lý files và directories
├── json_utils.py             # Utilities xử lý JSON data
├── progressbar_utils.py      # Utilities cho progress bars và UI
└── README.md                 # Hướng dẫn sử dụng
```

## 🚀 Cách Sử Dụng

### Cài đặt Dependencies

```bash
pip install deep-translator
```

### Cú pháp cơ bản

```bash
python run.py --project <PROJECT_ID> --path <PATH> [OPTIONS]
```

### Các tham số

- `--project`: ID của project (vd: `3385996759`)
- `--path`: Đường dẫn tương đối trong ModConf
  - `.` - Xử lý toàn bộ thư mục ModConf
  - `game_localText.json` - Xử lý file cụ thể
  - `subfolder` - Xử lý thư mục con
- `--dry-run`: Chỉ hiển thị danh sách file, không thực hiện dịch
- `--create-locales`: Danh sách ngôn ngữ cần tạo (vd: `vi,es,fr` hoặc `all`)
- `--file-type`: Loại file cần xử lý
  - `main` - Chỉ xử lý main files (file gốc có en/ch/tc/kr)
  - `locale` - Chỉ xử lý locale files (file đã dịch trong thư mục con)
  - `both` - Xử lý cả hai loại (mặc định)

## 📖 Ví dụ Sử dụng

### 1. Xử lý toàn bộ ModConf

```bash
python run.py --project 3385996759 --path .
```

### 2. Xử lý file cụ thể

```bash
python run.py --project 3385996759 --path game_localText.json
```

### 3. Kiểm tra trước khi chạy (dry-run)

```bash
python run.py --project 3385996759 --path . --dry-run
```

### 4. Chọn ngôn ngữ cụ thể

```bash
python run.py --project 3385996759 --path . --create-locales vi,es,fr
```

### 5. Tạo tất cả ngôn ngữ

```bash
python run.py --project 3385996759 --path . --create-locales all
```

### 6. Chỉ xử lý main files

```bash
python run.py --project 3385996759 --path . --file-type main
```

### 7. Chỉ xử lý locale files

```bash
python run.py --project 3385996759 --path . --file-type locale
```

## 🎯 Khi nào sử dụng File Type?

### `--file-type main`
Sử dụng khi:
- ✅ Có file gốc mới cần dịch lần đầu
- ✅ Cập nhật nội dung trong main files và cần tạo lại locale files
- ✅ Muốn tạo bộ dịch hoàn toàn mới cho project
- ⚠️ **Lưu ý**: Sẽ xóa tất cả locale files cũ và tạo mới

### `--file-type locale`
Sử dụng khi:
- ✅ Chỉ muốn cập nhật/sửa chữa locale files hiện có
- ✅ Thêm ngôn ngữ mới vào locale files đã tồn tại
- ✅ Không muốn động đến main files hoặc làm mới locale files
- ⚠️ **Lưu ý**: Không tạo locale files mới, chỉ cập nhật có sẵn

### `--file-type both` (mặc định)
Sử dụng khi:
- ✅ Muốn xử lý toàn diện cả main và locale files
- ✅ Không chắc chắn cần xử lý loại nào
- ✅ Lần đầu chạy script trên project

## 🌍 Ngôn ngữ Hỗ trợ

Mặc định hệ thống hỗ trợ các ngôn ngữ sau:

- `vi` - Tiếng Việt (Vietnamese)
- `es` - Tiếng Tây Ban Nha (Spanish)
- `fr` - Tiếng Pháp (French)
- `de` - Tiếng Đức (German)
- `ru` - Tiếng Nga (Russian)
- `ja` - Tiếng Nhật (Japanese)
- `la` - Tiếng Latin (Latin)

Bạn có thể sử dụng bất kỳ mã ngôn ngữ ISO nào được Google Translate hỗ trợ.

## 📂 Cấu trúc File

### Main Files (Thư mục gốc ModConf)
Chứa các ngôn ngữ gốc: `en`, `ch`, `tc`, `kr`

```json
[
  {
    "id": 1,
    "en": "English text",
    "ch": "简体中文",
    "tc": "繁體中文", 
    "kr": "한국어"
  }
]
```

### Locale Files (Thư mục con)
Chứa bản dịch với key gộp `en|ch|tc|kr`

```json
[
  {
    "id": 1,
    "en|ch|tc|kr": "Translated text"
  }
]
```

## 📝 Development & Maintenance

### Thêm ngôn ngữ mới

Chỉnh sửa `DEFAULT_TARGET_LANGUAGES` trong `consts.py`:

```python
DEFAULT_TARGET_LANGUAGES = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la', 'pt']  # Thêm Portuguese
```

---

**Version**: 2.0 | **Updated**: August 2025  
**Dependencies**: `deep-translator>=1.9.0`

## 📄 License

Project này thuộc về GuiGuBaHuang ModLib community.