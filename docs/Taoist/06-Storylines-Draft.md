# Bản nháp cốt truyện NPC (chờ duyệt, chưa code)

Bản nháp dàn ý để duyệt. Chưa có lời thoại. Sau khi chốt dàn ý, lời thoại sẽ viết đủ EN / 简 / 繁 / 한 để duyệt như các drama trước, rồi mới code.

## 0. Cách chọn NPC đóng vai

- Mỗi vai có **điều kiện chọn**:
  - giới tính;
  - tuổi so với tuổi thọ, vì tu sĩ thọ dài nên "trẻ" hay "già" tính theo tỉ lệ;
  - ngoại hình: chỉ số `beauty`, theo bậc dung mạo của game (`RoleBeauty`);
  - tính cách: nội tại (`inTrait` 1–7) và ngoại tại (`outTrait` 8–19), dùng đúng bảng tính cách của game;
  - chính / tà: `standUp` / `standDown`;
  - cảnh giới so với người chơi;
  - một số vai cần tông môn hoặc thành trấn.
- **Cách chọn:**
  - Vai được gán khi tuyến truyện mở. Hệ thống chọn NPC còn sống, khớp điều kiện nhất (có chấm điểm), chưa giữ vai nào khác, không phải đạo lữ hay con cháu của người chơi.
  - Nếu không có ai đủ điều kiện, hệ thống tạo NPC mới (`AddUnit`) rồi chỉnh lại giới tính, tuổi, tính cách và cảnh giới. Ngoại hình do game sinh, nên vai nào cần dung mạo cao thì lấy lại ngẫu nhiên vài lần, chọn kết quả tốt nhất.
  - Tên của nhân vật là tên NPC trong thế giới. Lời thoại dùng `{name}`. Tên trong tài liệu này (Nguyệt Hàn, Lão Hoàng…) chỉ là tên gọi tạm.
- **NPC đóng vai vẫn sống như NPC bình thường:** già, đột phá, kết hôn, có thể chết.
  - Chết giữa chừng thì tuyến rẽ sang kết thúc tương ứng ("Hy sinh", "Người thừa kế"…), hoặc tuyến đóng lại và ghi vào Taoist log.
- **Giọng văn theo tính cách:** mỗi nhân vật có một "giọng" ghi bên dưới. Cùng một nhân vật thì giữ giọng thống nhất qua các hồi.
- **Nhịp độ:** mỗi hồi cách nhau vài năm trong game.
  - Hồi tiếp theo mở khi đủ điều kiện (thời gian, cảnh giới, địa điểm), kiểm tra hàng tháng, có thể rơi vào lúc bế quan hoặc skip.
  - Không có mốc thời gian bắt buộc: bỏ lỡ thì tuyến chờ.
- **Phần thưởng:** vừa phải để tránh lạm phát.
  - Chủ yếu là quan hệ (đồng minh, đạo lữ, kẻ thù), quyền lợi nhỏ (giảm giá, hàng gửi định kỳ), và một món đồ đặc trưng ở kết thúc.
  - Không cho exp lớn.
- **Tắt / bật:** tùy chọn S&M "Không có cốt truyện NPC" ở trang 3. Mỗi tuyến cũng có thể tắt riêng nếu bạn muốn.

## 1. Danh sách nhân vật

