@echo off

for /f "tokens=5" %%a in ('netstat -aon ^| findstr "5000" ') do @taskkill /F /PID  %%a
for /f "tokens=5" %%a in ('netstat -aon ^| findstr "5001" ') do @taskkill /F /PID  %%a