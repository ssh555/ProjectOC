1. 基本都只有逻辑层面，不怎么涉及在unity里面的可视化交互

2. ProducingDepartment => 暂时先把它做成一个可用的预制体，使用拖拽的形式进行测试，可以考虑就是一个方块

3. GameManager 最上层
        => LocalGameManager

4. Store => 交互项待具体完整整理设计，参考2.ProducingDepartment

5. Worker => 纯只有逻辑，只涉及了一个Mono和AI寻路
    Mono => 用于可放置于场景，暂时使用Capsule代替
    AI寻路 => 暂时使用unity自带，留好设置速度和移动接口即可

6. 涉及到有坑点、可优化点、待完善点之类的，用"to-do : xxx"标记

7. 涉及到框架更改，确定一个属于自己的唯一标识符注释，方便查找，审核更改了哪些

8. to-do : 场景内的可视化、UI交互、其他交互等等, Player的设计与InteractComponent

[
    个人建议: 把伪代码梳理完善，补充细节 => 可以考虑写个word截图进去
    涉及到框架、unityscene内的可视化、一些使用不清楚的注意事项等等，随时问ssh
]


