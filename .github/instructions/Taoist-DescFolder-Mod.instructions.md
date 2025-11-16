---
applyTo: '**/3161035078/**/ModMain/Mod/*.cs'
---
Folder này chứa source code của các mods. Mỗi file trong thư mục này đại diện cho một mod cụ thể, bao gồm các tính năng và chức năng mở rộng cho ứng dụng hoặc trò chơi mà bạn đang phát triển.
Các mod trong thư mục này sử dụng events từ [ModEvent](**/3385996759/**/ModLib/Mod/Base/ModEvent.cs) để tương tác với hệ thống chính của ứng dụng hoặc trò chơi. Điều này cho phép các mod có thể lắng nghe và phản hồi các sự kiện xảy ra trong quá trình chạy của ứng dụng, từ đó cung cấp các tính năng bổ sung hoặc thay đổi hành vi mặc định.
Các mod phải khai báo [Cache](**/3385996759/**/ModLib/Attribute/CacheAttribute.cs) để khai báo và lưu trữ dữ liệu mod cho các phiên làm việc khác nhau. Điều này giúp mod có thể duy trì trạng thái và dữ liệu của mình giữa các lần khởi động lại ứng dụng hoặc trò chơi.
Các properties trong mod sẽ được lưu trữ dưới dạng json file trong thư mục [nE7UL2](C:\Users\*\AppData\LocalLow\guigugame\guigubahuang\mod\nE7UL2).