| # | Tạm gọi | Giới | Điều kiện chọn | Tính cách → giọng văn |
|---|---|---|---|---|
| 1 | **Nguyệt Hàn** (kiếm tu bị trục xuất) | Nữ | Đã sống 15–35 % tuổi thọ; dung mạo ≥ "Thanh tú"; nội tại Chính trực/Vô tư; ngoại tại Thù dai hoặc Chung thủy; chính đạo; căn cơ kiếm cao nhất trong các căn cơ vũ khí; cùng cảnh giới hoặc hơn người chơi 1 cảnh giới; thuộc tông môn | Nghiêm, ít lời, câu ngắn; lạnh lùng lúc đầu, ấm dần theo thân thiết |
| 2 | **Lão Hoàng** (thương nhân mắc nợ) | Nam | Đã sống ≥ 60 % tuổi thọ; nội tại Trung dung/Ích kỷ; ngoại tại Gia đình hoặc Danh tiếng; nhiều linh thạch; ở trấn có chợ | Lắm lời, hay than, nịnh khéo, pha hài; lúc nguy thì thật lòng |
| 3 | **Tô Tiểu Mạn** (luyện đan sư tập sự) | Nữ | Đã sống ≤ 15 % tuổi thọ; nội tại Nhân thiện; ngoại tại Tùy hứng; kỹ năng luyện đan cao nhất trong các kỹ nghệ; cảnh giới thấp hơn người chơi | Lanh lợi, nói nhanh, hay đùa, hay gây họa; xưng "tiểu muội" |
| 4 | **Thiết Ngưu** (thể tu chất phác) | Nam | Đã sống 15–40 % tuổi thọ; dung mạo thường; nội tại Chính trực; ngoại tại Nghĩa khí; căn cơ quyền / chưởng cao nhất | Thật thà, nói thẳng, ăn khỏe, hài kiểu ngây ngô; trọng tình anh em |
| 5 | **Hồng Liên phu nhân** (trưởng lão ma đạo, kẻ đứng sau) | Nữ | Đã sống 40–70 % tuổi thọ; dung mạo ≥ "Khuynh thành"; nội tại Tà ác/Cuồng tà; ngoại tại Quyền lực; tà đạo; cảnh giới cao hơn người chơi 1–2 cảnh giới | Văn vẻ, mềm mỏng, mỉa mai; đe dọa mà không lớn tiếng |
| 6 | **Mộ Dung Ca** (lãng tử cầm sư) | Nam | Đã sống 15–30 % tuổi thọ; dung mạo ≥ "Thanh tú"; ngoại tại Đa tình; nội tại Trung dung | Bay bổng, hay ngâm thơ, tán tỉnh; hài nhẹ nhàng, có nỗi buồn giấu kín |
| 7 | **Thạch bà bà** (lão bà hái thuốc ẩn cư) | Nữ | Đã sống ≥ 75 % tuổi thọ; nội tại Nhân thiện/Vô tư; ngoại tại Truyền thừa; không tông môn | Hiền, chậm rãi, nói ví von; gọi người chơi là "hài tử" |
| 8 | **A Ly** (cô bé mồ côi) | Nữ | Tạo mới khi tuyến mở (6–10 tuổi, chỉ là bản ghi như con chưa trưởng thành); 16 tuổi thì thành NPC | Rụt rè lúc nhỏ, lớn lên cứng cỏi; giọng phụ thuộc cách người chơi nuôi dạy |

Có 5 nữ, 3 nam; 3 nhân vật nghiêm túc (1, 5, 7), 3 nhân vật pha hài (2, 3, 4), 2 nhân vật nửa này nửa kia (6, 8).

## 2. Các tuyến truyện

### Tuyến A: "Kiếm lạnh" (Nguyệt Hàn; Hồng Liên đứng sau)
- **Mở đầu** (người chơi ≥ cảnh giới 2, ngoài trấn): gặp Nguyệt Hàn bị thương, đang bị đồng môn truy sát.
  - (a) Cứu nàng: vào tuyến.
  - (b) Giao nàng cho kẻ truy sát: nhận linh thạch, tuyến đi tới kết thúc "Kẻ phản bội".
  - (c) Bỏ đi: tuyến tạm đóng, 3 năm sau có thể gặp lại.
- **Hồi 1:** nàng bị sư phụ vu oan. Vai sư phụ do một trưởng lão có thật của tông môn đó đảm nhận.
  - Nhiệm vụ: tìm thư tay làm chứng cứ, mua ở chợ (hiện thành món hàng trong chợ của người chơi) hoặc lấy từ người bán hàng rong.
  - Lựa chọn: tin nàng, hoặc điều tra thêm (mất 1–2 năm, nhưng biết được Hồng Liên đứng sau).
- **Hồi 2** (5–10 năm sau): nàng đã mạnh lên và muốn báo thù.
  - (a) Cùng nàng đánh lên tông môn (trận lớn).
  - (b) Khuyên nàng buông bỏ.
  - (c) Bán tin cho sư phụ nàng.
- **Kết thúc:**
  - **Đạo lữ:** chọn (a), thắng, thân thiết cao. Nàng ngỏ ý kết đạo lữ, nối vào hệ thống gia tộc.
  - **Tông chủ:** chọn (a), thắng. Nàng lập tông, người chơi làm trưởng lão: giảm giá đổi cống hiến, đệ tử vào giúp trận.
  - **Nhập ma:** chọn (b) nhưng thất bại (thân thiết thấp). Nàng giết sư phụ rồi theo Hồng Liên; người chơi phải đối đầu (trận boss, nàng treo thưởng người chơi).
  - **Hy sinh:** người chơi sắp chết trong trận ở hồi 2, nàng chết thay. Người chơi nhận kiếm của nàng.
  - **Kẻ phản bội:** chọn (c), hoặc (b) ở mở đầu. Nàng thành kẻ thù, định kỳ truy sát người chơi.

### Tuyến B: "Món nợ của lão Hoàng" (Lão Hoàng; Hồng Liên đứng sau)
- **Mở đầu** (ở chợ): lão Hoàng bán rẻ một món đồ tốt. Hôm sau sát thủ ma giáo tìm đến đòi món đó.
  - (a) Trả đồ.
  - (b) Đánh.
  - (c) Chạy về tìm lão Hoàng.
