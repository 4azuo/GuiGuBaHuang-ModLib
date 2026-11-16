# Bản nháp lời thoại cốt truyện NPC — bản 2 (chờ duyệt, chưa code)

Dựa trên dàn ý đã chốt ở `06-Storylines-Draft.md`. Bản 2 kéo dài các tuyến theo góp ý: mỗi tuyến có 5–6 hồi, mỗi hồi có ít nhất 3 bước (cảnh, lựa chọn, trận đánh).

Đã chốt từ bản 1 (2026-11-16):
- Giọng văn và lời thoại: ok.
- Các nhánh thất bại "Kiếm gãy", "Con nợ biến mất", "Thi trượt", "Ma vực lan rộng", kết thúc "Bình yên" và kết thúc ẩn "Quỳ": ok.
- Vườn thuốc dùng làm nơi bế quan theo ý, **cần thân thiết với Thạch bà bà từ 300 trở lên** (xem E4).
- Các con số: ok.

Quy ước:
- `{p}` là tên người chơi. `{A}`, `{H}`… là tên NPC đóng vai.
- `▸` là lựa chọn. `→` là kết quả. *Nghiêng* là lời dẫn truyện.
- **Hạn** là thời hạn của hồi. Quá hạn thì tuyến rẽ sang nhánh thất bại.
- Mỗi hồi mở khi hồi trước xong và đủ điều kiện, được kiểm tra hàng tháng. Một hồi có thể rơi vào lúc skip/bế quan (drama sẽ ngắt skip).
- Mỗi bước dưới đây là một drama, hoặc một drama kèm trận đánh. Giữa các bước có thể cách nhau vài tháng.

Sau khi bạn duyệt: dịch 繁體 / 한국어 và 8 ngôn ngữ còn lại (dùng `Translate/ai_bridge.py`), rồi code.

---

## Tuyến A — "Kiếm lạnh" (Nguyệt Hàn = `{A}`, sư phụ = `{M}`, tông môn = `{S}`)

Giọng `{A}`: ít lời, câu ngắn, lạnh. Thân thiết cao thì mềm hơn, đôi khi gọi `{p}` bằng tên.

### A0. Gặp gỡ (người chơi từ cảnh giới 2, ngoài trấn; 0,3 % mỗi tháng)
1. **Truy sát.**
   - *EN:* A woman in torn sect robes stumbles out of the reeds, a broken sword in her hand. Three disciples of {S} follow, blades drawn.
   - *ZH:* 一名身着残破宗门道袍的女子从芦苇中踉跄而出，手握断剑。三名{S}弟子拔剑紧随其后。
   - **{A}:** "Stay out of this. It is not your fight." / “别插手。这不是你的事。”
   - **Disciple / 弟子:** "Traitor {A} stole the sect's sword manual. Hand her over, and the sect will remember your kindness." / “叛徒{A}盗走了宗门剑谱。把她交出来，宗门必有厚报。”
   - ▸ Fight the disciples / 与弟子交战 ▸ "She's yours." / “人归你们。” ▸ Walk away / 转身离开
2. **Sau trận** (nếu đánh và thắng).
   - **{A}:** "…Thank you. I will not forget a debt." / “……多谢。这份恩情，我不会忘。”
   - *EN:* She tears a strip of blue silk from her sleeve and presses it into your hand. "If you hear my name again, do not believe what they say."
   - *ZH:* 她从袖口撕下一条蓝绸，塞进你手里。“若再听到我的名字，别信他们说的。”
   - → Người chơi nhận vật phẩm cốt truyện "Dải lụa xanh". `{A}` thân thiết +100.
3. **Lệnh truy nã** (1–3 tháng sau, ở trấn gần nhất).
   - *EN:* A notice on the town gate: "Wanted — {A}, thief of {S}." Someone has drawn her face badly.
   - *ZH:* 城门上贴着告示：“通缉——{A}，{S}窃贼。”画像画得很糟。
   - ▸ Tear it down / 撕下告示 (trưởng trấn ghét người chơi −20) ▸ Leave it / 不管它
   - → Vào A1.

- **Giao nộp:** nhánh **Kẻ phản bội**.
  - *EN:* As they drag her away, her eyes find yours — a cold, patient promise.
  - *ZH:* 她被拖走时，目光落在你身上——冰冷而耐心的承诺。
- **Bỏ đi:** 3 năm sau gặp lại một lần.

### A1. Vết thương cũ (**hạn: 1 năm**)
1. **Tìm đến.**
   - *EN:* Night. Someone taps on your window: {A}, pale, a hand pressed to her side. "The wound will not close. Their sword was poisoned."
   - *ZH:* 深夜，有人轻叩你的窗：{A}脸色苍白，一手捂着腰侧。“伤口合不上。他们的剑上有毒。”
2. **Chữa thương:** cần một viên đan giải độc, hoặc một cây thảo dược từ cảnh giới của `{A}` trở lên.
   - ▸ Give her a pill / 给她丹药 ▸ Give her a herb / 给她药草 ▸ "I have nothing." (search the market) / “我身上没有。”（去市场找）
3. **Lời thật đầu tiên.**
   - **{A}:** "The manual was never stolen. It was sold. I saw who received it — a woman in red, with a lotus on her fan."
   - *ZH:* “剑谱从未被盗，是被卖了。我看见了接货的人——一个红衣女子，扇上绣着一朵莲。”
   - → Vào A2. Quá hạn mà không chữa: `{A}` tự khỏi nhưng thân thiết −100, vẫn vào A2.

