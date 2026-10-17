using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

class TranslateExcel
{
    static Dictionary<string, string> translations = new Dictionary<string, string>
    {
        // Menu and Navigation
        {"菜单管理", "Menu Management"},
        {"用户管理", "User Management"},
        {"角色管理", "Role Management"},
        {"部门管理", "Department Management"},
        {"岗位管理", "Position Management"},
        {"字典管理", "Dictionary Management"},
        {"参数设置", "Parameter Settings"},
        {"通知公告", "Notice Announcement"},
        {"日志管理", "Log Management"},
        {"系统监控", "System Monitoring"},
        {"系统工具", "System Tools"},
        {"定时任务", "Scheduled Tasks"},
        {"代码生成", "Code Generation"},
        {"系统接口", "System Interface"},
        {"服务监控", "Service Monitoring"},
        {"缓存监控", "Cache Monitoring"},
        {"在线用户", "Online Users"},
        {"登录日志", "Login Log"},
        {"操作日志", "Operation Log"},
        {"字典类型", "Dictionary Type"},
        {"字典数据", "Dictionary Data"},
        {"文章管理", "Article Management"},
        {"文章列表", "Article List"},
        {"发布文章", "Publish Article"},
        {"文件存储", "File Storage"},
        {"通讯录", "Address Book"},
        {"多语言配置", "Multi-language Configuration"},
        {"发送邮件", "Send Email"},
        {"短信管理", "SMS Management"},
        {"员工管理", "Employee Management"},
        {"个人中心", "Personal Center"},
        {"个人信息", "Personal Information"},
        {"修改密码", "Change Password"},
        {"企业信息", "Company Info"},
        {"薪资管理", "Salary Management"},
        {"薪资表模板", "Salary Template"},
        {"薪资记录", "Salary Records"},
        
        // Actions
        {"导出", "Export"},
        {"导入", "Import"},
        {"查询", "Query"},
        {"新增", "Add"},
        {"修改", "Edit"},
        {"删除", "Delete"},
        {"查看", "View"},
        {"搜索", "Search"},
        {"重置", "Reset"},
        {"提交", "Submit"},
        {"取消", "Cancel"},
        {"确定", "Confirm"},
        {"清空", "Clear"},
        {"详情", "Details"},
        {"刷新", "Refresh"},
        {"保存", "Save"},
        {"下载", "Download"},
        {"上传", "Upload"},
        {"批量删除", "Batch Delete"},
        {"批量导入", "Batch Import"},
        {"批量导出", "Batch Export"},
        
        // Common Fields
        {"状态", "Status"},
        {"备注", "Remark"},
        {"创建时间", "Create Time"},
        {"更新时间", "Update Time"},
        {"操作", "Operation"},
        {"编号", "ID"},
        {"序号", "Serial No"},
        {"名称", "Name"},
        {"类型", "Type"},
        {"排序", "Sort"},
        {"描述", "Description"},
        {"说明", "Explanation"},
        {"内容", "Content"},
        {"标题", "Title"},
        {"地址", "Address"},
        {"电话", "Phone"},
        {"邮箱", "Email"},
        {"手机", "Mobile"},
        {"传真", "Fax"},
        {"邮编", "Postal Code"},
        {"网址", "Website"},
        {"开始时间", "Start Time"},
        {"结束时间", "End Time"},
        {"创建人", "Creator"},
        {"修改人", "Modifier"},
        {"审核人", "Reviewer"},
        {"审核时间", "Review Time"},
        {"发布时间", "Publish Time"},
        
        // Status Values
        {"是", "Yes"},
        {"否", "No"},
        {"正常", "Normal"},
        {"停用", "Disabled"},
        {"启用", "Enabled"},
        {"禁用", "Disabled"},
        {"显示", "Show"},
        {"隐藏", "Hide"},
        {"成功", "Success"},
        {"失败", "Failed"},
        {"待审核", "Pending Review"},
        {"已审核", "Reviewed"},
        {"已发布", "Published"},
        {"草稿", "Draft"},
        {"已删除", "Deleted"},
        {"进行中", "In Progress"},
        {"已完成", "Completed"},
        {"未开始", "Not Started"},
        
        // Menu Related
        {"父级", "Parent"},
        {"子级", "Child"},
        {"路径", "Path"},
        {"组件", "Component"},
        {"权限", "Permission"},
        {"权限标识", "Permission Code"},
        {"图标", "Icon"},
        {"可见", "Visible"},
        {"路由", "Route"},
        {"重定向", "Redirect"},
        {"目录", "Directory"},
        {"菜单", "Menu"},
        {"按钮", "Button"},
        {"链接", "Link"},
        
        // System Related
        {"系统", "System"},
        {"业务", "Business"},
        {"其他", "Other"},
        {"管理", "Management"},
        {"监控", "Monitoring"},
        {"工具", "Tools"},
        {"配置", "Configuration"},
        {"设置", "Settings"},
        {"列表", "List"},
        {"数据", "Data"},
        {"信息", "Information"},
        {"记录", "Record"},
        {"文件", "File"},
        {"版本", "Version"},
        {"更新", "Update"},
        {"维护", "Maintenance"},
        
        // User and Auth
        {"用户", "User"},
        {"用户名", "Username"},
        {"密码", "Password"},
        {"昵称", "Nickname"},
        {"真实姓名", "Real Name"},
        {"性别", "Gender"},
        {"男", "Male"},
        {"女", "Female"},
        {"角色", "Role"},
        {"部门", "Department"},
        {"岗位", "Position"},
        {"职位", "Job Title"},
        {"权限组", "Permission Group"},
        {"登录", "Login"},
        {"注册", "Register"},
        {"登出", "Logout"},
        {"记住密码", "Remember Password"},
        {"忘记密码", "Forgot Password"},
        {"验证码", "Verification Code"},
        
        // Organization
        {"公司", "Company"},
        {"企业", "Enterprise"},
        {"组织", "Organization"},
        {"机构", "Institution"},
        {"分公司", "Branch"},
        {"子公司", "Subsidiary"},
        {"部门", "Department"},
        {"团队", "Team"},
        {"小组", "Group"},
        {"负责人", "Manager"},
        {"领导", "Leader"},
        {"主管", "Supervisor"},
        {"员工", "Employee"},
        {"职员", "Staff"},
        
        // Common Words
        {"首页", "Home"},
        {"仪表盘", "Dashboard"},
        {"控制台", "Console"},
        {"工作台", "Workspace"},
        {"概览", "Overview"},
        {"统计", "Statistics"},
        {"分析", "Analysis"},
        {"报表", "Report"},
        {"图表", "Chart"},
        {"导航", "Navigation"},
        {"菜单", "Menu"},
        {"侧边栏", "Sidebar"},
        {"头部", "Header"},
        {"底部", "Footer"},
        {"主要", "Main"},
        {"次要", "Secondary"},
        {"默认", "Default"},
        {"自定义", "Custom"},
        {"模板", "Template"},
        {"示例", "Example"},
        {"测试", "Test"},
        {"生产", "Production"},
        {"开发", "Development"},
        {"调试", "Debug"},
        
        // Date and Time
        {"今天", "Today"},
        {"昨天", "Yesterday"},
        {"明天", "Tomorrow"},
        {"本周", "This Week"},
        {"上周", "Last Week"},
        {"下周", "Next Week"},
        {"本月", "This Month"},
        {"上月", "Last Month"},
        {"下月", "Next Month"},
        {"本年", "This Year"},
        {"去年", "Last Year"},
        {"明年", "Next Year"},
        {"日期", "Date"},
        {"时间", "Time"},
        {"年", "Year"},
        {"月", "Month"},
        {"日", "Day"},
        {"时", "Hour"},
        {"分", "Minute"},
        {"秒", "Second"},
        
        // Numbers and Counts
        {"总计", "Total"},
        {"合计", "Sum"},
        {"数量", "Quantity"},
        {"数目", "Number"},
        {"金额", "Amount"},
        {"价格", "Price"},
        {"费用", "Cost"},
        {"折扣", "Discount"},
        {"税率", "Tax Rate"},
        {"百分比", "Percentage"},
        {"比例", "Ratio"},
        {"平均", "Average"},
        {"最大", "Maximum"},
        {"最小", "Minimum"},
        
        // File Operations
        {"文件名", "File Name"},
        {"文件类型", "File Type"},
        {"文件大小", "File Size"},
        {"上传文件", "Upload File"},
        {"下载文件", "Download File"},
        {"删除文件", "Delete File"},
        {"文件路径", "File Path"},
        {"文件夹", "Folder"},
        {"目录", "Directory"},
        {"压缩", "Compress"},
        {"解压", "Decompress"},
        {"复制", "Copy"},
        {"粘贴", "Paste"},
        {"剪切", "Cut"},
        {"重命名", "Rename"},
        
        // Messages and Notifications
        {"消息", "Message"},
        {"通知", "Notification"},
        {"提示", "Tip"},
        {"警告", "Warning"},
        {"错误", "Error"},
        {"成功", "Success"},
        {"信息", "Information"},
        {"确认", "Confirmation"},
        {"提醒", "Reminder"},
        {"公告", "Announcement"},
        {"邮件", "Email"},
        {"短信", "SMS"},
        {"站内信", "Internal Message"},
        
        // Business Terms
        {"订单", "Order"},
        {"客户", "Customer"},
        {"供应商", "Supplier"},
        {"产品", "Product"},
        {"商品", "Goods"},
        {"库存", "Inventory"},
        {"销售", "Sales"},
        {"采购", "Purchase"},
        {"财务", "Finance"},
        {"会计", "Accounting"},
        {"审计", "Audit"},
        {"报销", "Reimbursement"},
        {"发票", "Invoice"},
        {"合同", "Contract"},
        {"项目", "Project"},
        {"任务", "Task"},
        {"计划", "Plan"},
        {"进度", "Progress"},
        {"里程碑", "Milestone"},
        {"交付", "Delivery"},
        {"验收", "Acceptance"},
        
        // Additional Common Terms
        {"选择", "Select"},
        {"全选", "Select All"},
        {"取消选择", "Deselect"},
        {"反选", "Inverse Selection"},
        {"展开", "Expand"},
        {"收起", "Collapse"},
        {"更多", "More"},
        {"返回", "Back"},
        {"前进", "Forward"},
        {"帮助", "Help"},
        {"关于", "About"},
        {"退出", "Exit"},
        {"关闭", "Close"},
        {"打开", "Open"},
        {"新建", "New"},
        {"编辑", "Edit"},
        {"预览", "Preview"},
        {"打印", "Print"},
        {"导航菜单", "Navigation Menu"},
        {"面包屑", "Breadcrumb"},
        {"分页", "Pagination"},
        {"每页", "Per Page"},
        {"共", "Total"},
        {"条", "Items"},
        {"第", "Page"},
        {"页", "Page"},
        {"跳转", "Jump"},
        {"确认删除", "Confirm Delete"},
        {"操作成功", "Operation Successful"},
        {"操作失败", "Operation Failed"},
        {"请输入", "Please Enter"},
        {"请选择", "Please Select"},
        {"必填", "Required"},
        {"选填", "Optional"},
        {"格式", "Format"},
        {"示例", "Example"},
        {"说明", "Description"},
        {"附件", "Attachment"},
        {"链接", "Link"},
        {"地址", "Address"},
        {"标签", "Tag"},
        {"分类", "Category"},
        {"排序", "Sort"},
        {"筛选", "Filter"},
        {"高级搜索", "Advanced Search"},
        {"快速搜索", "Quick Search"},
        {"批量操作", "Batch Operation"},
        {"导入数据", "Import Data"},
        {"导出数据", "Export Data"},
        {"数据备份", "Data Backup"},
        {"数据恢复", "Data Recovery"},
        {"系统设置", "System Settings"},
        {"个人设置", "Personal Settings"},
        {"安全设置", "Security Settings"},
        {"主题设置", "Theme Settings"},
        {"语言设置", "Language Settings"},
        {"时区设置", "Timezone Settings"},
        {"通知设置", "Notification Settings"},
        {"隐私设置", "Privacy Settings"},
        {"账号绑定", "Account Binding"},
        {"第三方登录", "Third-party Login"},
        {"二维码", "QR Code"},
        {"扫码", "Scan"},
        {"分享", "Share"},
        {"收藏", "Favorite"},
        {"点赞", "Like"},
        {"评论", "Comment"},
        {"回复", "Reply"},
        {"转发", "Forward"},
        {"举报", "Report"},
        {"黑名单", "Blacklist"},
        {"白名单", "Whitelist"},
        {"权限不足", "Insufficient Permission"},
        {"访问被拒绝", "Access Denied"},
        {"页面不存在", "Page Not Found"},
        {"服务器错误", "Server Error"},
        {"网络错误", "Network Error"},
        {"超时", "Timeout"},
        {"重试", "Retry"},
        {"加载中", "Loading"},
        {"加载更多", "Load More"},
        {"没有更多数据", "No More Data"},
        {"暂无数据", "No Data"},
        {"敬请期待", "Coming Soon"},
        {"正在开发中", "Under Development"},
        {"维护中", "Under Maintenance"},
        {"即将上线", "Coming Online Soon"}
    };

