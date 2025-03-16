有命名更新，都需要来维护这个文档!!!

Unity 程序内使用的资产命名
不做用途区分的，可忽略这一项命名

Addressable Name
    (新建Group)所属系统
        /资产类型[_用途][/如果是文件夹，则自动加入具体资产]
        (如果是单个文件，如图集) /资产类型_用途_名字
    Label也类似
        系统_类型[_用途]_名字

Assets下所有资产命名(除去策划表格配置项对应的资产命名、特殊命名资产、导入的别人的资产可改可不改(自己制作的需要更改))
        -> 策划配置项是否需要一起统一还待商榷
    类型_系统[_用途]_名字
    没有所属系统，即通用的，就类型_Common_名字，其他同理
JSON相关暂时可以不动，后续会迭代掉

Assets下资产的放置
    [专属于ML的资产同理]
    OCResource/系统(不要缩写)/[类型|作用]/[可以有子文件夹做区分/]资产名字

类型
    Shader : Shader_
    Material : Mat_
    Texture2D : Tex2D_
    Texture3D : Tex3D_
    Prefab : Prefab_
    ConfigAsset : Config_
    TMP_Font Asset : Font_SDF
    Font Asset : Font_
    Mesh : Mesh_
    Model Asset(Mesh+Material组合成的那个) : Model_
    SpriteAtals : SA_
    AnimationClip : Anim_
    Animator Controller  : AnimCtrl_
    TMP Sprite Asset : TMPSprite_

系统
    Scene 正式用场景: Scene_
        路径: 
            Assets/_ML/Scene/*
            Assets/_ProjectOC/Scene/*
        Addressable: Scene/*
    TestScene 测试用场景: TestScene_
        路径: 
            Assets/_ML/TestScene*
            Assets/_ProjectOC/TestScene/*
        Addressable: TestScene/*
    JSON文件
        路径: 
            Assets/_ML/MLResources/Json
            Assets/_ProjectOC/OCResources/Json
        Addressable: Json/*
    BaseUIPanel ML基础UIPanel
        to-do
    BaseUIPrefab ML基础UIPrefab
        to-do
    SaveSystem 存档系统 : _SaveSystem_
        路径: Assets/_ML/MLResources/SaveSystem
        Addressable: SaveSystem/*
    Fonts ML 字体
        路径: Assets/_ML/MLResources/Fonts
        Addressable: Fonts/*
    InteractSystem 交互系统 : _InteractSystem_
        路径: Assets/_ML/MLResources/InteractSystem
        提示UI预制体: */UI/Prefab_InteractSystem_KeyTip
    BuildingSystem 建造系统 : _BS_
        路径: Assets/_ProjectOC/OCResources/BuildingSystem
        Addressable: BuildingSystem/*
        点面、点点配置项: 
            路径: */_MatchConfig
            Addressable: */Config
        用于辅助配置BPart项:
            路径: */_Prefabs
        建筑物预制体:
            路径: */BuildingPart
            Addressable: */Prefab_BuildingPart
        建筑物材质包:
            路径: */MatPackage
            Addressable: MatAsset_MatPackage
        建筑物材质包对应的图标Icon:
            路径: */Texture2D/MatPackageIcon
        建筑物材质包对应的材质:
            路径: */Materials
        建筑物材质包对应的材质的纹理贴图Texture3D:
            路径: */Texture3D
        建筑物模型:
            路径: */OriginModel
        建筑物图标:
            路径: */Texture2D/BuildIcon
            Addressable: */SA_UI_BuildIcon
        建筑物一级分类:
            路径: */Texture2D/Category/Category1
            Addressable: */SA_UI_Category1
        建筑物二级分类:
            路径: */Texture2D/Category/Category2
            Addressable: */SA_UI_Category2
        建筑物三级分类:
            路径: */Texture2D/Category/Category3
            Addressable: */SA_UI_Category3
        建造系统UIPanel:
            路径: */UIPrefabs/Panel
            Addressable: */Prefab_BuildingPart
        家具主题图片:
            路径: */Texture2D/FurnitureTheme
            Addressable: */SA_UI_FurnitureTheme
    Materials 通用材质
        路径: Assets/_ProjectOC/OCResources/Materials
    Terrains 地形相关资产 : _Terrain_
    AICharacter 角色相关(不包括Player) : _AIC_
        路径: Assets/_ProjectOC/OCResources/AICharacter
        Worker相关:
            路径: */Worker
            Addressable: Worker/*
            预制体: 
                路径: */Prefabs
                Addressable: */Prefab_Character
    Player Player相关 : _Player_
        路径: Assets/_ProjectOC/OCResources/Player
        Addressable: Player/*
        动画相关: 
            路径: */Animation
        模型相关:
            路径: */Model
        预制体:
            路径: */Prefabs
            Addressable: */Prefab_Character
    PinchFace 捏脸相关 : _PinchFace_
        路径: Assets/_ProjectOC/OCResources/PinchFace
        Addressable: PinchFace/*
        捏脸设置Config:
            路径: */Config/PinchFaceConfig
            Addressable: */Config/PinchFaceSetting
        捏脸类型Config:
            路径: */Config/TypePackage
            Addressable: */Config/TypePackage
        捏脸相关原模型:
            路径: */Model
        捏脸相关预制体:
            路径: */Prefabs
            Addressable: */Prefabs_PinchPart
        捏脸UIPanel:
            路径: */Prefabs/UIPanel
    Inventory 背包 : _Inventory_
        路径: Assets/_ProjectOC/OCResources/Inventory
        Addressable: Inventory/*
        背包显示图标配置相关:
            路径: */UIPanel/Texture2D/ItemType
    Item相关(背包系统) : _Item_
        路径: Assets/_ProjectOC/OCResources/Item
        Addressable: Item/*
        世界物体相关:
            路径: */WorldItem
            Addressable: */Prefab_Worlditem
        ItemIcon相关:
            路径: */ItemIcon
            Addressable: */SA_Item_UI_ItemIcon
    OrderSystem 订单管理: _Order_
        路径: Assets/_ProjectOC/OCResources/OrderSystem
        Addressable: OrderSystem/*
    ResonanceWheel 隐兽共鸣: _Resonance_
        路径: Assets/_ProjectOC/OCResources/OrderSystem
        Addressable: ResonanceWheel/*
    Restaurant 餐厅 : _Restaurant_
        路径: Assets/_ProjectOC/OCResources/Restaurant
        Addressable: Restaurant/*
    Store 仓库 : _Store_
        路径: Assets/_ProjectOC/OCResources/Store
        Addressable: Store/*
    TechTree 科技树 : _TechTree_
        路径: Assets/_ProjectOC/OCResources/TechTree
        Addressable: TechTree/*
    MainInteract 主交互 : _MainInteract_
        路径: Assets/_ProjectOC/OCResources/MainInteract
        Addressable: MainInteract/*
    ProNode 生产节点 : _ProNode_
        路径: Assets/_ProjectOC/OCResources/ProNode
        Addressable: ProNode/*