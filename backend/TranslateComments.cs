using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class TranslateComments
{
    private static readonly Dictionary<string, string> TranslationMap = new Dictionary<string, string>
    {
        // XML Documentation Comments
        ["用户表"] = "User table",
        ["用户id"] = "User ID",
        ["用户ID"] = "User ID",
        ["用户名"] = "Username",
        ["密码"] = "Password",
        ["创建时间"] = "Create time",
        ["更新时间"] = "Update time",
        ["删除时间"] = "Delete time",
        ["备注"] = "Remark",
        ["状态"] = "Status",
        ["排序"] = "Sort order",
        ["是否删除"] = "Is deleted",
        ["创建人"] = "Creator",
        ["更新人"] = "Updater",
        ["删除标志"] = "Delete flag",
        ["角色表"] = "Role table",
        ["菜单表"] = "Menu table",
        ["部门表"] = "Department table",
        ["字典表"] = "Dictionary table",
        ["日志表"] = "Log table",
        ["文件表"] = "File table",
        ["通知公告表"] = "Notice table",
        ["系统配置表"] = "System configuration table",
        ["岗位表"] = "Position table",
        ["操作日志表"] = "Operation log table",
        ["登录日志表"] = "Login log table",
        
        // Common Properties
        ["用户账号"] = "User account",
        ["用户昵称"] = "User nickname",
        ["用户邮箱"] = "User email",
        ["手机号码"] = "Phone number",
        ["用户性别"] = "User gender",
        ["用户头像"] = "User avatar",
        ["帐号状态"] = "Account status",
        ["最后登录IP"] = "Last login IP",
        ["最后登录时间"] = "Last login time",
        ["登录用户名"] = "Login username",
        ["角色名称"] = "Role name",
        ["角色权限"] = "Role permission",
        ["角色排序"] = "Role sort",
        ["数据范围"] = "Data scope",
        ["菜单名称"] = "Menu name",
        ["父菜单ID"] = "Parent menu ID",
        ["显示顺序"] = "Display order",
        ["菜单类型"] = "Menu type",
        ["菜单状态"] = "Menu status",
        ["菜单图标"] = "Menu icon",
        ["路由地址"] = "Route path",
        ["组件路径"] = "Component path",
        ["权限标识"] = "Permission identifier",
        ["是否缓存"] = "Is cached",
        ["是否显示"] = "Is visible",
        
        // Method Descriptions
        ["查询用户列表"] = "Query user list",
        ["添加用户"] = "Add user",
        ["修改用户"] = "Update user",
        ["删除用户"] = "Delete user",
        ["导入用户"] = "Import users",
        ["导出用户"] = "Export users",
        ["上传文件"] = "Upload file",
        ["下载文件"] = "Download file",
        ["批量上传文件"] = "Batch upload files",
        ["批量删除"] = "Batch delete",
        ["获取详情"] = "Get details",
        ["保存数据"] = "Save data",
        ["更新数据"] = "Update data",
        ["查询列表"] = "Query list",
        ["分页查询"] = "Paged query",
        ["导入数据"] = "Import data",
        ["导出数据"] = "Export data",
        ["重置密码"] = "Reset password",
        ["修改密码"] = "Change password",
        ["获取用户信息"] = "Get user information",
        ["退出登录"] = "Logout",
        ["用户登录"] = "User login",
        ["刷新token"] = "Refresh token",
        
        // Error Messages
        ["上传文件不能为空"] = "Upload file cannot be empty",
        ["上传失败"] = "Upload failed",
        ["文件类型不允许上传"] = "File type not allowed for upload",
        ["文件过大"] = "File too large",
        ["配置文件缺失"] = "Configuration file missing",
        ["阿里云连接失败"] = "Aliyun connection failed",
        ["操作成功"] = "Operation successful",
        ["操作失败"] = "Operation failed",
        ["参数错误"] = "Parameter error",
        ["系统错误"] = "System error",
        ["无权限"] = "No permission",
        ["没有权限"] = "No permission",
        ["登录失败"] = "Login failed",
        ["用户不存在"] = "User does not exist",
        ["密码错误"] = "Password incorrect",
        ["密码不正确"] = "Password incorrect",
        ["token已过期"] = "Token expired",
        ["Token已过期"] = "Token expired",
        ["演示模式，不允许操作"] = "Demo mode, operation not allowed",
        ["演示模式 , 不允许操作"] = "Demo mode, operation not allowed",
        ["账号已停用"] = "Account disabled",
        ["账号已锁定"] = "Account locked",
        ["验证码错误"] = "Verification code error",
        ["验证码已过期"] = "Verification code expired",
        ["请输入验证码"] = "Please enter verification code",
        ["用户名或密码错误"] = "Username or password incorrect",
        ["登录成功"] = "Login successful",
        ["退出成功"] = "Logout successful",
        ["注册成功"] = "Registration successful",
        ["修改成功"] = "Update successful",
        ["删除成功"] = "Delete successful",
        ["保存成功"] = "Save successful",
        ["添加成功"] = "Add successful",
        ["导入成功"] = "Import successful",
        ["导出成功"] = "Export successful",
        ["重置成功"] = "Reset successful",
        ["发送成功"] = "Send successful",
        ["数据不存在"] = "Data does not exist",
        ["数据已存在"] = "Data already exists",
        ["请选择要删除的数据"] = "Please select data to delete",
        ["请选择要导出的数据"] = "Please select data to export",
        ["无效的请求"] = "Invalid request",
        ["服务器内部错误"] = "Internal server error",
        ["网络异常"] = "Network error",
        ["数据库异常"] = "Database error",
        
        // File Related
        ["文件名生成类型"] = "File name generation type",
        ["原文件名"] = "Original file name",
        ["自定义"] = "Custom",
        ["自动生成"] = "Auto-generate",
        ["存储目录"] = "Storage directory",
        ["分类"] = "Category",
        ["压缩质量"] = "Compression quality",
        ["文件后缀"] = "File extension",
        ["文件大小KB"] = "File size in KB",
        ["文件大小"] = "File size",
        ["文件路径"] = "File path",
        ["文件类型"] = "File type",
        ["存储类型"] = "Storage type",
        ["本地存储"] = "Local storage",
        ["云存储"] = "Cloud storage",
        ["上传时间"] = "Upload time",
        ["上传人"] = "Uploader",
        
        // Business Terms
        ["企业消息测试"] = "Enterprise message test",
        ["发送的消息"] = "Message to send",
        ["接收人"] = "Recipient",
        ["消息类型"] = "Message type",
        ["通知公告"] = "Notice announcement",
        ["系统消息"] = "System message",
        ["待办事项"] = "Todo items",
        ["已完成"] = "Completed",
        ["未完成"] = "Not completed",
        ["进行中"] = "In progress",
        ["已取消"] = "Cancelled",
        ["优先级"] = "Priority",
        ["紧急"] = "Urgent",
        ["重要"] = "Important",
        ["普通"] = "Normal",
        ["开始时间"] = "Start time",
        ["结束时间"] = "End time",
        ["执行时间"] = "Execution time",
        
        // Code Comments
        ["返回结果"] = "Return result",
        ["查询条件"] = "Query conditions",
        ["分页参数"] = "Pagination parameters",
        ["获取用户信息"] = "Get user information",
        ["保存到本地"] = "Save to local",
        ["保存到阿里云"] = "Save to Aliyun",
        ["初始化数据"] = "Initialize data",
        ["清理数据"] = "Clean data",
        ["缓存处理"] = "Cache processing",
        ["异常处理"] = "Exception handling",
        ["参数验证"] = "Parameter validation",
        ["权限验证"] = "Permission validation",
        ["数据验证"] = "Data validation",
        ["事务处理"] = "Transaction processing",
        ["日志记录"] = "Logging",
        ["定时任务"] = "Scheduled task",
        
        // Console Messages
        ["📖官方文档："] = "📖 Official Documentation: ",
        ["💰打赏作者："] = "💰 Support Author: ",
        ["启动成功"] = "Started successfully",
        ["启动失败"] = "Start failed",
        ["正在启动"] = "Starting",
        ["正在停止"] = "Stopping",
        ["已停止"] = "Stopped",
        ["正在运行"] = "Running",
        ["配置加载成功"] = "Configuration loaded successfully",
        ["数据库连接成功"] = "Database connected successfully",
        ["数据库连接失败"] = "Database connection failed",
        
        // Additional Terms
        ["是"] = "Yes",
        ["否"] = "No",
        ["启用"] = "Enable",
        ["禁用"] = "Disable",
        ["显示"] = "Show",
        ["隐藏"] = "Hide",
        ["全部"] = "All",
        ["部分"] = "Partial",
        ["默认"] = "Default",
        ["自定义"] = "Custom",
        ["系统"] = "System",
        ["用户"] = "User",
        ["管理员"] = "Administrator",
        ["访客"] = "Guest",
        ["只读"] = "Read-only",
        ["读写"] = "Read-write",
        ["公开"] = "Public",
        ["私有"] = "Private",
        ["内部"] = "Internal",
        ["外部"] = "External"
    };

    public static void Main(string[] args)
    {
        string rootPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        
        Console.WriteLine($"Starting translation in: {rootPath}");
        
        var csFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
        int totalFiles = csFiles.Length;
        int modifiedFiles = 0;
        
        Console.WriteLine($"Found {totalFiles} .cs files");
        
        foreach (var file in csFiles)
        {
            if (TranslateFile(file))
            {
                modifiedFiles++;
                Console.WriteLine($"✓ Translated: {Path.GetRelativePath(rootPath, file)}");
            }
        }
        
        Console.WriteLine($"\nTranslation complete!");
        Console.WriteLine($"Files processed: {totalFiles}");
        Console.WriteLine($"Files modified: {modifiedFiles}");
    }
    
    private static bool TranslateFile(string filePath)
    {
        try
        {
            string content = File.ReadAllText(filePath, Encoding.UTF8);
            string originalContent = content;
            
            // Translate strings in quotes
            foreach (var kvp in TranslationMap)
            {
                // Translate in double quotes
                content = content.Replace($"\"{kvp.Key}\"", $"\"{kvp.Value}\"");
                // Translate in XML comments
                content = Regex.Replace(content, 
                    $@"(///\s*<summary>\s*)([\s\S]*?)({Regex.Escape(kvp.Key)})([\s\S]*?)(\s*</summary>)",
                    $"$1$2{kvp.Value}$4$5");
                content = Regex.Replace(content,
                    $@"(///\s*<param[^>]*>)([\s\S]*?)({Regex.Escape(kvp.Key)})([\s\S]*?)(</param>)",
                    $"$1$2{kvp.Value}$4$5");
                content = Regex.Replace(content,
                    $@"(///\s*<returns>)([\s\S]*?)({Regex.Escape(kvp.Key)})([\s\S]*?)(</returns>)",
                    $"$1$2{kvp.Value}$4$5");
                // Translate in single line XML comments
                content = Regex.Replace(content,
                    $@"(///\s*)({Regex.Escape(kvp.Key)})",
                    $"$1{kvp.Value}");
                // Translate in regular comments
                content = Regex.Replace(content,
                    $@"(//\s*)({Regex.Escape(kvp.Key)})",
                    $"$1{kvp.Value}");
                // Translate in attributes
                content = Regex.Replace(content,
                    $@"(ColumnDescription\s*=\s*"")({Regex.Escape(kvp.Key)})("")",
                    $"$1{kvp.Value}$3");
                content = Regex.Replace(content,
                    $@"(\[SugarTable\([^,]+,\s*"")({Regex.Escape(kvp.Key)})("")",
                    $"$1{kvp.Value}$3");
                content = Regex.Replace(content,
                    $@"(\[Log\(Title\s*=\s*"")({Regex.Escape(kvp.Key)})("")",
                    $"$1{kvp.Value}$3");
            }
            
            if (content != originalContent)
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {filePath}: {ex.Message}");
            return false;
        }
    }
}
