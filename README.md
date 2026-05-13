# QPICPSOControlAutomation1

基于Automation1 .NET API 实现PSO控制

## 思路说明

|顺序|功能|方法名|
|----|----|----|
|1.|初始化|FormMain_Load|
|2.|点击按钮变色，变字，PsoOutput|Button1_Click|

## 更新说明

### V1.0.1
功能完善
1. 在初始化成功后再显示主窗口，启动时先隐藏，以减少闪烁并 (FormMain.cs/Line 21、51)
2. 初始化按钮文本与颜色，使用 `DataCollection` 快照读取 `PsoStatus`，并通过位掩码判断 PSO 输出状态 (FormMain.cs/Line 32--50)
3. 在初始化时，若控制箱未打开，增加异常捕获以避免关闭时崩溃 (FormMain.cs/Line 56)

### V1.0.0
实现核心功能
1. 定义`controller`变量，并链接设备进行初始化(FormMain.cs/Line 10)
2. 按钮逻辑(FormMain.cs/Line 50--66)
3. 控制箱报错((FormMain.cs/Line 14、25、67--77))