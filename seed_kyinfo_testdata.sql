SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- 1) Schools（如果表非空，这里可能因唯一/重复数据导致失败；建议仅在空库运行）
INSERT INTO dbo.Schools (Name, ShortName, Province, City, LevelTag, [Type], [Property], Website, CreatedAt)
VALUES
('北京大学',       '北大',   '北京', '北京', '985',   '综合', '公办', 'https://www.pku.edu.cn',       GETUTCDATE()),
('清华大学',       '清华',   '北京', '北京', '985',   '综合', '公办', 'https://www.tsinghua.edu.cn',  GETUTCDATE()),
('浙江大学',       '浙大',   '浙江', '杭州', '985',   '综合', '公办', 'https://www.zju.edu.cn',       GETUTCDATE()),
('华中科技大学',   '华科',   '湖北', '武汉', '985',   '理工', '公办', 'https://www.hust.edu.cn',      GETUTCDATE()),
('南京师范大学',   '南师大', '江苏', '南京', '211',   '师范', '公办', 'https://www.njnu.edu.cn',      GETUTCDATE()),
('深圳大学',       '深大',   '广东', '深圳', '普通',  '综合', '公办', 'https://www.szu.edu.cn',       GETUTCDATE());

-- 2) Majors
INSERT INTO dbo.Majors
    (Name, Code, DisciplineCategory, DegreeType, StudyType, DurationYears, TuitionPerYear, SchoolDepartment, [Description], SchoolId)
VALUES
-- 北大（SchoolId=1）
('计算机科学与技术', '081200', '工学',   '学硕', '全日制', 3, 8000,  '信息科学技术学院',     '研究人工智能、系统结构、软件工程等方向', 1),
('软件工程',         '083500', '工学',   '学硕', '全日制', 3, 8000,  '软件与微电子学院',     '聚焦大型软件系统设计与开发',             1),
-- 清华（SchoolId=2）
('计算机科学与技术', '081200', '工学',   '学硕', '全日制', 3, 8000,  '计算机科学与技术系',   '国内顶尖计算机学科',                     2),
('电子信息',         '085400', '工学',   '专硕', '全日制', 2, 10000, '计算机科学与技术系',   '工程实践导向，含AI与大数据方向',         2),
-- 浙大（SchoolId=3）
('计算机科学与技术', '081200', '工学',   '学硕', '全日制', 3, 8000,  '计算机科学与技术学院', '涵盖AI、图形学、网络等',                 3),
('人工智能',         '085410', '工学',   '专硕', '全日制', 2, 10000, '人工智能研究所',       '聚焦深度学习、NLP、CV',                  3),
-- 华科（SchoolId=4）
('计算机科学与技术', '081200', '工学',   '学硕', '全日制', 3, 8000,  '计算机科学与技术学院', '华中地区最强CS',                         4),
-- 南师大（SchoolId=5）
('教育学',           '040100', '教育学', '学硕', '全日制', 3, 8000,  '教育科学学院',         '研究课程与教学论、教育技术',             5),
('学科教学（语文）', '045103', '教育学', '专硕', '全日制', 2, 10000, '教师教育学院',         '培养中学语文教师',                       5),
-- 深大（SchoolId=6）
('计算机技术',       '085404', '工学',   '专硕', '全日制', 3, 12000, '计算机与软件学院',     '工程应用导向，就业率高',                 6);

-- 3) ScoreLines
INSERT INTO dbo.ScoreLines (Year, Score, IsNational, SchoolId, MajorId, Note, CreatedAt)
VALUES
-- 国家线（IsNational=1，SchoolId/MajorId应为空）
(2023, 273, 1, NULL, NULL, '2023年工学国家线A区', GETUTCDATE()),
(2024, 275, 1, NULL, NULL, '2024年工学国家线A区', GETUTCDATE()),
(2025, 270, 1, NULL, NULL, '2025年工学国家线A区', GETUTCDATE()),
(2023, 350, 1, NULL, NULL, '2023年教育学国家线A区', GETUTCDATE()),
(2024, 355, 1, NULL, NULL, '2024年教育学国家线A区', GETUTCDATE()),
(2025, 352, 1, NULL, NULL, '2025年教育学国家线A区', GETUTCDATE()),

