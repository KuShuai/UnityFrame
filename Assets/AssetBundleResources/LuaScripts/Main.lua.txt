UIPanelEnum = require "XLua.UI.UIPanelEnum"
require "XLua.UI.UIConfig"

function Debug(fmt,...)
	if type(fmt)=="string" then
		local msg = 'LUA-'..string.format(fmt,...)
		CS.LuaInterface.DebugLog(msg)
	else
		print(fmt,...)
	end
end

CS.LuaInterface_UI.OpenPanel(UIPanelEnum.Panel)
Debug("AAAAA1111")


--常用
--Ctrl + Z 撤销上一次修改
--Ctrl + Shift +Z 恢复上一次修改
--Ctrl + Alt + L 编排代码
--Ctrl + R 替换文本
--Ctrl + Shift + R 替换项目所有内容
--Ctrl + “+”展开文本
--Ctrl + Shift + “+”展开所有文本
--Ctrl + “-”折叠文本
--Ctrl + Shift +“-”折叠所有文本
--Ctrl + Alt + Insert 当前目录创建文件
--Shift + Alt + 鼠标左键 多光标，再次左键取消光标
--ALT + 6 Rider提示
--ALT + 9 文件变化（需要结合版本管理工具）
--
--查找
--Ctrl + B 跳转声明和实现
--Shift + Shift 查找所有（文件，类，符号，行为）
--Ctrl + Shift + N 查找文件
--Ctrl + Shift + F 搜索项目所有内容
--Ctrl + F12 当前文件所有函数和变量
--
--跳转
--Ctrl + Tab 切换打开的文件
--Ctrl + E 显示最近打开的文件
--Ctrl + F11 添加标签
--Ctrl + 标签 跳转标签位置（配套 Ctrl + F11）
--Shift + Alt + ↑ 或 ↓ 移动选中块/当前行
--Alt + ↑ 或 ↓ 光标移动到上下函数位置
--Ctrl + Home 光标移动到文件开始处
--Ctrl + End 光标移动到文件结束处
--
--调试
--Ctrl + F5 重新运行项目
--Ctrl + F2 终止项目
--F7 单步执行 进入子函数
--F8 单步执行 不进入子函数
--F9 继续运行
--Ctrl + F8 打断点