    static void Main(string[] args)
    {
        string inputFile = @"F:\mahamBackendNET\backend\ZR.Admin.WebApi\wwwroot\data.xlsx";
        string outputFile = @"F:\mahamBackendNET\backend\ZR.Admin.WebApi\wwwroot\data_english.xlsx";
        
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        try
        {
            Console.WriteLine($"Opening Excel file: {inputFile}");
            
            FileInfo existingFile = new FileInfo(inputFile);
            FileInfo newFile = new FileInfo(outputFile);
            
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                Console.WriteLine($"Number of worksheets: {package.Workbook.Worksheets.Count}");
                
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    Console.WriteLine($"\nProcessing worksheet: {worksheet.Name}");
                    
                    if (worksheet.Dimension == null)
                    {
                        Console.WriteLine("  Worksheet is empty, skipping...");
                        continue;
                    }
                    
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    
                    Console.WriteLine($"  Rows: {rowCount}, Columns: {colCount}");
                    
                    for (int row = 1; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            var cell = worksheet.Cells[row, col];
                            if (cell.Value != null)
                            {
                                string originalValue = cell.Value.ToString();
                                string translatedValue = TranslateText(originalValue);
                                
                                if (originalValue != translatedValue)
                                {
                                    cell.Value = translatedValue;
                                    Console.WriteLine($"  Translated [{row},{col}]: '{originalValue}' -> '{translatedValue}'");
                                }
                            }
                        }
                    }
                }
                
                // Save to new file
                package.SaveAs(newFile);
                Console.WriteLine($"\nTranslated file saved to: {outputFile}");
            }
            
            Console.WriteLine("\nTranslation completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    static string TranslateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;
        
        // Try exact match first
        if (translations.ContainsKey(text))
            return translations[text];
        
        // Try to translate parts of the text
        string result = text;
        foreach (var kvp in translations)
        {
            if (result.Contains(kvp.Key))
            {
                result = result.Replace(kvp.Key, kvp.Value);
            }
        }
        
        return result;
    }
}