-- 北大-计算机学硕（1,1）
(2023, 360, 0, 1, 1, '北大计科学硕复试线', GETUTCDATE()),
(2024, 370, 0, 1, 1, '北大计科学硕复试线上涨', GETUTCDATE()),
(2025, 358, 0, 1, 1, '北大计科学硕复试线回落，大小年明显', GETUTCDATE()),

-- 清华-电子信息专硕（2,4）
(2023, 380, 0, 2, 4, '清华电子信息专硕复试线', GETUTCDATE()),
(2024, 388, 0, 2, 4, '清华电子信息专硕复试线，竞争激烈', GETUTCDATE()),
(2025, 382, 0, 2, 4, '清华电子信息专硕复试线', GETUTCDATE()),

-- 浙大-计算机学硕（3,5）
(2023, 350, 0, 3, 5, '浙大计科学硕复试线', GETUTCDATE()),
(2024, 358, 0, 3, 5, '浙大计科学硕复试线', GETUTCDATE()),
(2025, 355, 0, 3, 5, '浙大计科学硕复试线', GETUTCDATE()),

-- 华科-计算机学硕（4,7）
(2023, 335, 0, 4, 7, '华科计科学硕复试线', GETUTCDATE()),
(2024, 342, 0, 4, 7, '华科计科学硕复试线', GETUTCDATE()),
(2025, 338, 0, 4, 7, '华科计科学硕复试线', GETUTCDATE()),

-- 南师大-教育学学硕（5,8）
(2023, 358, 0, 5, 8, '南师大教育学学硕复试线', GETUTCDATE()),
(2024, 368, 0, 5, 8, '南师大教育学学硕复试线上涨', GETUTCDATE()),
(2025, 362, 0, 5, 8, '南师大教育学学硕复试线', GETUTCDATE()),

-- 深大-计算机技术专硕（6,10）
(2023, 305, 0, 6, 10, '深大专硕复试线，性价比高', GETUTCDATE()),
(2024, 315, 0, 6, 10, '深大专硕复试线小幅上涨', GETUTCDATE()),
(2025, 312, 0, 6, 10, '深大专硕复试线', GETUTCDATE());

-- 4) RecruitInfos
INSERT INTO dbo.RecruitInfos
    (Year, SchoolId, MajorId, PlanCount, ExamSubjects, ExtraRequirements, SourceUrl, PublishedAt, CreatedAt)
VALUES
-- 北大-计科学硕（1,1）
(2025, 1, 1, 35,  '政治、英语一、数学一、408计算机学科专业基础',       '要求本科为计算机相关专业',        'https://admission.pku.edu.cn/zsxx/sszs/',   '2024-09-15', GETUTCDATE()),
(2024, 1, 1, 32,  '政治、英语一、数学一、408计算机学科专业基础',       '要求本科为计算机相关专业',        'https://admission.pku.edu.cn/zsxx/sszs/',   '2023-09-15', GETUTCDATE()),

-- 北大-软件工程学硕（1,2）
(2025, 1, 2, 20,  '政治、英语一、数学一、408计算机学科专业基础',       '软微学院单独招生',                'https://admission.pku.edu.cn/zsxx/sszs/',   '2024-09-15', GETUTCDATE()),

-- 清华-电子信息专硕（2,4）
(2025, 2, 4, 50,  '政治、英语一、数学一、912计算机专业基础综合',       '复试含上机编程测试',              'https://yz.tsinghua.edu.cn/',               '2024-09-20', GETUTCDATE()),
(2024, 2, 4, 48,  '政治、英语一、数学一、912计算机专业基础综合',       '复试含上机编程测试',              'https://yz.tsinghua.edu.cn/',               '2023-09-20', GETUTCDATE()),

-- 浙大-计算机学硕（3,5）
(2025, 3, 5, 40,  '政治、英语一、数学一、408计算机学科专业基础',       '接受跨专业考生',                  'https://grs.zju.edu.cn/yjszs/',             '2024-09-18', GETUTCDATE()),