- **Hồi 1:** lão thú nhận vay nợ ma giáo, nhờ người chơi giúp. Hệ thống vay ngân hàng có sẵn được dùng ở đây.
  - Bảo lãnh khoản vay: nếu lão không trả, người chơi gánh nợ.
  - Cho mượn tiền riêng.
  - Từ chối.
- **Hồi 2:** ma giáo đòi chiếm trấn của lão (chiến tranh trấn). Người chơi là trưởng trấn hoặc hộ vệ thì đây là trận giữ thành.
  - (a) Giữ thành.
  - (b) Thương lượng nộp cống.
  - (c) Bỏ mặc.
- **Kết thúc:**
  - **Đại phú thương:** chọn (a), thắng. Lão thành đồng minh hoặc trưởng trấn: giảm phí chợ, hàng gửi định kỳ.
  - **Người thừa kế:** lão chết già. Người chơi nhận các món lão đang bán và một lệnh mua giá tốt.
  - **Trấn sa ngã:** chọn (c), hoặc (a) mà thua. Trấn thành sào huyệt ma giáo: thuế cao, lính canh thù địch.
  - **Bị phản bội:** người chơi bảo lãnh xong thì lão bỏ trốn. Người chơi gánh nợ, lão bị treo thưởng, người chơi có thể tự săn.
  - **Ân nhân:** người chơi cho mượn tiền và lão trả đủ. Lão thành bạn thân, sau này cứu người chơi một lần khi bị truy nã.

### Tuyến C: "Lò đan nổ" (Tô Tiểu Mạn; giọng hài)
- **Mở đầu:** cô làm nổ lò đan, khói bay khắp trấn. Người chơi bị lôi vào làm "chuột thử thuốc".
  - Đồng ý: nhận một viên đan ngẫu nhiên, có thể có tác dụng phụ buồn cười trong vài tháng (tóc đổi màu, tâm trạng thất thường), nhưng vô hại.
  - Từ chối: cô bám theo nài nỉ thêm một lần nữa.
- **Hồi 1:** cô muốn thi vào hội luyện đan nhưng thiếu dược liệu hiếm. Người chơi đi tìm cùng cô (một kỳ ngộ ngoài trấn), gặp Thạch bà bà, nối sang tuyến E.
- **Hồi 2:** đơn thuốc của cô bị một đại sư danh tiếng đánh cắp.
  - Vạch mặt đại sư: tranh luận trước trấn, cần danh vọng.
  - Đánh tay đôi với đại sư.
  - Khuyên cô nhịn: cô buồn, thân thiết giảm.
- **Kết thúc:**
  - **Đại sư luyện đan:** cô bán đan cho người chơi với giá rẻ (giảm giá vĩnh viễn khi mua đan từ cô).
  - **Bạn đồng hành:** cô vào đội của người chơi miễn lương. Thỉnh thoảng có drama hài khi cô thử đan lên đồng đội.
  - **Lò nổ lần cuối:** cô thất bại và bỏ nghề, mở quán trà. Kết thúc nhẹ nhàng, thỉnh thoảng cô tặng đồ ăn hồi tâm trạng.

### Tuyến D: "Huynh đệ kết nghĩa" (Thiết Ngưu, Mộ Dung Ca; giọng hài)
- **Mở đầu:** Thiết Ngưu ăn quỵt ở quán trọ vì quên mang tiền, rồi đòi đánh tay đôi để trả nợ. Mộ Dung Ca đứng ngoài bình luận bằng thơ.
  - Trả giúp: Thiết Ngưu thân thiết cao.
  - Đánh với Thiết Ngưu: trận tay đôi, thắng hay thua đều được hắn nể.
- **Hồi 1:** Mộ Dung Ca nhờ người chơi và Thiết Ngưu giúp hắn tỏ tình. Người được tỏ tình là một NPC nữ có thật, nếu tuyến A đang chạy thì có thể chính là Nguyệt Hàn.
  - Giúp thật lòng.
  - Phá đám.
  - Người chơi tự tỏ tình với nàng (tạo tam giác tình cảm).
- **Hồi 2:** Thiết Ngưu bị tông môn ép đi đánh trận chiến tranh trấn ở phe đối địch với người chơi.
  - Thuyết phục hắn bỏ trận.
  - Đánh với hắn.
  - Đổi phe theo hắn.
