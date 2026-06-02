-- =====================================================
-- 清理表情数据脚本
-- 执行时间: 2026-06-01
-- 影响表: Discussions, DiscussionComments
-- =====================================================

-- 表情代码列表
DECLARE @StickerPatterns TABLE (Code NVARCHAR(50));
INSERT INTO @StickerPatterns VALUES
('neutral'), ('grin_big'), ('grin'), ('grin_squint'), ('beam'),
('grin_open'), ('smile'), ('upside_down'), ('frown_slight'), ('frown'),
('pout_angry'), ('neutral_line'), ('expressionless'), ('open_mouth'),
('hushed'), ('astonished'), ('flushed'), ('frown_open'), ('anguished'),
('angry'), ('angry_red'), ('relieved'), ('unamused'), ('smirk'),
('wink'), ('kiss'), ('heart_eyes'), ('kissing'), ('kiss_closed'),
('kiss_smile'), ('pensive'), ('persevere'), ('blush'), ('smile_eyes'),
('hug'), ('dizzy'), ('surprised'), ('weary'), ('squint_tongue'),
('tongue'), ('wink_tongue'), ('yum'), ('roll_eyes'), ('wide_eyes'),
('grimace'), ('innocent'), ('teeth'), ('sweat_smile'), ('sad_sweat'),
('cry'), ('disappointed'), ('cold_sweat'), ('worried'), ('fearful'),
('joy_tears'), ('sleepy'), ('cool'), ('mask'), ('sleeping'),
('sob'), ('bandage'), ('money'), ('think'), ('thermometer'),
('zipper'), ('nerd'), ('triumph'), ('poop');

-- 清理 Discussions 表
DECLARE @Code NVARCHAR(50);
DECLARE cursor_codes CURSOR FOR SELECT Code FROM @StickerPatterns;
OPEN cursor_codes;
FETCH NEXT FROM cursor_codes INTO @Code;
WHILE @@FETCH_STATUS = 0
BEGIN
    UPDATE Discussions SET Content = REPLACE(Content, ':' + @Code + ':', '');
    UPDATE DiscussionComments SET Content = REPLACE(Content, ':' + @Code + ':', '');
    FETCH NEXT FROM cursor_codes INTO @Code;
END
CLOSE cursor_codes;
DEALLOCATE cursor_codes;

-- 清理空白内容
UPDATE Discussions SET Content = LTRIM(RTRIM(Content)) WHERE LEN(LTRIM(RTRIM(Content))) = 0 OR Content IS NULL;
UPDATE DiscussionComments SET Content = LTRIM(RTRIM(Content)) WHERE LEN(LTRIM(RTRIM(Content))) = 0 OR Content IS NULL;

-- 验证结果
SELECT '清理完成 - Discussions表剩余记录数:' AS Info, COUNT(*) AS Count FROM Discussions
UNION ALL
SELECT '清理完成 - DiscussionComments表剩余记录数:', COUNT(*) FROM DiscussionComments;