-- 浙大-AI专硕（3,6）
(2025, 3, 6, 25,  '政治、英语一、数学一、408计算机学科专业基础',       '需提交AI相关项目经历',            'https://grs.zju.edu.cn/yjszs/',             '2024-09-18', GETUTCDATE()),

-- 华科-计算机学硕（4,7）
(2025, 4, 7, 60,  '政治、英语一、数学一、408计算机学科专业基础',       '华科招生规模较大',                'https://gs.hust.edu.cn/',                   '2024-09-22', GETUTCDATE()),
(2024, 4, 7, 58,  '政治、英语一、数学一、408计算机学科专业基础',       NULL,                              'https://gs.hust.edu.cn/',                   '2023-09-22', GETUTCDATE()),

-- 南师大-教育学学硕（5,8）
(2025, 5, 8, 15,  '政治、英语一、311教育学专业基础',                   '需提交研究计划',                  'https://yz.njnu.edu.cn/',                   '2024-09-25', GETUTCDATE()),

-- 南师大-学科教学专硕（5,9）
(2025, 5, 9, 30,  '政治、英语二、333教育综合、824语文课程与教学论',   '需有教师资格证优先',              'https://yz.njnu.edu.cn/',                   '2024-09-25', GETUTCDATE()),

-- 深大-计算机技术专硕（6,10）
(2025, 6, 10, 80, '政治、英语二、数学二、408计算机学科专业基础',      '招生名额多，竞争相对较小',        'https://yz.szu.edu.cn/',                    '2024-09-28', GETUTCDATE()),
(2024, 6, 10, 75, '政治、英语二、数学二、408计算机学科专业基础',      NULL,                              'https://yz.szu.edu.cn/',                    '2023-09-28', GETUTCDATE());

-- 5) Users（你当前库里已有一个用户，所以这里指定 Id=1..3）
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (Id, UserName, Email, PasswordHash, Role, CreatedAt)
VALUES
(1, '张三', 'zhangsan@test.com', '$2a$11$placeholder_hash_for_zhangsan', 0, GETUTCDATE()),
(2, '李四', 'lisi@test.com',     '$2a$11$placeholder_hash_for_lisi',     0, GETUTCDATE()),
(3, '王五', 'wangwu@test.com',   '$2a$11$placeholder_hash_for_wangwu',   1, GETUTCDATE());
SET IDENTITY_INSERT dbo.Users OFF;

-- 6) ExamScores
INSERT INTO dbo.ExamScores
    (Year, TotalScore, PoliticsScore, EnglishScore, MathScore, MajorSubjectScore, UserId, SchoolId, MajorId, CreatedAt)
VALUES
-- 张三（UserId=1）：2025 北大计科学硕
(2025, 385, 75, 78, 132, 100, 1, 1, 1, GETUTCDATE()),
-- 张三（UserId=1）：2024 浙大计科学硕
(2024, 350, 72, 70, 120, 88,  1, 3, 5, GETUTCDATE()),

-- 李四（UserId=2）：2025 华科计科学硕
(2025, 362, 70, 65, 135, 92,  2, 4, 7, GETUTCDATE()),
-- 李四（UserId=2）：2025 深大专硕（保底）
(2025, 340, 68, 62, 125, 85,  2, 6, 10, GETUTCDATE()),

-- 王五（UserId=3）：2025 南师大教育学学硕（无数学）
(2025, 375, 80, 72, NULL, 223, 3, 5, 8, GETUTCDATE()),
-- 王五（UserId=3）：2024 南师大学科教学（无数学）
(2024, 380, 78, 75, NULL, 227, 3, 5, 9, GETUTCDATE());

COMMIT TRANSACTION;

-- 验证数量
SELECT 'Schools' AS TableName, COUNT(*) AS RowCount FROM dbo.Schools
UNION ALL SELECT 'Majors', COUNT(*) FROM dbo.Majors
UNION ALL SELECT 'ScoreLines', COUNT(*) FROM dbo.ScoreLines
UNION ALL SELECT 'RecruitInfos', COUNT(*) FROM dbo.RecruitInfos
UNION ALL SELECT 'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT 'ExamScores', COUNT(*) FROM dbo.ExamScores;