### A2. Bức thư tay (**hạn: 3 năm**)
1. **Kế hoạch.**
   - **{A}:** "My master kept every letter. One of them went astray — the courier was robbed on the road to {T}. Find it."
   - *ZH:* “我师父每封信都留着。有一封送丢了——信使在去{T}的路上被劫。找到它。”
2. **Đi tìm:** bức thư nằm ở một trong hai chỗ.
   - Ở chợ của `{T}`, do một NPC đăng bán dưới tên "thư cũ".
   - Hoặc trong túi một tên cướp: đánh tên cướp ở vùng quanh `{T}`.
3. **Mở thư.**
   - *EN:* The letter bears {M}'s seal — and a red lotus pressed into the wax. {A} reads it twice, then folds it with steady hands. "One proof. They will say I forged it."
   - *ZH:* 信上盖着{M}的印——蜡封上压着一朵红莲。{A}读了两遍，手很稳地把信折好。“一份证据。他们会说是我伪造的。”
   - ▸ "Then we find a second." / “那就再找一份。” ▸ "Prove it was not forged." (investigate: +1 year, learn of Lady Honglian) / “先证明它不是伪造的。”（调查：多一年，得知红莲夫人）
   - → Vào A3.
- **Quá hạn: nhánh "Kiếm gãy".** `{A}` bị bắt về `{S}` và giam lại.
  - Người chơi có thể cướp ngục: một trận ở tông môn, thắng thì vào A3.
  - Bỏ mặc thì 5 năm sau `{A}` tự trốn ra và theo ma đạo (kết thúc Nhập ma).

### A3. Nhân chứng (**hạn: 2 năm**)
1. **Người gác kho.**
   - **{A}:** "Old Lu kept the sect's treasury. He saw the exchange. He fled the sect a month later — afraid."
   - *ZH:* “老陆管着宗门的库房。他看见了交易。一个月后他就逃出宗门了——吓坏了。”
   - Lão Lục là một NPC già có thật, ở một trấn khác.
2. **Thích khách.** Khi người chơi đến, thích khách ma giáo cũng vừa tới.
   - *EN:* The door is already broken. Two masked killers stand over Old Lu.
   - *ZH:* 门已经被砸开。两个蒙面杀手站在老陆身前。
   - → Trận đánh. Người chơi phải bảo vệ Lão Lục.
3. **Lời khai.**
   - **Old Lu / 老陆:** "I will speak before the elders — if you keep me alive until then." / “我会在长老面前作证——只要你们让我活到那一天。”
   - → Vào A4.
- **Lão Lục chết** (thua trận hoặc quá hạn): nhánh "Sổ kho".
  - Người chơi lẻn vào kho của `{S}` lấy sổ ghi chép: một trận với lính gác tông môn, rủi ro bị treo thưởng.
  - Lấy được sổ thì vào A4.

### A4. Mài kiếm (5–10 năm sau A3; có thể qua bế quan)
1. **Lời hẹn.**
   - **{A}:** "Proof is not enough against an elder. I must be strong enough to stand before him. Spar with me, {p}."
   - *ZH:* “对付长老，光有证据不够。我必须强到能站在他面前。陪我练剑，{p}。”
   - → Đấu tập với `{A}` (trận không chết người). Mỗi trận thắng: thân thiết +50, `{A}` +exp.
2. **Thanh kiếm mới.**
   - **{A}:** "My sword broke the day I left. I need a blade that will not." / “离开那天我的剑断了。我需要一把不会断的剑。”
   - ▸ Give a sword (an artifact of her grade or higher) / 赠剑 ▸ Pay a smith (spirit stones) / 请铁匠打造（灵石） ▸ "Your will is the blade." / “你的意志就是剑。”
   - → Tặng kiếm: `{A}` dùng kiếm đó ở trận cuối, thân thiết +100.
3. **Đột phá.**
   - *EN:* Thunder rolls over the mountain. When the clouds part, {A} stands in the rain, her eyes clear for the first time. "I am ready."
   - *ZH:* 雷声滚过山巅。云开之时，{A}站在雨中，眼神第一次如此清明。“我准备好了。”
   - `{A}` lên một cảnh giới.

### A5. Trở về `{S}` (**hạn: 2 năm**)
1. **Trước cổng tông môn.**
   - **{A}:** "I will go to {S} and take back my name — with this sword, or with my life." / “我要去{S}，夺回我的名字——用这把剑，或用这条命。”
   - ▸ "Then we go together." / “那我们一起去。” ▸ "Let it go." / “放下吧。” ▸ (secretly) Warn {M} / （暗中）向{M}通风报信
2. **Trước các trưởng lão** (nếu cùng đi).
   - **{M}:** "A thief and her accomplice. Elders, you see the shamelessness." / “一个窃贼和她的同党。诸位长老，看看这无耻之徒。”
   - ▸ Show the letter and the witness / 出示书信与证人 ▸ Challenge {M} to a duel / 向{M}挑战
   - Có cả thư và nhân chứng (hoặc sổ kho): các trưởng lão đứng ngoài, chỉ đánh với `{M}`. Thiếu bằng chứng: đánh cả hộ pháp lẫn `{M}`.
