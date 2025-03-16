版本 V x.y.z
x : 程序框架大版本的UI制作流程迭代
y : UI制作流程内部较大更新迭代
z : UI制作流程小变动更新迭代

1. 拼UI: 
    UI层级结构:
        层级|功能区分
        便于美术替换|扩展
        善用空物体节点
        划分UIPanel区域块
    命名:
        区分
        方便代码引用查找
    Raycast:
        不需要的就将Raycast项置为false, 不会阻挡屏幕UI的射线检测
    Layout布局:
        水平、垂直、格子
        自己摆放适配布局
        主要用于方便扩展、程序代码自动增删项(UITemplate)
    锚点与布局(Rect)、分辨率适配:
        屏幕不同分辨率下的UI适配
        横屏，不一定都是16:9
    Check:
        A. 查看各区域块在各种分辨率下的布局适配情况
        B. 查看区域块内的显示情况
        C. 检查层级结构、UI组件配置|参数配置等
2. 挂UIPanel脚本 : ML.Engine.UI.UIBasePanel => Input管理
    脚本:
        建议类名与对应的UIPanel的名称一致
        注意脚本类的命名空间
        继承UIBasePanel
        UIManager.Push(UIPanel_Instance), 实例化时注意SetParent(Canvas, false),后面这项为不保留世界坐标，默认是true
        挂载于对应个UIPanel预制体
        后续一般使用协程与AB包异步加载资源，记得在协程开始前this.enabled = true，不然协程不会执行，执行完成后this.enabled = false
3. UIPanel显示用JSON数据文件
    ① 放置于 Json/TextContent下，可以有子文件夹，如Json/TextContent/Inventory/UIInventoryPanel.Json
    ② 标记AssetBundle，建议标记json文件上一级文件夹，这样所有放入文件夹的都是这个AB包标记，不需要重复标记，会自动跟随文件夹的AB包标记，也可以自己覆盖标记
    ③ AB包标记不区分大小写，都会整成小写，JsonAsset资产名称区分大小写
    ④ 在UIPanel脚本中确定好Json文件对应的数据结构体，并在Json文件中填入对应内容(需根据UIPanel显示适配文本项确定，不完全只是文本，也可能会有Texture2D之类的)
    ⑤ 在UIPanel脚本内使用 ABJsonAssetProcessor 加载JsonAsset，建议只加载一次，也可以每次实例化都加载一次，OnExit|Destroy时释放
    ⑥ 在UIPanel初始化与Refresh时调用Json数据更新UI面板
4. UI脚本:
    ① 协程使用
        使用前this.enabled = true
        使用后this.enabled = false
    ② Update相关
        使用ITickComponent接口
        使用前调用TickManager.Register才能生效
        不使用或者销毁时记得Unregister
    ③ 按键输入
        使用新的输入系统
        事件响应绑定
            InputAction.(started|canceled|performed) += 绑定, -= 移除
            OnEnter|OnRecovery: 启用并绑定输入
            OnExit|OnPause: 禁用并解绑输入
        尽量只控制UIPanel内部使用的按键输入不要涉及其他
    ④ 引用查找
        查找Awake中UIPanel层级结构下需要控制的UI组件的引用
        可能是固定的UI组件，只更新显示内容
        也可能是UITemplate，查找到之后gameobject.SetActive(false)，在第一次Refresh时实例化生成，后续使用临时存储的对象
    ⑤ 读取数据
        A. UIPanel对应的逻辑数据，处理为UIPanel方便控制处理的形式，并能反馈给逻辑层，如UIInventory对应的Inventory的存储数据ItemList
        B. JsonAsset数据，用于Refresh时更新UI面板显示
        C. 其他不存在于A.B.的数据，如Item.Icon，存在于背包系统的ItemManager中，根据Item的ID可以获取
        D. 内存管理
            JsonAsset数据可以只载入一次之后不释放，占用不大
            其他在实例化的UIPanel中申请内存的对象，在OnExit前不用释放，用变量临时存储，可反复使用，在OnExit时统一释放
            如 创建的Sprite、实例化的UITemplate.gameobject、new() 等等
    ⑥ UIPanel的显示与刷新
        实现函数void Refresh(): 在相关数据有更新时、成为顶层UIPanel时(OnEnter OnRecovery)、需要刷新UIPanel显示时 调用
        涉及申请内存的对象，用temp相关存储，只申请一次，类似于对象池，在OnExit时统一释放
        不同的UIPanel交互与显示效果的处理不一样
        Sprite可以根据sprite.texture唯一引用，即有多个image使用同一张图片时，根据texture2d找到对应的sprite，共用同一个sprite
5. 使用
    UIManager.Push|Pop(UIPanel_Instance)
    如果是最底层UI，即第一个进栈的UIPanel，使用ChangeBotPanel替换或出栈
6. 资产放置与命名规范
    ① AssetBundle标记
        建议只标记文件夹
    ② Texture2D
        处理为 Sprite and UI
        枚举类型Texture2D: 建议与枚举项的名称一致，直接使用enum.ToString()查找，便于扩展
        Icon Texture2D: 随意
    ③ 预制体
        也要进行AB包标记，暂时不用管，让它自动管理就行，后续再改
    ④ JsonAsset
        表数据: Json/TableData/...
        文本数据: Json/TextContent/...
7. 字体
    暂时使用微软雅黑 -> MSYH SDF
    使用的Text为TextMeshProUGUI
    富文本插入图片使用的SpriteAsset -> TMPSprite

目前已经制作的UI
    A. PlayerBotUIPanel : Player最底层UI
    B. PlayerUIPanel : PlayerUI交互选择面板
    C. 建造系统多个UIPanel
    D. 科技树的UIPanel
    E. 背包UIPanel
    F. 交互组件的UITip
    G. 预制体: KeyTip...
