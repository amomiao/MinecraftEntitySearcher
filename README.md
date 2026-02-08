# MinecraftEntitySearcher 使用说明
这个软件制作的初衷是让玩家能够通过读取存档数据，找到他的“动物朋友（酒狐）”。因为这个功能已经实现，所以尽管功能比较简陋，也发布出来给大家使用。

## 使用步骤
1. 启动 `MoNbtSearcher.exe`。
2. 点击 **"选择"** 按钮，找到存档目录下的 `level.dat` 文件。  
   > 全程对存档是只读的，不会对存档产生任何破坏风险。
3. 在 **"维度:"** 选择需要的维度，点击 **"加载.MCA"** 按钮。
4. 设置过滤条件。  
   - **Owner** 可以一键搜索。  
   - **Nbt** 搜索需要一定的知识，详细信息请参考 "推荐辅助工具" 部分。
5. 点击 **搜索**，等待查询结果。

# 推荐辅助工具 (Recommended Auxiliary Tools)

- **[NbtStudio](https://github.com/tryashtar/nbt-studio)**  
   这是一个非常好用的 NBT 文件编辑和查看工具，适合对 NBT 文件有一定了解的用户。通过它，你可以更轻松地浏览和修改存档中的 NBT 数据，帮助你更准确地设置搜索条件。


# Third-Party Libraries and Licenses 第三方库及其协议
This project includes the following third-party libraries:
1. **[fNbt](https://github.com/mstefarov/fNbt)**  
   Copyright (c) 2025 Matvei Stefarov
   fNbt is licensed under the **3-Clause BSD License**. See the full license in [docs/LICENSE.txt](docs/LICENSE.txt).
2. **[LitJSON](https://github.com/LitJSON/litjson)**  
   Copyright (c) 2020  
   LitJSON is released into the **Public Domain** via the [Unlicense](http://unlicense.org/).  
   "The software is provided 'as is', without warranty of any kind."
3. **[Tommy](https://github.com/dezhidki/Tommy)**  
   Copyright (c) 2020 Denis Zhidkikh  
   This software is licensed under the **MIT License**. See the full license above.
4. **[Tommy.Serializer](https://github.com/instance-id/Tommy.Serializer)**  
   Copyright (c) 2020 Dan  
   This software is licensed under the **MIT License**. See the full license above.