3. **Trận với `{M}`**, rồi một lựa chọn.
   - **{A}:** "He is on his knees. Tell me, {p} — what is mercy worth?" / “他跪下了。告诉我，{p}——慈悲值多少？”
   - ▸ "Let the sect judge him." / “交给宗门审判。” ▸ "Your choice, not mine." / “你自己决定。”
   - → Kết thúc theo thân thiết (bảng dưới).

### Kết thúc A
| Kết thúc | EN | ZH | Điều kiện / hiệu ứng |
|---|---|---|---|
| Đạo lữ | "When I had nothing, you stood beside me. Walk the rest of the road with me, {p}." | “我一无所有时，是你站在我身边。余下的路，与我同行吧，{p}。” | A5 thắng, thân thiết ≥ 500: đề nghị kết đạo lữ. |
| Tông chủ | "I will build a sect where no disciple is betrayed by a master. Its first elder should be you." | “我要建立一个师父绝不背叛弟子的宗门。它的第一位长老，该是你。” | A5 thắng, thân thiết < 500: giảm giá đổi cống hiến, đệ tử vào giúp trận. |
| Bình yên | "The sword is quieter now. So am I." | “剑静了。我也静了。” | A5 chọn "buông bỏ", thân thiết ≥ 300: mỗi năm nhận một thanh kiếm. |
| Nhập ma | "Mercy was a lie they taught me. Lady Honglian taught me the truth." | “慈悲是他们教我的谎言。红莲夫人教会了我真相。” | Buông bỏ khi thân thiết < 300, hoặc "Kiếm gãy" bị bỏ mặc: `{A}` thành boss ở trận cuối. |
| Hy sinh | "…Your turn… to live." | “……换你……活下去。” | Người chơi sắp chết trong trận A5: nhận kiếm của `{A}`. |
| Kẻ phản bội | "You sold me once. I will collect the debt." | “你出卖过我一次。这笔账，我会来讨。” | Mỗi 3–5 năm `{A}` truy sát một lần. |

---

## Tuyến B — "Món nợ của lão Hoàng" (lão Hoàng = `{H}`, trấn của lão = `{T}`)

Giọng `{H}`: lắm lời, hay than, nịnh khéo, pha hài. Lúc nguy thì thật lòng.

### B0. Món hời (người chơi mở chợ ở `{T}`; 1 % mỗi lần mở)
1. **Lời chào hàng.**
   - **{H}:** "This [{item}] — worth three times the price, I swear on my grandmother's tomb. For you? Half." / “这件[{item}]——值三倍价钱，我拿我奶奶的坟发誓。给您？半价！”
   - ▸ Buy it / 买下 ▸ "Your grandmother must be very busy." / “你奶奶的坟怕是很忙。”
2. **Kẻ đòi đồ** (hôm sau).
   - *EN:* Two masked men block your path. "That [{item}] belongs to the Red Lotus. Return it, and keep your hands."
   - *ZH:* 两个蒙面人拦住你。“那件[{item}]是红莲教的东西。还回来，手还能留着。”
   - ▸ Return it / 交还 ▸ Fight / 交战 ▸ Run to {H} / 去找{H}
3. **Lão Hoàng run rẩy.**
   - **{H}:** "They came? Already? Oh heavens, oh ancestors, oh my aching knees—" / “他们来了？这么快？老天爷，列祖列宗，我这老膝盖——”
   - → Vào B1. Nếu trả đồ: tuyến dừng 2 năm rồi mới vào B1.

### B1. Lời thú nhận (**hạn: 2 năm**)
1. **Chỗ trốn.** `{H}` trốn trong kho hàng; người chơi phải tìm (chỗ trốn ngẫu nhiên trong `{T}` hoặc trấn bên cạnh).
2. **Câu chuyện.**
   - **{H}:** "When my caravan sank, I borrowed from the Red Lotus. Ten years of interest… I sold their goods to pay them back. Badly, as you saw."
   - *ZH:* “商队沉了那年，我向红莲教借了钱。十年利滚利……我卖他们的货来还债。卖砸了，你也看到了。”
3. **Lời nhờ.**
   - **{H}:** "Vouch for me at the bank? Just a signature! What could go wrong?" / “去钱庄给我作个保？就签个名！能出什么事？”
   - ▸ Vouch for him / 为他作保 ▸ Lend him spirit stones yourself / 自己借他灵石 ▸ Refuse / 拒绝
   - → Vào B2 với khoản tiền tương ứng.

### B2. Chuyến hàng cuối cùng (**hạn: 1 năm**)
1. **Kế hoạch.**
   - **{H}:** "One last caravan to {T2}. Silk, herbs, a little spirit jade. If it arrives, I pay everyone. If not… I become a very sincere monk." / “最后一趟去{T2}的商队。丝绸、药材、一点灵玉。货到了，我把债都还清。到不了……我就去当个很虔诚的和尚。”
2. **Hộ tống.** Đi cùng đoàn hàng (`{H}` di chuyển tới `{T2}`), giữa đường bị phục kích: trận với cướp, hoặc thích khách ma giáo nếu đã từ chối ở B0.
3. **Chia lời.**
   - Đến nơi: `{H}` trả một phần nợ.
   - **{H}:** "Your share. Don't count it in front of me, I'll cry." / “你的那份。别当着我面数，我会哭的。”
   - Người chơi nhận 10 % lợi nhuận.
- **Quá hạn hoặc đoàn hàng bị cướp: nhánh "Con nợ biến mất".** Nợ tăng gấp đôi và `{H}` biến mất.
  - Tìm được `{H}` ở một trấn xa thì quay lại B3, và lão trả nợ bằng hàng.

