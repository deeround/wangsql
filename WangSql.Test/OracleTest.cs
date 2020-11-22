using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Test
{
    public class OracleTest
    {
        public static void Test()
        {
            DbProviderManager.Set(
                    "Oracle",
                    "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.4.182)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = elaneweb)));User ID=elaneweb;Password=elaneweb;",
                    "Oracle.ManagedDataAccess.Client.OracleConnection,Oracle.ManagedDataAccess",
                    true,
                    true,
                    ":",
                    false,
                    false
                    );
            var _sqlMapper = new SqlMapper();

            //删除表
            try
            {
                string sql = $"DROP TABLE {_sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(TableMap.GetMap<Models.UserInfo>().Name)}";
                _sqlMapper.Execute(sql, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("没有表，不用删除：" + ex.Message);
            }

            //创建表
            _sqlMapper.Migrate().Run();

            //删除数据
            _sqlMapper.Entity<Models.UserInfo>().Delete();

            //插入数据
            string text = @"以前觉得，谈恋爱，一定要找一个百分百对自己好的人。

    体贴、温柔的人真的很讨喜呀，他愿意包容我的小脾气，接纳我的小情绪，分享我的不满和委屈，在我被悲伤和失落包围的时候，给我恰到好处的温柔和爱意。

　　这样的他会让我觉得，他是真的很爱很爱我。

　　可现实是，对很多女孩子来说，想要找到一个百分百的人，太难了，换句话讲，无限包容和忍让，或许只是爱情的其中一种样子。

　　我想，还有一种爱情，可能听起来不够甜蜜，但事实上，却应该是很多女孩子期待的恋爱真实状态：遇到一个愿意好好陪你吵架的男孩。

　　好友Andy就是一个性格外向的女孩，她属于那种有什么话一定要说出来，绝对不会憋在心里半句。

　　谈恋爱之后，Andy风风火火的脾气没有任何改变，可恰好她的男朋友也是一个直来直去的人，时间久了，我们都很好奇他们两个人该如何相处下去。

　　有次我和Andy一起加班到很晚，Andy给男友发微信，希望他可以来接我们，没想到过了半小时也没等到回复。

　　Andy有点想发火，但还是忍着拨通了男友的电话。

　　“我刚刚没听到，怎么啦？”

　　大概是在气头上，Andy不由分说地把男生骂了一顿，也许是为了发泄吧，语气十分很强烈。

　　让我吃惊地是，男生没有道歉或者沉默，而是用同样分贝的声音和Andy“吵”了起来：“喂，我也很累呀，给点面子好不好？再说了，你这么大个人不会打车吗，真是的……”

　　然后，他们两个人你一言我一语，在电话里展开了辩论，足足持续了二十分钟。

　　但挂掉电话，Andy并没有表现出生气的样子。

　　结果一会儿，男友把车开到了公司楼下，他朝Andy作出一副恭恭敬敬的手势：“欢迎老婆大人上车，我们打道回府。”

　　我和Andy都被逗笑了。

　　后来Andy告诉我，吵架其实他们两个人的常态，因为都是脾气火爆的人，所以很多事情根本憋不住，不如干脆把不满说出来，也好过彼此隐瞒。

　　“我觉得，吵架也是检验爱情的标准之一，如果他连吵架都懒得跟你吵，两个人的隔阂只会越来越深，时间久了，就再也看不清对方了。”

　　深以为然，一直觉得，两个人在一起，就是一个不断暴露自我的过程，双方都会有大大小小的缺点，在吵架的过程中，如果可以把心中的不痛快发泄出来，才能真正看出对方的态度。

　　如果确实不能继续相处，三观不合，好聚好散也好过藕断丝连的拖着。

　　最怕的不是吵到天翻地覆，而是一味地退让隐忍，或者干脆使用冷暴力。

　　知乎上有个答主讲过自己的故事：“和男朋友分手，只是因为他没有好好听我发的语音。";
            for (int i = 0; i < 10000; i++)
            {
                Models.UserInfo userInfo = new Models.UserInfo(Guid.NewGuid().ToString(), "deeround" + Guid.NewGuid().ToString(), 99, true, text);
                userInfo.CreateDateTime = DateTime.Now;
                _sqlMapper.Entity<Models.UserInfo>().Insert(userInfo);
            }

            //查询
            var users = _sqlMapper.Entity<Models.UserInfo>().ToList();
        }
    }
}
