# ��������

### �����ļ�

 appsettings.json

```json
{
  "Database": [
    {
      "Name": "SQLite",
      "Enabled": true,
      "ConnectionString": "Data Source=wangsql.db;",
      "ConnectionType": "System.Data.SQLite.SQLiteConnection,System.Data.SQLite",
      "UseParameterPrefixInSql": true,
      "UseParameterPrefixInParameter": true,
      "ParameterPrefix": "@",
      "UseQuotationInSql": false,
      "Debug": false
    }
  ]
}
```

���� 

app.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<database>
  <dbProvider>
    <name>SQLite</name>
    <enabled>true</enabled>
    <connectionString>Data Source=wangsql.db;</connectionString>
    <connectionType>System.Data.SQLite.SQLiteConnection,System.Data.SQLite</connectionType>
    <useParameterPrefixInSql>true</useParameterPrefixInSql>
    <useParameterPrefixInParameter>true</useParameterPrefixInParameter>
    <parameterPrefix>@</parameterPrefix>
    <useQuotationInSql>false</useQuotationInSql>
    <debug>false</debug>
  </dbProvider>
</database>
```

### ��ʼ������

~~~c#
/// <summary>
/// ͨ��Ĭ�������ļ�(appsettings.json����app.config����web.config)��ʼ����������
/// </summary>
DbProviderManager.Set();
~~~

����

~~~c#
/// <summary>
/// ͨ��ָ���ļ���ʼ����������
/// </summary>
DbProviderManager.Set("/app_database.config");
~~~

����

~~~c#
/// <summary>
/// ͨ�������ʼ����������
/// </summary>
DbProviderManager.Set(
                    "SQLite",
                    "Default",
                    "Data Source=mario.db;",
                    "System.Data.SQLite.SQLiteConnection,System.Data.SQLite",
                    true,
                    true,
                    "@",
                    false,
                    false
                    );
~~~

### ����SqlMapper����

~~~c#
/// <summary>
/// ����Ĭ�϶���
/// </summary>
var _sqlMapper = new SqlMapper();
~~~

����

~~~ c#
/// <summary>
/// �����������ƴ���ָ������
/// </summary>
var _sqlMapper = new SqlMapper("SQLite");
~~~

### CRUDʹ��

##### ��ѯ�б�����

~~~c#
//�޲�����ѯ�б�
var sql = "select * from tb_user";
var r = _sqlMapper.Query<Dictionary<string, object>>(sql, null).ToList();
~~~

~~~c#
//�в�����ѯ�б�

//���η�ʽ1 ֱ�Ӵ�������͵�ֵ
var sql = "select * from tb_user where userid=#userid#";
var r1 = _sqlMapper.Query<Dictionary<string, object>>(sql, userid).ToList();
Dictionary<string, object>  sod = new Dictionary<string, object>()
{
    { "userid", userid }
};

//���η�ʽ2 �����ֵ��
var r2 = _sqlMapper.Query<Dictionary<string, object>>(sql, sod).ToList();

//���η�ʽ3 ����ʵ��
UserInfo user = new UserInfo()
{
    UserId = userid
};
var r3 = _sqlMapper.Query<Dictionary<string, object>>(sql, user).ToList();
~~~

##### ��ѯ��������

~~~c#
//�޲�����ѯ����
var sql = "select * from tb_user";
var r = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, null);
~~~

~~~c#
//�в�����ѯ����

//���η�ʽ1 ֱ�Ӵ�������͵�ֵ
var sql = "select * from tb_user where userid=#userid#";
var r1 = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, userid);
Dictionary<string, object>  sod = new Dictionary<string, object>()
{
    { "userid", userid }
};

//���η�ʽ2 �����ֵ��
var r2 = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, sod);

//���η�ʽ3 ����ʵ��
UserInfo user = new UserInfo()
{
    UserId = userid
};
var r3 = _sqlMapper.QueryFirstOrDefault<Dictionary<string, object>>(sql, user);
~~~

##### ��������

����ͬ��֧�ֲ�ѯʱ������������ͣ�����ֻ��ʾ����һ�ִ��η�ʽ��

~~~c#
var sql = "insert into tb_user(userid,username,age) values(#userid#,#username#,#age#)";
var sod = new Dictionary<string, object>()
{
    { "userid", userid },
    { "username", username },
    { "age", age }
};
var r = _sqlMapper.Execute(sql, sod);
~~~

##### ��������

����ͬ��֧�ֲ�ѯʱ������������ͣ�����ֻ��ʾ����һ�ִ��η�ʽ��

~~~c#
var sql = "update tb_user set username=#username#,age=#age# where userid=#userid#";
var sod = new Dictionary<string, object>()
{
    { "userid", userid },
    { "username", username },
    { "age", age }
};
var r = _sqlMapper.Execute(sql, sod);
~~~

##### ɾ������

����ͬ��֧�ֲ�ѯʱ������������ͣ�����ֻ��ʾ����һ�ִ��η�ʽ��

~~~ c#
var sql = "delete from tb_user where userid=#userid#";
var sod = new Dictionary<string, object>()
{
    { "userid", userid }
};
var r = _sqlMapper.Execute(sql, sod);
~~~