### B3. Cuốn sổ đỏ (**hạn: 2 năm**)
1. **Sứ giả.**
   - *EN:* A man in red silk visits {H}'s shop. "The Lady sends her regards. Interest is due."
   - *ZH:* 一个身着红绸的男人走进{H}的铺子。“夫人问候您。利息该交了。”
2. **Bí mật.**
   - **{H}:** "I saw his ledger. Not just me — half the merchants of this region owe her. She is buying the towns, one debt at a time." / “我瞥见了他的账本。不止我——这一带一半的商人都欠她的钱。她在一笔债一笔债地买下这些城。”
   - ▸ Steal the ledger (fight the envoy's guards) / 夺取账本（与使者护卫交战） ▸ Pay the interest / 付利息 ▸ Stall him / 拖延他
3. **Hậu quả.**
   - Lấy được sổ: mở trước trận cuối; các thương nhân trong sổ thành đồng minh, giảm giá ở chợ của họ.
   - Trả lãi: yên ổn 2 năm.
   - Kéo dài thời gian: vào thẳng B4.

### B4. Ma giáo đòi trấn (**trận chiến tranh trấn tiếp theo, tối đa 3 năm**)
1. **Cờ đỏ.**
   - *EN:* Red banners rise outside {T}. The Red Lotus demands the town as payment for {H}'s debt.
   - *ZH:* {T}城外升起红旗。红莲教要以整座城抵{H}的债。
2. **Chuẩn bị.**
   - **{H}:** "I have savings. Secret savings. Very secret. Take them — hire guards, buy arrows, anything!" / “我有积蓄。私房钱。很私的那种。拿去——雇人、买箭，什么都行！”
   - ▸ Hire guards (town budget +) / 招募守卫（城防预算增加） ▸ Negotiate a tribute / 谈判纳贡 ▸ Stay out of it / 袖手旁观
3. **Trận giữ thành** (nếu không nộp cống và không bỏ mặc).
   - Thắng: kết thúc **Đại phú thương**.
   - Thua hoặc bỏ mặc: kết thúc **Trấn sa ngã**.
   - Nộp cống: trấn mất 30 % ngân sách mỗi năm trong 5 năm, B4 có thể lặp lại.

### B5. Hậu sự (sau B4, hoặc khi `{H}` chết già)
1. **Bữa rượu.**
   - **{H}:** "You know, {p}, in forty years of trade you are my only customer who never haggled. Fool." / “你知道吗，{p}，我做了四十年买卖，你是唯一一个从不还价的客人。傻子。”
2. **Món quà.** Người chơi nhận một vật phẩm quý theo cảnh giới.
3. **Kết thúc** theo các lựa chọn trước (bảng dưới).

### Kết thúc B
| Kết thúc | EN | ZH | Hiệu ứng |
|---|---|---|---|
| Đại phú thương | "From today, your coin is no good in my shops. Well — half good." | “从今天起，你在我铺子里的钱不好使了。嗯——半价好使。” | Giảm phí chợ, mỗi năm gửi hàng. |
| Người thừa kế | "To my only honest customer…" (letter) | “致我唯一诚实的客人……”（信） | `{H}` chết già: người chơi nhận các món lão đang bán, kèm một lệnh mua giá tốt. |
| Trấn sa ngã | — | — | Trấn thành sào huyệt ma giáo: thuế ×2, lính canh thù địch. |
| Bị phản bội | "Sorry, friend. A man must live." (note) | “对不住了朋友。人总得活下去。”（字条） | Người chơi gánh nợ; `{H}` bị treo thưởng. |
| Ân nhân | "You trusted a fool. The fool will not forget." | “你信了一个傻瓜。傻瓜不会忘恩。” | Một lần duy nhất: `{H}` trả tiền xóa lệnh truy nã của người chơi. |

---

## Tuyến C — "Lò đan nổ" (Tô Tiểu Mạn = `{X}`, đại sư = `{G}`; giọng hài)

### C0. Chuột thử thuốc (người chơi vào trấn có `{X}`; 1 % mỗi lần vào)
1. **BÙM.**
   - *EN:* BOOM. Purple smoke rolls down the street. A girl with singed eyebrows waves at you through the haze.
   - *ZH:* 轰！紫烟滚滚漫过街道。一个眉毛烧焦的少女在烟雾中向你挥手。
   - **{X}:** "You look healthy. Want to test a pill? Ninety percent safe! Eighty. Probably." / “你看起来很健康。想试颗丹药吗？九成安全！八成。大概。”
   - ▸ "…Fine." / “……行吧。” ▸ Back away slowly / 慢慢后退
2. **Tác dụng phụ** (nếu thử).
   - *EN:* Your hair turns lavender for three months. {X} takes notes, delighted.
   - *ZH:* 你的头发变成了淡紫色，持续三个月。{X}兴奋地做着记录。
   - Đặc tính tạm "Tóc tím", tâm trạng +10.
3. **Đền bù.**
   - **{X}:** "Sorry sorry sorry! Here, a real pill. Works every time. Mostly." / “对不起对不起！给，一颗真丹药。每次都灵。基本上。”
   - Nhận một viên đan theo cảnh giới. Vào C1.

### C1. Sen nguyệt sương (**hạn: 2 năm**)
1. **Kỳ thi sắp tới.**
   - **{X}:** "The Alchemy Guild exam is next spring. I need a Moonfrost Lotus. One valley. One very large snake. Hehe." / “丹会考核在明年春天。我需要一株月霜莲。一个山谷。一条很大很大的蛇。嘿嘿。”
2. **Bản đồ.** Mua bản đồ thung lũng từ một người bán thảo dược, hoặc đổi bằng một cây thảo dược.
3. **Thung lũng.**
   - Đánh con mãng xà và hái sen.
   - Ở thung lũng gặp Thạch bà bà, mở tuyến E.
   - **Thạch bà bà / 石婆婆:** "Young ones, always in a hurry. Take the lotus — and leave the roots." / “年轻人，总是这么急。莲花拿去吧——根留下。”
- **Quá hạn: nhánh "Thi trượt".** `{X}` tự đi một mình, trở về bị thương.
  - Người chơi chăm sóc `{X}` (tốn đan dược) thì vào C2 muộn 1 năm.
  - Không chăm sóc thì kết thúc **Lò nổ lần cuối**.

### C2. Kỳ thi (**mùa xuân tới**)
1. **Lệ phí.**
   - **Examiner / 考官:** "Furnace fee: {0} spirit stones." / “丹炉费：{0}灵石。”
   - {X} *looks at you with enormous eyes.* / {X}*用一双大眼睛望着你。*
   - ▸ Pay / 付钱 ▸ "You have savings." / “你有积蓄。”
2. **Ba lần điều lửa:** `{X}` hỏi ý người chơi ba lần (lửa lớn / lửa nhỏ / thêm sen). Đúng từ 2/3 trở lên thì đỗ.
   - **{X}:** "Big fire or small fire? Quick, quick, it's bubbling!" / “大火还是小火？快快快，冒泡了！”
3. **Kết quả.**
   - **Đỗ:** "I PASSED! Did you see? Did everyone see?!" / “我考过了！你看到了吗？大家都看到了吗？！”
   - **Trượt:** "…Next year. I'll do it next year." / “……明年。我明年一定行。” Thi lại năm sau.

### C3. Đơn thuốc bị đánh cắp (**hạn: 1 năm**)
1. **Phát hiện.**
   - **{X}** (*no jokes for once* / *这一次没有玩笑*): "Grandmaster {G} presented my recipe to the Guild. My recipe. He laughed when I said so." / “{G}大宗师把我的丹方献给了丹会。我的丹方。我说出来时，他笑了。”
2. **Thu thập bằng chứng.** Hỏi hai học đồ khác của Hội luyện đan. Mỗi người cần một ân huệ: một món đồ, hoặc một trận đấu với kẻ bắt nạt họ.
3. **Đối chất.**
   - ▸ Expose him before the Guild (needs both apprentices, or 500 reputation) / 在丹会当众揭穿他 ▸ Duel him / 与他决斗 ▸ "Let it go." / “算了吧。”
   - "Bỏ qua": thân thiết −100, kết thúc **Lò nổ lần cuối**.

### C4. Đại hội luyện đan (năm sau C3)
1. **Lời khiêu chiến.**
   - **{G}:** "Little thief, face me at the Grand Gathering. The loser leaves alchemy forever." / “小贼，丹道大会上与我一较高下。输的人永离丹道。”
2. **Phá hoại.**
   - Đêm trước đại hội có kẻ lẻn vào phá lò của `{X}`: trận với kẻ phá hoại.
   - Thua thì `{X}` thi với lò cũ, tỉ lệ thắng thấp hơn.
3. **Đại hội.**
   - *EN:* Two furnaces, one hall, a thousand eyes. {X}'s pill rises from the flames, glowing like a small moon.
   - *ZH:* 两座丹炉，一座大殿，千双眼睛。{X}的丹药从火中升起，像一轮小小的月亮。
   - Thắng thì kết thúc theo thân thiết (bảng dưới).

### Kết thúc C
| Kết thúc | EN | ZH | Hiệu ứng |
|---|---|---|---|
| Đại sư luyện đan | "Master {X}'s pills — for you, at cost. For everyone else, double!" | “{X}大师的丹药——给你，成本价。给别人，翻倍！” | Mua đan của `{X}` giảm 30 %. |
| Bạn đồng hành | "I'm coming with you. Someone has to test pills on your enemies." | “我跟你走。总得有人拿你的敌人试药。” | Thân thiết ≥ 300: `{X}` vào đội không cần lương. |
| Lò nổ lần cuối | *A small teahouse opens. The tea is excellent. Nothing explodes.* | *一家小茶馆开张了。茶很好。什么都没炸。* | Thỉnh thoảng `{X}` gửi đồ ăn giúp hồi tâm trạng. |

---

## Tuyến D — "Huynh đệ kết nghĩa" (Thiết Ngưu = `{N}`, Mộ Dung Ca = `{C}`, người được tỏ tình = `{L}`; giọng hài; giữ tam giác)

### D0. Mười một bát mì (quán trọ của trấn có cả `{N}` và `{C}`)
1. **Ăn quỵt.**
   - **Innkeeper / 掌柜:** "Eleven bowls of noodles and no money?!" / “吃了十一碗面，一文钱没有？！”
   - **{N}:** "I can pay with a fight! A good one!" / “我可以打一架抵账！打得很精彩那种！”
   - **{C}** (*strumming* / *拨着琴弦*): "Eleven bowls, one fool, no coin — a tragedy in three verses." / “十一碗面，一个傻子，没有铜板——三段式悲剧。”
2. **Lựa chọn.**
   - ▸ Pay for him / 替他付账 ▸ "Fight me, then." / “那就和我打。”
   - → Trả tiền: `{N}` thân thiết +150. Đánh: `{N}` thân thiết +100, thắng hay thua đều được.
3. **Rượu.**
   - **{C}:** "A fool, a hero and a poet walk into an inn. I'll write it down." / “一个傻子、一个英雄和一个诗人走进客栈。我要把它写下来。”
   - Mở D1.

### D1. Săn yêu thú (**hạn: 1 năm**)
1. **Lời mời.**
   - **{N}:** "There's a beast eating the farmers' oxen. Three of us — easy! Ca, you can sing at it." / “有头妖兽在吃农户的牛。我们三个——轻松！阿歌，你可以对着它唱歌。”
2. **Trận** cùng `{N}` và `{C}` (đồng minh) với một yêu thú theo cảnh giới.
3. **Bên đống lửa.**
   - **{N}:** "My sect wants me to fight in their wars. I just want to punch bandits." / “我的宗门要我去打他们的仗。我只想揍土匪。”
   - **{C}:** "I write love poems because the other kind hurts too much." / “我写情诗，因为另一种诗太疼了。”
   - ▸ "Then swear brotherhood with me." / “那就和我结拜。” (cần thân thiết ≥ 200 với cả hai) ▸ "Get some sleep." / “睡吧。”

### D2. Bốn mươi hai bài thơ (**hạn: 1 năm**)
1. **Lời nhờ.**
   - **{C}:** "I have written forty-two poems for {L}. She has read none. I need a… strategist." / “我为{L}写了四十二首诗。她一首都没读。我需要一位……军师。”
2. **Món quà.** Chọn một món quà để gửi kèm thơ. Món hợp sở thích của `{L}` thì thân thiết giữa `{C}` và `{L}` tăng gấp đôi.
3. **Lời tỏ tình.**
   - ▸ Help sincerely / 真心帮他 ▸ Sabotage / 暗中搅局 ▸ Court {L} yourself / 自己去追求{L}
   - Tự tỏ tình: `{L}` chọn người có thân thiết cao hơn.
     - Người chơi thắng: kết thúc **Tình địch** (D4 vẫn diễn ra).
     - `{C}` thắng: `{C}` mời người chơi dự cưới.

### D3. Món nợ máu của Mộ Dung (**hạn: 2 năm**)
1. **Bí mật.**
   - **{C}** (*no music tonight* / *今晚没有琴声*): "My family were merchants. They refused the Red Lotus's loan. One night, the house burned with them in it." / “我家是商人。他们拒绝了红莲教的借款。一天夜里，房子连同他们一起烧了。”
2. **Dấu vết.** Theo dấu một thích khách ma giáo tới một ngôi miếu bỏ hoang: trận với thích khách.
3. **Lựa chọn.**
   - **Assassin / 刺客** (*dying* / *垂死*): "The Lady… will… burn you too." / “夫人……会把你们……也烧了。”
   - ▸ "We'll end her together." / “我们一起终结她。” ▸ "Revenge will not bring them back." / “复仇换不回他们。”
   - → Cùng báo thù: `{C}` thành đồng minh ở trận cuối.

### D4. Hai chiến tuyến (khi có chiến tranh trấn mà `{N}` đứng phe đối địch)
1. **Gặp nhau trên chiến trường.**
   - **{N}:** "Brother… my sect ordered me. I can't run." / “兄弟……宗门下了令。我不能逃。”
   - ▸ "Lower your fists and walk away with me." / “放下拳头，跟我走。” ▸ "Then fight me, and don't hold back." / “那就和我打，别留手。” ▸ Switch sides / 倒戈与他并肩
2. **Đánh tay đôi** (nếu không đi cùng được): thân thiết với `{N}` từ 300 trở lên thì hắn nương tay.
3. **Sau trận.**
   - `{N}` còn sống: "Your fists are heavier than my sect's orders." / “你的拳头比宗门的命令还重。” → kết thúc **Kết nghĩa ba người** nếu `{C}` cũng thân thiết từ 300 trở lên.
   - `{N}` chết: kết thúc **Mất một người huynh đệ**.

### Kết thúc D
| Kết thúc | EN | ZH | Hiệu ứng |
|---|---|---|---|
| Kết nghĩa ba người | "Heaven above, earth below — three of us, one life!" | “皇天在上，后土在下——我们三人，同生共死！” | Hai người vào trận giúp người chơi; nếu người chơi bị giết thì họ treo thưởng kẻ giết. |
| Mộ Dung thành đạo lữ | "Forty-three poems. This one, you'll read." | “第四十三首。这一首，你会读的。” | Người chơi nữ, hoặc S&M cho phép cùng giới; tính cách Đa tình đổi thành Chung thủy. |
| Mất một người huynh đệ | "He always said the noodles here were the best." | “他总说这里的面最好吃。” | Nhận công pháp "Ngưu quyền". |
| Tình địch | "You won her. Next time we meet, it's on the battlefield." | “你赢了她。下次见面，就在战场上。” | `{C}` đứng phe địch trong chiến tranh trấn. |

---

## Tuyến E — "Vườn thuốc của bà bà" (Thạch bà bà = `{O}`, A Ly = `{Y}`; giọng nghiêm và ấm)

**A Ly lúc nhỏ:** không có NPC và không có chân dung, chỉ là một bản ghi như con chưa trưởng thành.
- Lời của A Ly hiện như **lời dẫn truyện** trên ảnh nền, hoặc do `{O}` kể lại.
- 16 tuổi A Ly thành NPC (tạo mới, nữ, ngoại hình do game sinh), từ đó có chân dung trong drama.

### E0. Vườn trên núi (đến từ tuyến C, hoặc gặp ở vùng núi; 0,3 % mỗi tháng)
1. **Lời nhờ.**
   - **{O}:** "Child, these old knees cannot reach the high terraces anymore. Will you keep my garden for a while?" / “孩子，这双老腿再也爬不上高处的药圃了。你愿意替我照看一阵园子吗？”
   - ▸ "Of course, grandmother." / “当然，婆婆。” ▸ "I have no time for gardens." / “我没空种园子。”
2. **Ngày đầu làm vườn.**
   - Nhổ cỏ, tưới cây, đuổi lũ khỉ ăn trộm đào (trận vui với lũ khỉ yêu).
   - `{O}` thân thiết +100.
3. **Lời dạy đầu tiên.**
   - **{O}:** "A sword is like a root: it grows deep before it grows tall." / “剑如根：先往深处长，再往高处长。”
   - +1 căn cơ ngẫu nhiên. Từ đó mỗi năm ghé vườn được thêm một lời dạy, tối đa 5 lần, mỗi lần thân thiết +50.

### E1. Đứa trẻ trong tro (**hạn: 1 năm**)
1. **Gặp A Ly.**
   - *EN:* A thin girl hides behind {O}'s skirts, clutching a wooden spoon like a sword.
   - *ZH:* 一个瘦小的女孩躲在{O}身后，像握剑一样攥着一把木勺。
2. **Câu chuyện.**
   - **{O}:** "Her village burned. Red banners, the survivors said. I am too old to raise her, and too old to say no to her." / “她的村子烧了。幸存的人说，有红色的旗。我老得养不动她，也老得拒绝不了她。”
3. **Lựa chọn.**
   - ▸ Adopt her / 收养她 ▸ Send her to a sect / 送她去宗门 ▸ Leave her with you, grandmother / 留给婆婆照顾
   - → Nhận nuôi: `{Y}` thành con nuôi trong hệ thống gia tộc (con chưa trưởng thành).

### E2. A Ly lớn dần (ba khoảnh khắc lúc 8, 12 và 15 tuổi; mỗi lần phải ghé thăm trong vòng 1 năm)
1. **8 tuổi.**
   - *EN:* {Y} burned the rice again. She says it was a "fire technique".
   - *ZH:* {Y}又把饭烧糊了。她说那是“火系功法”。
   - ▸ Teach her to cook / 教她做饭 (hướng đan sư) ▸ Teach her a stance / 教她扎马步 (hướng kiếm tu)
2. **12 tuổi.**
   - *EN:* {Y} came home with a black eye. "He called you a nobody. I corrected him."
   - *ZH:* {Y}带着一只青眼圈回家。“他说你是无名小卒。我纠正了他。”
   - ▸ Scold her / 责备她 ▸ Laugh / 大笑 ▸ Teach her when to fight / 教她何时该出手
3. **15 tuổi.**
   - *EN:* {Y} asks what her parents were like. {O} looks at you.
   - *ZH:* {Y}问起她的父母是什么样的人。{O}看向了你。
   - ▸ Tell the truth (red banners) / 说出真相（红旗） ▸ "They loved you." / “他们很爱你。”
   - Nói thật: `{Y}` sẽ muốn báo thù và tham gia trận cuối.

Hướng phát triển của `{Y}` cộng dồn qua 3 lần: đan sư / kiếm tu / tự do. Bỏ lỡ cả 3 lần thì vào nhánh **Đứa trẻ lạc lối**.

### E3. Bà bà lâm bệnh (khi `{O}` còn ≤ 3 năm thọ)
1. **Tin xấu.**
   - **{Y}** (*voice* / *旁白*): "Grandmother coughs at night now. She says it's the wind." / “婆婆夜里咳嗽了。她说是风。”
2. **Cây cỏ kéo dài thọ.**
   - Tìm một cây "Trường thọ thảo": một kỳ ngộ ngoài trấn, có trận với yêu thú canh giữ.
   - Hoặc mua ở chợ với giá rất cao.
3. **Lựa chọn của bà.**
   - **{O}:** "You brought this for me? …Child. A garden needs old trees to fall, so young ones get the light." / “你为我带来的？……孩子。园子里，老树要倒下，小树才晒得到太阳。”
   - ▸ Insist / 坚持让她服下 (bà sống thêm 10 năm, thân thiết +200) ▸ Respect her wish / 尊重她的心愿 (lời dạy cuối, +3 căn cơ)

### E4. Di nguyện (khi `{O}` mất theo tuổi thọ thật)
1. **Dưới gốc lê.**
   - *EN:* {O} sleeps under the old pear tree and does not wake. In her hand, a seed wrapped in silk.
   - *ZH:* {O}在老梨树下睡去，再没醒来。手中握着一颗用丝绢包好的种子。
2. **Lá thư.**
   - *EN:* "To the one who kept my garden: plant it where you will rest."
   - *ZH:* “给替我照看园子的人：把它种在你歇息的地方。”
   - ▸ Keep the garden / 留下药圃 ▸ Give it to {Y} / 交给{Y}
3. **Vườn thuốc.**
   - **Giữ vườn:** mở **nơi bế quan theo ý**, chọn 1–20 năm, trả linh thạch, nhận exp theo năm, bị ngắt như skip.
     - **Chỉ mở khi thân thiết với `{O}` từ 300 trở lên** (lúc bà mất). Dưới 300 thì vườn chỉ là kỷ vật.
     - Khi `{O}` còn sống mà thân thiết từ 300 trở lên, bà đã cho người chơi bế quan ở vườn từ E0 trở đi.
     - **{O}:** "Sit under the pear tree as long as you need. The garden will keep time for you." / “在梨树下坐多久都行。园子会替你记着时辰。”
   - **Trao cho `{Y}`:** kết thúc **Truyền nhân**.

### Kết thúc E
- **Truyền nhân:** `{Y}` thành đan sư hoặc kiếm tu, theo cách nuôi dạy. Khi chơi tiếp bằng hậu duệ, `{Y}` được ưu tiên.
  - *EN:* "You taught me to stand. Now let me carry the family."
  - *ZH:* “是你教会我站起来。现在，让我来扛起这个家。”
- **Đứa trẻ lạc lối:** `{Y}` lớn lên dưới tay Hồng Liên.
  - *EN:* "Nobody came for me. She did."
  - *ZH:* “没人来找我。只有她来了。”
- **Vườn thuốc truyền đời:** vườn được giữ qua nhiều thế hệ; mỗi thế hệ nhận "lời dạy" đầu tiên.

---

## Trận cuối — "Hồng Liên" (`{R}`)

**Khi nào:** tuyến A hoặc B đã tới hồi 4 trở lên, và có thêm ít nhất 2 tuyến khác đã kết thúc. **Hạn: 5 năm**; quá hạn thì nhánh "Ma vực lan rộng" (mỗi năm thêm 2 trấn sa ngã, trận cuối khó hơn).

### F1. Ma vực
1. **Dấu hiệu.**
   - *EN:* Three towns raise red banners in one season. Merchants whisper a name: the Lady.
   - *ZH:* 一季之内，三座城升起红旗。商人们低声念着一个名字：夫人。
2. **Lá thư.**
   - **{R}** (*letter* / *信*): "Little thread-puller, you have been busy. Come to the Lotus Pavilion. Bring your friends — I so love company." / “小小的牵线人，你可真忙。来红莲阁吧。带上你的朋友——我最喜欢热闹了。”
3. **Lựa chọn.** ▸ Gather allies / 召集盟友 ▸ Go alone now / 立刻独自前往

### F2. Tập hợp đồng minh (mỗi đồng minh một drama ngắn)
- Đến tìm từng người đã có kết thúc tốt (`{A}`, `{H}`, `{N}`, `{C}`, `{Y}`, `{X}`). Mỗi người có một câu trả lời riêng:
  - **{A}:** "Finally." / “终于。”
  - **{H}:** "I'll bring the arrows. And snacks." / “我带箭。还有点心。”
  - **{N}:** "Who do I punch?" / “揍谁？”
  - **{C}:** "For my family." / “为了我的家人。”
  - **{Y}:** "For my village." / “为了我的村子。”
  - **{X}:** "I made explosive pills! On purpose this time!" / “我做了爆炸丹！这次是故意的！”
- Mỗi đồng minh vào trận cuối, kẻ thù cũng vậy. Đi một mình: trận cuối khó hơn, nhưng thưởng thêm.

### F3. Hồng Liên Các
1. **Cổng các:** trận với hộ pháp và những người đã thành kẻ thù trong các tuyến.
2. **Gặp Hồng Liên.**
   - **{R}:** "So many little threads, all tied back to me. Kneel, and I will let your friends keep their heads." / “这么多细线，都牵回到我身上。跪下，我便留你朋友们的脑袋。”
   - ▸ "My friends answer for themselves." / “我的朋友，自己替自己回答。” ▸ Kneel (secret ending) / 下跪（隐藏结局）
3. **Trận cuối** với `{R}` và các đồng minh.

### F4. Hậu chiến
1. **Sen tàn.**
   - *EN:* The red lotus withers. The towns she held open their gates.
   - *ZH:* 红莲凋零。她占据的城池纷纷打开城门。
2. **Phần thưởng:**
   - "Hồng Liên kính" (một món đồ độc nhất);
   - các trấn bị chiếm được giải phóng;
   - sổ nợ của Hồng Liên bị xóa.
3. **Đồng minh chia tay:** mỗi người một câu, rồi trở về kết thúc riêng của tuyến mình.

- **Kết thúc ẩn "Quỳ":** toàn thế giới treo thưởng người chơi, nhưng người chơi điều khiển các trấn của Hồng Liên.

---

## Cần bạn chốt
1. Độ dài mới: 6 hồi cho A và B, 5 hồi cho C, D, E, và 4 hồi cho trận cuối; mỗi hồi 3 bước.
2. Các tình tiết mới:
   - **A:** vết thương cũ, nhân chứng Lão Lục, mài kiếm.
   - **B:** chuyến hàng cuối, cuốn sổ đỏ.
   - **C:** kỳ thi điều lửa, đại hội luyện đan.
   - **D:** săn yêu thú, mối thù của Mộ Dung Ca với Hồng Liên.
   - **E:** A Ly lớn dần, bà bà lâm bệnh và cây cỏ trường thọ.
3. **Liên kết mới:**
   - làng của A Ly và gia đình Mộ Dung Ca đều bị Hồng Liên hại;
   - cuốn sổ đỏ của tuyến B nối với trận cuối.