- **Kết thúc:**
  - **Kết nghĩa ba người:** cả hai đều thân thiết cao. Hai người lập quan hệ anh em kết nghĩa, vào trận giúp người chơi như bạn bè, và báo thù nếu người chơi bị giết (nối với hệ thống truy nã).
  - **Mộ Dung thành đạo lữ:** người chơi nữ, hoặc S&M cho phép cùng giới. Hắn bỏ thói lăng nhăng (tính cách Đa tình đổi thành Chung thủy).
  - **Mất một người huynh đệ:** Thiết Ngưu chết ở hồi 2. Mộ Dung Ca viết khúc nhạc tiễn đưa, người chơi nhận "Ngưu quyền" (công pháp quyền của hắn).
  - **Tình địch:** người chơi tranh người với Mộ Dung Ca và thắng. Hắn rời đi, sau này xuất hiện ở trận chiến tranh trấn của phe kia.

### Tuyến E: "Vườn thuốc của bà bà" (Thạch bà bà, A Ly; giọng nghiêm và ấm)
- **Mở đầu** (có thể đến từ tuyến C, hoặc gặp ngẫu nhiên ở vùng núi): bà bà nhờ người chơi trông nom vườn thuốc.
  - Đồng ý: mỗi năm có một "lời dạy", +1 căn cơ ngẫu nhiên, tối đa 5 lần.
  - Từ chối.
- **Hồi 1:** bà bà nhặt được A Ly, một bé gái mồ côi, nhưng bà đã quá già để nuôi.
  - Nhận nuôi: A Ly thành con nuôi, theo hệ thống gia tộc như con ruột, 16 tuổi thành NPC và có thể làm người thừa kế.
  - Gửi vào tông môn: A Ly thành đệ tử tông môn, sau này là đồng minh.
  - Để bà bà tự nuôi: bà mất sớm hơn, A Ly lang thang.
- **Hồi 2** (bà bà mất, theo tuổi thọ thật): để lại "hạt giống đạo" và di nguyện.
  - Người chơi giữ vườn: mở một nơi bế quan tự chọn (nối với ý tưởng "nút bế quan theo ý" đang chờ chốt).
  - Hoặc trao vườn cho A Ly.
- **Kết thúc:**
  - **Truyền nhân:** A Ly lớn lên thành luyện đan sư hoặc kiếm tu, tùy cách nuôi.
    - Nếu là người thừa kế, A Ly được ưu tiên khi chơi tiếp bằng hậu duệ.
  - **Đứa trẻ lạc lối:** người chơi bỏ mặc A Ly. Cô lớn lên theo ma đạo dưới tay Hồng Liên, gặp lại ở tuyến A hoặc B như một kẻ thù có lý do.
  - **Vườn thuốc truyền đời:** vườn được giữ qua nhiều thế hệ.
    - Hậu duệ của người chơi vẫn nhận "lời dạy".
    - Đây là tuyến duy nhất kéo dài qua các thế hệ.

## 3. Liên kết giữa các tuyến

- **Hồng Liên phu nhân** là kẻ đứng sau tuyến A (vu oan Nguyệt Hàn) và tuyến B (ma giáo đòi nợ). Nếu cả hai tuyến tới hồi 2, sẽ mở **trận cuối**:
  - Người chơi cùng các đồng minh đã có (Nguyệt Hàn, lão Hoàng, Thiết Ngưu, Mộ Dung Ca, A Ly) đánh lên sào huyệt của Hồng Liên.
  - Những ai thành kẻ thù trong các tuyến sẽ đứng về phía Hồng Liên.
  - Kết thúc lớn: Hồng Liên chết hoặc bị phong ấn. Người chơi nhận "Hồng Liên kính" (một món đồ đặc trưng duy nhất), và các trấn bị ma giáo chiếm được giải phóng.
- Tuyến C dẫn sang tuyến E qua Thạch bà bà.
- Ở tuyến D, người được tỏ tình có thể là Nguyệt Hàn.
- Ở tuyến E, A Ly có thể sa vào ma đạo dưới tay Hồng Liên.
- Mọi lựa chọn và kết thúc được ghi vào Taoist log. Thêm một mục "Truyện đã trải qua" trong Taoist Settings để xem lại.

## 4. Cần bạn chốt

1. **Số tuyến:** làm cả 5 tuyến, hay làm trước A + B + trận cuối rồi thêm C, D, E sau?
2. **Thiếu NPC hợp vai:** cho tạo NPC mới hay đợi đến khi có NPC phù hợp?
3. **Thời gian:** có giới hạn không, ví dụ tuyến tự đóng nếu 30 năm không tiến triển?
4. **Tam giác tình cảm ở tuyến D:** giữ hay bỏ?
5. **Vườn thuốc ở tuyến E:** có gộp với "nút bế quan theo ý" (câu Q3 còn treo) không?
6. **Câu hỏi về bế quan còn treo:** "Trọng thương hôn mê" thay hẳn hình phạt mất exp khi thua, hay làm thành lựa chọn riêng trong S&M?
