# PowerShell script to translate Excel file content from Chinese to English
# Using COM object to work with Excel

$excelPath = "F:\mahamBackendNET\backend\ZR.Admin.WebApi\wwwroot\data.xlsx"
$outputPath = "F:\mahamBackendNET\backend\ZR.Admin.WebApi\wwwroot\data_english.xlsx"

# Translation dictionary for common Chinese terms
$translations = @{
    "菜单管理" = "Menu Management"
    "用户管理" = "User Management"
    "角色管理" = "Role Management"
    "部门管理" = "Department Management"
    "岗位管理" = "Position Management"
    "字典管理" = "Dictionary Management"
    "参数设置" = "Parameter Settings"
    "通知公告" = "Notice Announcement"
    "日志管理" = "Log Management"
    "系统监控" = "System Monitoring"
    "系统工具" = "System Tools"
    "定时任务" = "Scheduled Tasks"
    "代码生成" = "Code Generation"
    "系统接口" = "System Interface"
    "服务监控" = "Service Monitoring"
    "缓存监控" = "Cache Monitoring"
    "在线用户" = "Online Users"
    "登录日志" = "Login Log"
    "操作日志" = "Operation Log"
    "字典类型" = "Dictionary Type"
    "字典数据" = "Dictionary Data"
    "文章管理" = "Article Management"
    "文章列表" = "Article List"
    "发布文章" = "Publish Article"
    "文件存储" = "File Storage"
    "通讯录" = "Address Book"
    "多语言配置" = "Multi-language Configuration"
    "发送邮件" = "Send Email"
    "短信管理" = "SMS Management"
    "员工管理" = "Employee Management"
    "个人中心" = "Personal Center"
    "个人信息" = "Personal Information"
    "个人信息" = "Personal Info"
    "修改密码" = "Change Password"
    "企业信息" = "Company Info"
    "薪资管理" = "Salary Management"
    "薪资表模板" = "Salary Template"
    "薪资记录" = "Salary Records"
    "导出" = "Export"
    "导入" = "Import"
    "查询" = "Query"
    "新增" = "Add"
    "修改" = "Edit"
    "删除" = "Delete"
    "查看" = "View"
    "搜索" = "Search"
    "重置" = "Reset"
    "提交" = "Submit"
    "取消" = "Cancel"
    "确定" = "Confirm"
    "清空" = "Clear"
    "详情" = "Details"
    "状态" = "Status"
    "备注" = "Remark"
    "创建时间" = "Create Time"
    "更新时间" = "Update Time"
    "操作" = "Operation"
    "编号" = "ID"
    "名称" = "Name"
    "类型" = "Type"
    "排序" = "Sort"
    "是" = "Yes"
    "否" = "No"
    "正常" = "Normal"
    "停用" = "Disabled"
    "显示" = "Show"
    "隐藏" = "Hide"
    "父级" = "Parent"
    "子级" = "Child"
    "路径" = "Path"
    "组件" = "Component"
    "权限" = "Permission"
    "图标" = "Icon"
    "可见" = "Visible"
    "系统" = "System"
    "业务" = "Business"
    "其他" = "Other"
    "管理" = "Management"
    "监控" = "Monitoring"
    "工具" = "Tools"
    "配置" = "Configuration"
    "设置" = "Settings"
    "列表" = "List"
    "数据" = "Data"
    "信息" = "Information"
    "记录" = "Record"
    "文件" = "File"
    "用户" = "User"
    "角色" = "Role"
    "部门" = "Department"
    "岗位" = "Position"
    "菜单" = "Menu"
    "字典" = "Dictionary"
    "参数" = "Parameter"
    "通知" = "Notice"
    "公告" = "Announcement"
    "日志" = "Log"
    "任务" = "Task"
    "接口" = "Interface"
    "服务" = "Service"
    "缓存" = "Cache"
    "在线" = "Online"
    "登录" = "Login"
    "操作" = "Operation"
    "文章" = "Article"
    "发布" = "Publish"
    "存储" = "Storage"
    "通讯" = "Contact"
    "语言" = "Language"
    "邮件" = "Email"
    "短信" = "SMS"
    "员工" = "Employee"
    "个人" = "Personal"
    "密码" = "Password"
    "企业" = "Company"
    "薪资" = "Salary"
    "模板" = "Template"
    "表" = "Table"
    "首页" = "Home"
    "仪表盘" = "Dashboard"
    "外链" = "External Link"
    "布局" = "Layout"
}

try {
    # Create Excel COM object
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    
    # Open the workbook
    $workbook = $excel.Workbooks.Open($excelPath)
    
    Write-Host "Opened Excel file: $excelPath"
    Write-Host "Number of sheets: $($workbook.Sheets.Count)"
    
    # Process each worksheet
    foreach ($sheet in $workbook.Sheets) {
        Write-Host "`nProcessing sheet: $($sheet.Name)"
        
        $usedRange = $sheet.UsedRange
        if ($null -ne $usedRange) {
            $rowCount = $usedRange.Rows.Count
            $colCount = $usedRange.Columns.Count
            
            Write-Host "Rows: $rowCount, Columns: $colCount"
            
            # Iterate through all cells
            for ($row = 1; $row -le $rowCount; $row++) {
                for ($col = 1; $col -le $colCount; $col++) {
                    $cell = $sheet.Cells($row, $col)
                    $cellValue = $cell.Text
                    
                    if (![string]::IsNullOrWhiteSpace($cellValue)) {
                        # Try to translate using dictionary
                        $translated = $cellValue
                        foreach ($key in $translations.Keys) {
                            if ($cellValue -contains $key -or $cellValue -eq $key) {
                                $translated = $cellValue.Replace($key, $translations[$key])
                            }
                        }
                        
                        # Update cell if translation occurred
                        if ($translated -ne $cellValue) {
                            $cell.Value = $translated
                            Write-Host "Translated: '$cellValue' -> '$translated' at [$row,$col]"
                        }
                    }
                }
            }
        }
    }
    
    # Save as new file
    $workbook.SaveAs($outputPath)
    Write-Host "`nSaved translated file to: $outputPath"
    
    # Close and cleanup
    $workbook.Close()
    $excel.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    Remove-Variable excel
    
    Write-Host "`nTranslation completed successfully!"
}
catch {
    Write-Error "Error occurred: $_"
    if ($excel) {
        $excel.Quit()
        [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    }
}