# How to Check Test Results - دليل التحقق من نتائج الاختبارات

## Quick Guide - دليل سريع

### ✅ **Test Results are OK when you see:**
- `Passed!` - All tests passed
- `Total tests: X, Passed: X, Failed: 0`
- Exit code: `0`
- No red error messages

### ❌ **Test Results are NOT OK when you see:**
- `Failed!` - Some tests failed
- `Total tests: X, Passed: Y, Failed: Z` (where Z > 0)
- Exit code: `1` or non-zero
- Red error messages with stack traces

---

## Method 1: Using Command Line (PowerShell/CMD)

### Run All Tests
```powershell
cd "c:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Service.Tests"
dotnet test
```

### Check the Last Line
Look for the summary at the end:
```
Test Run Successful.
Total tests: 150
     Passed: 150
     Failed: 0
```

### Run with Detailed Output
```powershell
dotnet test --verbosity normal
```

This shows:
- ✅ `Passed!` for each passing test
- ❌ `Failed!` for each failing test with error details

### Run Specific Test Category
```powershell
# Run only Finance tests
dotnet test --filter "FullyQualifiedName~Finance"

# Run only Currency tests
dotnet test --filter "FullyQualifiedName~Currency"
```

---

## Method 2: Using Visual Studio

1. **Open Test Explorer**
   - Go to: `Test` → `Test Explorer` (or press `Ctrl+E, T`)

2. **Run Tests**
   - Click `Run All Tests` button
   - Or right-click on a test class/method → `Run Tests`

3. **Check Results**
   - ✅ Green checkmark = Passed
   - ❌ Red X = Failed
   - ⚠️ Yellow warning = Skipped

4. **View Details**
   - Click on a failed test to see error message
   - Check the "Output" window for detailed logs

---

## Method 3: Using VS Code

1. **Install .NET Test Explorer Extension**
   - Open Extensions (Ctrl+Shift+X)
   - Search for "`.NET Test Explorer`"
   - Install it

2. **Run Tests**
   - Open Command Palette (Ctrl+Shift+P)
   - Type: `Test: Run All Tests`

3. **Check Results**
   - Look at the Test Explorer panel
   - Green = Passed, Red = Failed

---

## Method 4: Check Exit Code

### In PowerShell:
```powershell
dotnet test
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed!" -ForegroundColor Red
}
```

### In CMD:
```cmd
dotnet test
if %ERRORLEVEL% EQU 0 (
    echo ✅ All tests passed!
) else (
    echo ❌ Some tests failed!
)
```

---

## Understanding Test Output

### ✅ Success Example:
```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   150, Skipped:     0, Total:   150, Duration: 5 s
```

**Meaning**: All 150 tests passed! ✅

### ❌ Failure Example:
```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Failed!  - Failed:     2, Passed:   148, Skipped:     0, Total:   150, Duration: 5 s

Failed tests:
  X CreateAccount_WithInvalidType_ShouldFail
    Error Message: Expected exception but none was thrown
  X PostVoucher_WithUnbalancedEntries_ShouldFail
    Error Message: Assert.Throws failed. No exception was thrown.
```

**Meaning**: 2 tests failed, 148 passed ❌

---

## Detailed Test Results

### Get Detailed Report
```powershell
dotnet test --logger "trx;LogFileName=test-results.trx"
```

This creates a `test-results.trx` file that can be opened in Visual Studio.

### Get HTML Report
```powershell
dotnet test --logger "html;LogFileName=test-results.html"
```

Open `test-results.html` in a browser for a visual report.

### Get JSON Report
```powershell
dotnet test --logger "json;LogFileName=test-results.json"
```

---

## Common Test Result Indicators

### Exit Codes:
- `0` = Success (all tests passed)
- `1` = Failure (some tests failed)
- `-1` = Error (test execution error)

### Status Messages:
- `Passed!` = All tests passed ✅
- `Failed!` = Some tests failed ❌
- `No test is available` = No tests found ⚠️

### Test Status:
- `Passed` = Test succeeded ✅
- `Failed` = Test failed ❌
- `Skipped` = Test was skipped ⏭️

---

## Quick Check Commands

### Check if tests pass (silent mode):
```powershell
dotnet test --verbosity quiet
echo "Exit code: $LASTEXITCODE"
```

### Count passed/failed tests:
```powershell
dotnet test --verbosity normal | Select-String -Pattern "(Passed|Failed):"
```

### Run tests and show only failures:
```powershell
dotnet test --verbosity normal | Select-String -Pattern "Failed" -Context 5
```

---

## Troubleshooting

### If tests don't run:
1. Check if project builds: `dotnet build`
2. Check if test project references are correct
3. Verify test framework is installed: `dotnet add package xunit`

### If tests fail:
1. Read the error message carefully
2. Check the stack trace for line numbers
3. Verify test data setup
4. Check if database tables are created correctly

### If tests are slow:
1. Check if using in-memory database (should be fast)
2. Look for blocking operations
3. Consider running tests in parallel: `dotnet test --parallel`

---

## Example: Complete Test Run

```powershell
PS> dotnet test

Build started...
Build succeeded.
    0 Warning(s)
    0 Error(s)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   150, Skipped:     0, Total:   150, Duration: 5 s

Test Run Successful.
Total tests: 150
     Passed: 150
     Failed: 0
     Total time: 5.2345 Seconds
```

**Result**: ✅ **ALL TESTS PASSED!**

---

## Summary

### ✅ **Good Signs:**
- Exit code = 0
- "Passed!" message
- Failed: 0
- No red error text

### ❌ **Bad Signs:**
- Exit code ≠ 0
- "Failed!" message
- Failed: > 0
- Red error messages with stack traces

### Quick Check:
```powershell
dotnet test
# If you see "Passed!" and exit code is 0 → ✅ OK
# If you see "Failed!" or exit code is not 0 → ❌ NOT OK
```





























