# PowerShell script to translate Chinese comments in .cs files
# This script will find and help translate all Chinese text in C# files

$translationMap = @{
    # Common XML comments
    '/// 用户表' = '/// User table'
    '/// 用户id' = '/// User ID'
    '/// 用户名' = '/// Username'
    '/// 密码' = '/// Password'
    '/// 创建时间' = '/// Create time'
    '/// 更新时间' = '/// Update time'
    '/// 删除时间' = '/// Delete time'
    '/// 备注' = '/// Remark'
    '/// 状态' = '/// Status'
    '/// 排序' = '/// Sort order'
    '/// 是否删除' = '/// Is deleted'
    '/// 创建人' = '/// Creator'
    '/// 更新人' = '/// Updater'
    '/// 删除标志' = '/// Delete flag'
    
    # Common error messages
    '"上传文件不能为空"' = '"Upload file cannot be empty"'
    '"上传失败"' = '"Upload failed"'
    '"文件类型不允许上传"' = '"File type not allowed for upload"'
    '"文件过大"' = '"File too large"'
    '"配置文件缺失"' = '"Configuration file missing"'
    '"阿里云连接失败"' = '"Aliyun connection failed"'
    '"操作成功"' = '"Operation successful"'
    '"操作失败"' = '"Operation failed"'
    '"参数错误"' = '"Parameter error"'
    '"系统错误"' = '"System error"'
    '"无权限"' = '"No permission"'
    '"登录失败"' = '"Login failed"'
    '"用户不存在"' = '"User does not exist"'
    '"密码错误"' = '"Password incorrect"'
    '"token已过期"' = '"Token expired"'
    '"演示模式，不允许操作"' = '"Demo mode, operation not allowed"'
    
    # Common code comments
    '//文件后缀' = '//File extension'
    '//文件大小KB' = '//File size in KB'
    '//文件大小' = '//File size'
    '//返回结果' = '//Return result'
    '//查询条件' = '//Query conditions'
    '//分页参数' = '//Pagination parameters'
    '//获取用户信息' = '//Get user information'
    '//保存到本地' = '//Save to local'
    '//保存到阿里云' = '//Save to Aliyun'
    
    # Database descriptions
    '"用户表"' = '"User table"'
    '"角色表"' = '"Role table"'
    '"菜单表"' = '"Menu table"'
    '"部门表"' = '"Department table"'
    '"字典表"' = '"Dictionary table"'
    '"日志表"' = '"Log table"'
    '"文件表"' = '"File table"'
    '"通知公告表"' = '"Notice table"'
    
    # Common properties
    '用户账号' = 'User account'
    '用户昵称' = 'User nickname'
    '用户邮箱' = 'User email'
    '手机号码' = 'Phone number'
    '用户性别' = 'User gender'
    '用户头像' = 'User avatar'
    '帐号状态' = 'Account status'
    '最后登录IP' = 'Last login IP'
    '最后登录时间' = 'Last login time'
    
    # Method descriptions
    '查询用户列表' = 'Query user list'
    '添加用户' = 'Add user'
    '修改用户' = 'Update user'
    '删除用户' = 'Delete user'
    '导入用户' = 'Import users'
    '导出用户' = 'Export users'
    '上传文件' = 'Upload file'
    '下载文件' = 'Download file'
    '批量上传文件' = 'Batch upload files'
    
    # Business terms
    '文件名生成类型' = 'File name generation type'
    '原文件名' = 'Original file name'
    '自定义' = 'Custom'
    '自动生成' = 'Auto-generate'
    '存储目录' = 'Storage directory'
    '分类' = 'Category'
    '压缩质量' = 'Compression quality'
}

function Translate-ChineseInFile {
    param (
        [string]$FilePath
    )
    
    $content = Get-Content -Path $FilePath -Encoding UTF8
    $modified = $false
    $newContent = @()
    
    foreach ($line in $content) {
        $newLine = $line
        foreach ($key in $translationMap.Keys) {
            if ($newLine -match [regex]::Escape($key)) {
                $newLine = $newLine -replace [regex]::Escape($key), $translationMap[$key]
                $modified = $true
            }
        }
        $newContent += $newLine
    }
    
    if ($modified) {
        Set-Content -Path $FilePath -Value $newContent -Encoding UTF8
        Write-Host "Translated: $FilePath" -ForegroundColor Green
        return $true
    }
    return $false
}

# Get all .cs files
$csFiles = Get-ChildItem -Path . -Filter "*.cs" -Recurse

$totalFiles = $csFiles.Count
$translatedFiles = 0

Write-Host "Starting translation of $totalFiles .cs files..." -ForegroundColor Cyan

foreach ($file in $csFiles) {
    if (Translate-ChineseInFile -FilePath $file.FullName) {
        $translatedFiles++
    }
}

Write-Host "`nTranslation complete!" -ForegroundColor Green
Write-Host "Files processed: $totalFiles" -ForegroundColor Yellow
Write-Host "Files modified: $translatedFiles" -ForegroundColor Yellow