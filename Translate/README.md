# 🌍 LocalText Translation System

Hệ thống tự động dịch các file `*localText.json` cho game GuiGuBaHuang ModLib.

## 📁 Cấu trúc Project

```
Translate/
├── translate.py           # Script chính để chạy và dịch
├── consts.py              # Constants và cấu hình
├── data_types.py          # Định nghĩa class và types
├── file_utils.py          # Utilities xử lý files
├── json_utils.py          # Utilities xử lý JSON
└── README.md              # Hướng dẫn sử dụng
```

## 🚀 Cách Sử Dụng

### Cài đặt Dependencies

```bash
pip install deep-translator
```

### Cú pháp cơ bản

```bash
python translate.py --project <PROJECT_ID> --path <PATH> [OPTIONS]
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
python translate.py --project 3385996759 --path .
```

### 2. Xử lý file cụ thể

```bash
python translate.py --project 3385996759 --path game_localText.json
```

### 3. Kiểm tra trước khi chạy (dry-run)

```bash
python translate.py --project 3385996759 --path . --dry-run
```

### 4. Chọn ngôn ngữ cụ thể

```bash
python translate.py --project 3385996759 --path . --create-locales vi,es,fr
```

### 5. Tạo tất cả ngôn ngữ

```bash
python translate.py --project 3385996759 --path . --create-locales all
```

### 6. Chỉ xử lý main files

```bash
python translate.py --project 3385996759 --path . --file-type main
```

### 7. Chỉ xử lý locale files

```bash
python translate.py --project 3385996759 --path . --file-type locale
```

### 8. Kiểm tra chỉ main files (dry-run)

```bash
python translate.py --project 3385996759 --path . --file-type main --dry-run
```

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

## 🔄 Workflow Xử lý

1. **Phân tích**: Tìm và phân loại main files vs locale files
2. **Lọc**: Chỉ chọn loại file cần xử lý (theo `--file-type`)
3. **Dọn dẹp**: Xóa các thư mục locale cũ (chỉ khi xử lý main files)
4. **Xử lý Main**: Đọc main files và tạo locale files tương ứng
5. **Xử lý Locale**: Cập nhật locale files hiện có (nếu được chọn)
6. **Dịch**: Sử dụng Google Translate để dịch từ tiếng Anh
7. **Lưu**: Ghi các locale files với cấu trúc đã được tối ưu

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

## ⚡ Tính năng Nâng cao

### Xử lý Lỗi Network
- Tự động retry khi gặp lỗi mạng (tối đa 3 lần)
- Delay giữa các request để tránh rate limit
- Graceful handling khi bị gián đoạn (Ctrl+C)

### Tối ưu Performance
- Skip các text không cần dịch (số, ký tự đặc biệt)
- Batch processing cho hiệu quả tốt hơn
- Progress tracking và thống kê chi tiết

### Validation
- Kiểm tra cấu trúc JSON hợp lệ
- Tự động sửa lỗi format phổ biến
- Validation dữ liệu trước khi xử lý

## 🛠️ Cấu hình

Tất cả cấu hình được định nghĩa trong `consts.py`:

```python
# Ngôn ngữ mặc định
DEFAULT_TARGET_LANGUAGES = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la']

# Cấu hình dịch
TRANSLATION_CONFIG = {
    'max_retries': 3,
    'delay_between_requests': 0.2,
    'retry_delay': 1.0,
    'source_language': 'en'
}
```

## 📊 Output

Script sẽ hiển thị thông tin chi tiết về:
- Số lượng file được tìm thấy và phân loại
- Progress dịch theo từng file
- Thống kê cuối: số file xử lý, số text dịch, thời gian

```
🚀 Script Xử Lý LocalText.json
📦 Project: 3385996759
📂 Path: .
� File type: main
�🌍 Ngôn ngữ locale: vi, es, fr

📊 THỐNG KÊ FILE
📁 Tổng số file tìm thấy: 15
📄 Main files: 8
🌍 Locale files: 7
🎯 Loại xử lý: main
📋 Sẽ xử lý: 8 file
🌍 Ngôn ngữ target: vi, es, fr

--- Xử lý 8 main file ---
  📄 game_localText.json
    📝 45 text cần dịch
    ✅ vi: game_localText.json
    ✅ es: game_localText.json
    ✅ fr: game_localText.json

📊 KẾT QUẢ
📁 Tổng số file: 8
✅ Xử lý thành công: 8
🌍 Đã dịch: 360 text
❌ Lỗi dịch: 0 text
⏱️ Thời gian: 45.2s
```

## 🐛 Troubleshooting

### Lỗi thường gặp

1. **Không tìm thấy project**
   ```
   ❌ Không tìm thấy project: ../3385996759
   ```
   → Kiểm tra project ID và đảm bảo thư mục tồn tại

2. **Lỗi network khi dịch**
   ```
   Thử lại lần 2 cho 'example text...'
   ```
   → Script sẽ tự động retry, có thể do mạng chậm

3. **File JSON lỗi format**
   ```
   Lỗi đọc file game_localText.json: ...
   ```
   → Script có tự động sửa một số lỗi phổ biến

### Interrupt an toàn

Khi cần dừng script (Ctrl+C), hệ thống sẽ:
- Hiển thị progress hiện tại
- Lưu kết quả đã hoàn thành
- Cho phép resume từ nơi đã dừng

## 📝 Development

### Thêm ngôn ngữ mới

Chỉnh sửa `DEFAULT_TARGET_LANGUAGES` trong `consts.py`:

```python
DEFAULT_TARGET_LANGUAGES = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la', 'th', 'id']
```

### Thay đổi cấu hình dịch

Chỉnh sửa `TRANSLATION_CONFIG` trong `consts.py`:

```python
TRANSLATION_CONFIG = {
    'max_retries': 5,           # Tăng số lần retry
    'delay_between_requests': 0.1,  # Giảm delay để nhanh hơn
    'retry_delay': 2.0,         # Tăng delay khi retry
    'source_language': 'en'
}
```

## 📄 License

Project này thuộc về GuiGuBaHuang ModLib community.
