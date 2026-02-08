// Serializer
Obj2Toml:
	TommySerializer.ToTomlFile(obj, path); // 原生
	TommySerializer.ToTomlText(obj);	   // 拓展
Toml2Obj:
	Obj obj = TommySerializer.FromTomlFile<Obj>(path); // 原生
	Obj obj = TommySerializer.FromTomlText<Obj>(reader.ReadToEnd()); // 拓展

特性:
	[TommyTableName(string: "mytablename")]
		修饰于类, 将其指定为一个Toml表格
	[TommyComment(string: "This is the player's name")]
		修饰于变量, 会把注释添加到生成的文本之上
	[TommySortOrder(int: i)]
		修饰于变量, 会让变量在生成的文本中作为第i个成员被写入
	[TommyInclude]
		修饰于私有变量,准许其参与序列化/反序列化
	[TommyIgnore]
		修饰于变量,禁止其参与序列化/反序列化

// 

————————————————————————————————————————————————————
官方: https://toml.io/cn/v1.0.0
Wiki: https://github.com/toml-lang/toml/wiki
官方仓库: https://github.com/xoofx/Tomlyn

通用安装: 
	工具(T) ->
	NuGet包管理器(N) -> 
	程序包管理器控制台(O) ->
	Install-Package Tomlyn

// MIT协议,记得标明引用
Tomlyn: 直接从仓库下载
	Tomlyn-xx -> src -> Tomlyn拖入
	* 注意: Unity2022是C#9.0(.Net5), 用不了
Tommy: 有修改,得用目录提供的这个
	https://github.com/dezhidki/Tommy
	https://github.com/instance-id/Tommy.Serializer